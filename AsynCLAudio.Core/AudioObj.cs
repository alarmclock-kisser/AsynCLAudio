using NAudio.Flac;
using NAudio.Wave;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AsynCLAudio.Core
{
	public class AudioObj : IDisposable
	{
		public Guid Id { get; private set; } = Guid.Empty;
		public DateTime CreatedAt { get; private set; } = DateTime.Now;
		public string FilePath { get; set; } = string.Empty;
		public string? ExportedFile { get; private set; } = null;
		public string Name => (this.IsProcessing ? "~Processing~ " : "") + ((this.player.PlaybackState == PlaybackState.Playing) ? "▶ " : this.player.PlaybackState == PlaybackState.Paused ? "|| " : "") + Path.GetFileNameWithoutExtension(this.FilePath);

		public float[] Data { get; private set; } = [];
		private int originalSampleRate = 0;
		public int SampleRate
		{ 
			get => this.originalSampleRate > 0 ? this.originalSampleRate : 44100;
			set
			{
				this.originalSampleRate = value > 0 ? value : 44100;
			}
		}
		public int Channels { get; private set; } = 0;
		public int BitDepth { get; private set; } = 0;
		public long Length => this.Data.LongLength;
		public double TotalSeconds => (this.SampleRate > 0 && this.Channels > 0) ? (double) this.Length / (this.SampleRate * this.Channels) : 0;
		public TimeSpan Duration => TimeSpan.FromSeconds(this.TotalSeconds);
		public float SizeInMb => this.Data.LongLength * sizeof(float) / (1024.0f * 1024.0f);

		public bool IsProcessing { get; set; } = false;
		public float ElapsedLoadingTime { get; set; } = 0.0f;
		public float ElapsedProcessingTime { get; set; } = 0.0f;

		public bool OnHost => this.Data.LongLength > 0 && this.Pointer == IntPtr.Zero;
		public bool OnDevice => this.Pointer != IntPtr.Zero && this.Data.LongLength == 0;
		public IntPtr Pointer { get; set; } = IntPtr.Zero;
		public int ChunkSize { get; set; } = 0;
		public int OverlapSize { get; set; } = 0;
		public string Form { get; set; } = "f";
		public double StretchFactor { get; set; } = 1.0;
		public float Bpm { get; private set; } = 0.0f;

		private float _currentVolume = 1.0f;
		public int Volume => (int) (this._currentVolume * 100.0f);
		private WaveOutEvent player = new();
		private WaveStream? waveStream;
		public object playerLock = new();

		public bool PlayerPlaying => this.player != null && this.player.PlaybackState == PlaybackState.Playing;
		public bool Playing = false;
		public bool Paused = false;
		// Get position from waveStream
		private long position => this.Playing != false && this.waveStream != null ? (this.waveStream.Position / sizeof(float)) / this.Channels : 0;
		private double positionSeconds => this.SampleRate <= 0 ? 0 : (double) this.position / this.SampleRate;
		public TimeSpan CurrentTime => TimeSpan.FromSeconds(this.positionSeconds);

		public int SilenceStart { get; private set; } = 0;

		private CancellationTokenSource _loopCts = new();
		private float _currentLoopValue = 0.0f;
		private bool _isLooping;
		private long loopStart = 0;
		public float Looping => this._isLooping ? this._currentLoopValue : 0;

		public AudioObj(string filePath, bool linearLoad = false)
		{
			this.Id = Guid.NewGuid();
			this.FilePath = filePath;

			this.ReadBpmTag();

			if (this.Data.LongLength <= 0 && linearLoad)
			{
				this.LoadAudioFile();
			}
		}

		public void Dispose()
		{
			this.player?.Stop();
			this.Playing = false;

			this.Data = [];

			BeatScanner.RemoveScanned(this);

			GC.SuppressFinalize(this);
		}

		public void LoadAudioFile()
		{
			if (string.IsNullOrEmpty(this.FilePath))
			{
				throw new FileNotFoundException("File path is empty");
			}

			Stopwatch sw = Stopwatch.StartNew();

			using AudioFileReader reader = new(this.FilePath);
			this.originalSampleRate = reader.WaveFormat.SampleRate;
			this.BitDepth = reader.WaveFormat.BitsPerSample;
			this.Channels = reader.WaveFormat.Channels;

			// Calculate number of samples
			long numSamples = reader.Length / (reader.WaveFormat.BitsPerSample / 8);
			this.Data = new float[numSamples];

			int read = reader.Read(this.Data, 0, (int) numSamples);
			if (read != numSamples)
			{
				float[] resizedData = new float[read];
				Array.Copy(this.Data, resizedData, read);
				this.Data = resizedData;
			}

			sw.Stop();
			this.ElapsedLoadingTime = (float) sw.Elapsed.TotalMilliseconds;

			// Read bpm metadata if available
			this.ReadBpmTag();
		}

		public static async Task<AudioObj?> CreateAsync(string filePath, int maxWorkers = -2)
		{
			// Check file
			if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
			{
				return null;
			}

			var obj = new AudioObj(filePath, false);

			Stopwatch sw = Stopwatch.StartNew();

			using var mainReader = new AudioFileReader(filePath);

			obj.originalSampleRate = mainReader.WaveFormat.SampleRate;
			obj.Channels = mainReader.WaveFormat.Channels;
			obj.BitDepth = mainReader.WaveFormat.BitsPerSample;

			long totalBytes = mainReader.Length;

			// Total length float[] (Data)
			int dataLengthInFloats = (int) totalBytes / sizeof(float);
			obj.Data = new float[dataLengthInFloats];

			// Worker count
			int workerCount = CommonStatics.AdjustWorkersCount(maxWorkers);
			int floatsPerWorker = dataLengthInFloats / workerCount;
			var tasks = new List<Task>();

			for (int i = 0; i < workerCount; i++)
			{
				int startFloatIndex = i * floatsPerWorker;
				int floatsToRead = (i == workerCount - 1) ? dataLengthInFloats - startFloatIndex : floatsPerWorker;

				tasks.Add(Task.Run(() =>
				{
					// Each worker gets a new reader for thread safety
					using var workerReader = new AudioFileReader(filePath);

					// Set start position in bytes
					workerReader.Position = startFloatIndex * sizeof(float);

					// KORREKTUR: Verwende einen float[]-Puffer
					var floatBuffer = new float[floatsToRead];

					// KORREKTUR: Verwende die korrekte Read-Methode für float[]
					int readFloats = workerReader.Read(floatBuffer, 0, floatsToRead);

					// Write to Data
					Array.Copy(floatBuffer, 0, obj.Data, startFloatIndex, readFloats);
				}));
			}

			// Wait for all to finish
			await Task.WhenAll(tasks);

			sw.Stop();
			obj.ElapsedLoadingTime = (float) sw.Elapsed.TotalMilliseconds;

			return obj;
		}

		public float ReadBpmTag(string tag = "TBPM", bool set = true)
		{
			// Read bpm metadata if available
			float bpm = 0.0f;
			float roughBpm = 0.0f;

			try
			{
				if (!string.IsNullOrEmpty(this.FilePath) && File.Exists(this.FilePath))
				{
					using (var file = TagLib.File.Create(this.FilePath))
					{
						if (file.Tag.BeatsPerMinute > 0)
						{
							roughBpm = (float) file.Tag.BeatsPerMinute;
						}
						if (file.TagTypes.HasFlag(TagLib.TagTypes.Id3v2))
						{
							var id3v2Tag = (TagLib.Id3v2.Tag) file.GetTag(TagLib.TagTypes.Id3v2);

							var tagTextFrame = TagLib.Id3v2.TextInformationFrame.Get(id3v2Tag, tag, false);

							if (tagTextFrame != null && tagTextFrame.Text.Any())
							{
								string bpmString = tagTextFrame.Text.FirstOrDefault() ?? "0,0";
								if (!string.IsNullOrEmpty(bpmString))
								{
									bpmString = bpmString.Replace(',', '.');

									if (float.TryParse(bpmString, NumberStyles.Any, CultureInfo.InvariantCulture, out float parsedBpm))
									{
										bpm = parsedBpm;
									}
								}
							}
							else
							{
								bpm = 0.0f;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Fehler beim Lesen des Tags {tag.ToUpper()}: {ex.Message} ({ex.InnerException?.Message ?? " - "})");
			}

			// Take rough bpm if <= 0.0f
			if (bpm <= 0.0f && roughBpm > 0.0f)
			{
				Console.WriteLine($"No value found for '{tag.ToUpper()}', taking rough BPM value from legacy tag.");
				bpm = roughBpm;
			}

			if (set)
			{
				this.Bpm = bpm;
				if (this.Bpm <= 10)
				{
					this.ReadBpmTagLegacy();
				}
			}

			return bpm;
		}

		public float ReadBpmTagLegacy()
		{
			// Read bpm metadata if available
			float bpm = 0.0f;

			try
			{
				if (!string.IsNullOrEmpty(this.FilePath) && File.Exists(this.FilePath))
				{
					using (var file = TagLib.File.Create(this.FilePath))
					{
						// Check for BPM in standard ID3v2 tag
						if (file.Tag.BeatsPerMinute > 0)
						{
							bpm = (float) file.Tag.BeatsPerMinute;
						}
						// Alternative für spezielle Tags (z.B. TBPM Frame)
						else if (file.TagTypes.HasFlag(TagLib.TagTypes.Id3v2))
						{
							var id3v2Tag = (TagLib.Id3v2.Tag) file.GetTag(TagLib.TagTypes.Id3v2);
							var bpmFrame = TagLib.Id3v2.UserTextInformationFrame.Get(id3v2Tag, "BPM", false);

							if (bpmFrame != null && float.TryParse(bpmFrame.Text.FirstOrDefault(), out float parsedBpm))
							{
								bpm = parsedBpm;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Fehler beim Lesen der BPM: {ex.Message}");
			}
			this.Bpm = bpm > 0 ? bpm / 100.0f : 0.0f;
			return this.Bpm;
		}

		public async Task UpdateBpm(float newValue = 0.0f, bool writeTag = false)
		{
			this.Bpm = newValue;

			if (writeTag )
			{
				await this.AddBpmTag(newValue, this.FilePath);
			}
		}

		public async Task<byte[]> GetBytesAsync(int maxWorkers = -2)
		{
			if (this.Data == null || this.Data.Length == 0)
			{
				return [];
			}

			// Negative maxWorkers means subtract from total (to have free workers left)
			maxWorkers = CommonStatics.AdjustWorkersCount(maxWorkers);

			int bytesPerSample = this.BitDepth / 8;
			byte[] result = new byte[this.Data.Length * bytesPerSample];

			await Task.Run(() =>
			{
				var options = new ParallelOptions
				{
					MaxDegreeOfParallelism = maxWorkers
				};

				Parallel.For(0, this.Data.Length, options, i =>
				{
					float sample = this.Data[i];

					switch (this.BitDepth)
					{
						case 8:
							result[i] = (byte) (sample * 127f);
							break;

						case 16:
							short sample16 = (short) (sample * short.MaxValue);
							Span<byte> target16 = result.AsSpan(i * 2, 2);
							BitConverter.TryWriteBytes(target16, sample16);
							break;

						case 24:
							int sample24 = (int) (sample * 8_388_607f); // 2^23 - 1
							Span<byte> target24 = result.AsSpan(i * 3, 3);
							target24[0] = (byte) sample24;
							target24[1] = (byte) (sample24 >> 8);
							target24[2] = (byte) (sample24 >> 16);
							break;

						case 32:
							Span<byte> target32 = result.AsSpan(i * 4, 4);
							BitConverter.TryWriteBytes(target32, sample);
							break;
					}
				});
			});

			return result;
		}

		public byte[] GetBytes(int maxWorkers = -2)
		{
			int bytesPerSample = this.BitDepth / 8;
			byte[] bytes = new byte[this.Data.Length * bytesPerSample];

			maxWorkers = CommonStatics.AdjustWorkersCount(maxWorkers);

			// Parallel options
			var parallelOptions = new ParallelOptions
			{
				MaxDegreeOfParallelism = maxWorkers
			};

			Parallel.For(0, this.Data.Length, parallelOptions, i =>
			{
				switch (this.BitDepth)
				{
					case 8:
						bytes[i] = (byte) (this.Data[i] * 127);
						break;
					case 16:
						short sample16 = (short) (this.Data[i] * short.MaxValue);
						Buffer.BlockCopy(BitConverter.GetBytes(sample16), 0, bytes, i * bytesPerSample, bytesPerSample);
						break;
					case 24:
						int sample24 = (int) (this.Data[i] * 8388607);
						Buffer.BlockCopy(BitConverter.GetBytes(sample24), 0, bytes, i * bytesPerSample, 3);
						break;
					case 32:
						Buffer.BlockCopy(BitConverter.GetBytes(this.Data[i]), 0, bytes, i * bytesPerSample, bytesPerSample);
						break;
				}
			});

			return bytes;
		}

		public async Task<IEnumerable<float[]>> GetChunks(int size = 2048, float overlap = 0.5f, bool keepData = false, int maxWorkers = -2)
		{
			// Input Validation (sync part for fast fail)
			if (this.Data == null || this.Data.Length == 0)
			{
				return [];
			}

			if (size <= 0 || overlap < 0 || overlap >= 1)
			{
				return [];
			}

			// Calculate chunk metrics (sync)
			this.ChunkSize = size;
			this.OverlapSize = (int) (size * overlap);
			int step = size - this.OverlapSize;
			int numChunks = (this.Data.Length - size) / step + 1;

			// Prepare result array
			float[][] chunks = new float[numChunks][];

			await Task.Run(() =>
			{
				// Parallel processing with optimal worker count
				Parallel.For(0, numChunks, new ParallelOptions
				{
					MaxDegreeOfParallelism = CommonStatics.AdjustWorkersCount(maxWorkers)
				}, i =>
				{
					int sourceOffset = i * step;
					float[] chunk = new float[size];
					Buffer.BlockCopy( // Faster than Array.Copy for float[]
						src: this.Data,
						srcOffset: sourceOffset * sizeof(float),
						dst: chunk,
						dstOffset: 0,
						count: size * sizeof(float));
					chunks[i] = chunk;
				});
			});

			// Cleanup if requested
			if (!keepData)
			{
				this.Data = [];
			}

			return chunks;
		}

		public async Task AggregateStretchedChunks(IEnumerable<float[]> chunks, bool keepPointer = false, int maxWorkers = 2)
		{
			if (chunks == null || chunks.LongCount() <= 0)
			{
				return;
			}

			// Pointer
			this.Pointer = keepPointer ? this.Pointer : IntPtr.Zero;

			// Pre-calculate all values that don't change
			double stretchFactor = this.StretchFactor;
			int chunkSize = this.ChunkSize;
			int overlapSize = this.OverlapSize;
			int originalHopSize = chunkSize - overlapSize;
			int stretchedHopSize = (int) Math.Round(originalHopSize * stretchFactor);
			int outputLength = (chunks.Count() - 1) * stretchedHopSize + chunkSize;

			// Create window function (cosine window)
			double[] window = await Task.Run(() =>
				Enumerable.Range(0, chunkSize)
						  .Select(i => 0.5 * (1.0 - Math.Cos(2.0 * Math.PI * i / (chunkSize - 1))))
						  .ToArray()  // Korrekte Methode ohne Punkt
			).ConfigureAwait(false);

			// Initialize accumulators in parallel
			double[] outputAccumulator = new double[outputLength];
			double[] weightSum = new double[outputLength];

			await Task.Run(() =>
			{
				var parallelOptions = new ParallelOptions
				{
					MaxDegreeOfParallelism = CommonStatics.AdjustWorkersCount(maxWorkers)
				};

				// Phase 1: Process chunks in parallel
				Parallel.For(0, chunks.LongCount(), parallelOptions, chunkIndex =>
				{
					var chunk = chunks.ElementAt((int) chunkIndex);
					int offset = (int) chunkIndex * stretchedHopSize;

					for (int j = 0; j < Math.Min(chunkSize, chunk.Length); j++)
					{
						int idx = offset + j;
						if (idx >= outputLength)
						{
							break;
						}

						double windowedSample = chunk[j] * window[j];

						// Using Interlocked for thread-safe accumulation
						Interlocked.Exchange(ref outputAccumulator[idx], outputAccumulator[idx] + windowedSample);
						Interlocked.Exchange(ref weightSum[idx], weightSum[idx] + window[j]);
					}
				});

				// Phase 2: Normalize results
				float[] finalOutput = new float[outputLength];
				Parallel.For(0, outputLength, parallelOptions, i =>
				{
					finalOutput[i] = weightSum[i] > 1e-6
						? (float) (outputAccumulator[i] / weightSum[i])
						: 0.0f;
				});

				// Final assignment (thread-safe)
				this.Data = finalOutput;
			}).ConfigureAwait(true);
		}

		// Playback
		public void SetVolume(float volume, bool smoothTransition = false)
		{
			volume = Math.Clamp(volume, 0.0f, 1.0f);

			lock (this.playerLock)
			{
				if (this.player == null)
				{
					return;
				}

				if (smoothTransition && this.player.PlaybackState == PlaybackState.Playing)
				{
					// Sanfte Übergänge (optional)
					Task.Run(async () =>
					{
						float startVolume = this._currentVolume;
						float duration = 0.2f; // 200ms Übergang
						int steps = 10;

						for (int i = 0; i <= steps; i++)
						{
							float t = i / (float) steps;
							float v = startVolume + (volume - startVolume) * t;

							lock (this.playerLock)
							{
								this.player.Volume = v;
							}

							await Task.Delay((int) (duration * 1000 / steps));
						}
						this._currentVolume = volume;
					});
				}
				else
				{
					// Direkte Änderung
					this.player.Volume = volume;
					this._currentVolume = volume;
				}
			}
		}

		public async Task Play(CancellationToken cancellationToken, Action? onPlaybackStopped = null, float? initialVolume = null)
		{
			this.Playing = true;

			initialVolume ??= this.Volume / 100f;

			// Stop any existing playback and cleanup if not paused
			bool paused = false;
			if (this.player != null && this.player.PlaybackState != PlaybackState.Paused)
			{
				this.player?.Stop();
				this.player?.Dispose();
				this.waveStream?.Dispose();
				this.waveStream = null;
				this.Paused = false;
			}
			else
			{
				paused = true;
				this.Paused = false;
			}

			if (this.Data == null || this.Data.Length == 0)
			{
				return;
			}

			try
			{
				// Initialize player with cancellation support
				this.player = new WaveOutEvent
				{
					Volume = initialVolume ?? 1.0f,
					DesiredLatency = 100 // Lower latency for better responsiveness
				};

				// Async audio data preparation with cancellation
				byte[] bytes = await this.GetBytesAsync();

				var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(this.SampleRate, this.Channels);
				var memoryStream = new MemoryStream(bytes);
				var audioStream = new RawSourceWaveStream(memoryStream, waveFormat);

				// Setup playback stopped handler
				this.player.PlaybackStopped += (sender, args) =>
				{
					try
					{
						onPlaybackStopped?.Invoke();
					}
					finally
					{
						this.Playing = false;
						audioStream.Dispose();
						memoryStream.Dispose();
						this.player?.Dispose();
					}
				};

				// Register cancellation callback
				using (cancellationToken.Register(() =>
				{
					this.player?.Stop();
				}))
				{
					// Start playback in background
					this.waveStream = audioStream;
					this.player.Init(audioStream);
					// this.waveformUpdateTimer.Start();

					// Non-blocking play (fire-and-forget with error handling)
					_ = Task.Run(() =>
					{
						try
						{
							// If paused, resume playback (set position)
							if (paused)
							{
								// Set position to last known position
							}

							this.player.Play();
							while (this.player.PlaybackState == PlaybackState.Playing)
							{
								cancellationToken.ThrowIfCancellationRequested();
								Thread.Sleep(50); // Reduce CPU usage
							}
						}
						catch (OperationCanceledException)
						{
							// Cleanup handled by cancellation callback
						}
						catch (Exception ex)
						{
							Debug.WriteLine($"Playback error: {ex.Message}");
						}
					}, cancellationToken);
				}

				// Return immediately (non-blocking)
			}
			catch (OperationCanceledException)
			{
				Debug.WriteLine("Playback preparation was canceled");
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Playback initialization failed: {ex.Message}");
				this.player?.Dispose();
				throw;
			}
		}

		public async Task Pause()
		{ 			
			if (this.player == null)
			{
				return;
			}

			if (this.player.PlaybackState == PlaybackState.Playing)
			{
				this.Playing = false;
				this.Paused = true;
				await Task.Run(() => this.player.Pause());
			}
			else if (this.player.PlaybackState == PlaybackState.Paused)
			{
				this.Playing = true;
				this.Paused = false;
				await Task.Run(() => this.player.Play());
			}

		}

		public void Stop()
		{
			this.Playing = false;
			this.Paused = false;
			this.waveStream?.Dispose();
			this.player.Stop();
		}

		public async Task Normalize(float maxAmplitude = 1.0f)
		{
			if (this.Data == null || this.Data.Length == 0)
			{
				return;
			}

			// Phase 1: Find global maximum (parallel + async)
			float globalMax = await Task.Run(() =>
			{
				float max = 0f;
				Parallel.For(0, this.Data.Length,
					() => 0f,
					(i, _, localMax) => Math.Max(Math.Abs(this.Data[i]), localMax),
					localMax => { lock (this) { max = Math.Max(max, localMax); } }
				);
				return max;
			}).ConfigureAwait(false);

			if (globalMax == 0f)
			{
				return;
			}

			// Phase 2: Apply scaling (parallel + async)
			float scale = maxAmplitude / globalMax;
			await Task.Run(() =>
			{
				Parallel.For(0, this.Data.Length, i =>
				{
					this.Data[i] *= scale;
				});
			}).ConfigureAwait(false);
		}

		public async Task Level(float duration = 1.0f, float average = 1.0f, int maxWorkers = -2)
		{
			// Validate input
			duration = Math.Clamp(duration, 0.1f, 600.0f);
			maxWorkers = CommonStatics.AdjustWorkersCount(maxWorkers);

			if (this.Data == null || this.Data.Length == 0)
			{
				return;
			}

			// Calculate number of samples to process
			int samplesPerSecond = (int) (this.SampleRate * this.Channels);
			int totalSamples = (int) (duration * samplesPerSecond);
			if (totalSamples <= 0 || totalSamples > this.Data.Length)
			{
				return;
			}

			// Calculate the average level over the specified duration
			float averageLevel = average * await Task.Run(() =>
			{
				float sum = 0f;
				int count = 0;
				Parallel.For(0, totalSamples, new ParallelOptions { MaxDegreeOfParallelism = maxWorkers }, i =>
				{
					if (i < this.Data.Length)
					{
						sum += Math.Abs(this.Data[i]);
						count++;
					}
				});
				return count > 0 ? sum / count : 0f;
			}).ConfigureAwait(false);
			if (averageLevel <= 0f)
			{
				return;
			}

			// Within the specified duration, normalize the audio data to the average level
			await Task.Run(() =>
			{
				Parallel.For(0, totalSamples, new ParallelOptions { MaxDegreeOfParallelism = maxWorkers }, i =>
				{
					if (i < this.Data.Length)
					{
						this.Data[i] = (this.Data[i] / Math.Abs(this.Data[i])) * averageLevel;
					}
				});
			}).ConfigureAwait(false);

			// Optionally, normalize the entire audio data to ensure no clipping occurs
			await this.Normalize(maxAmplitude: 1.0f).ConfigureAwait(false);
		}

		public async Task<System.Drawing.Image?> GetWaveformImageSimpleAsync(float[]? data, int width = 720, int height = 480,
			int samplesPerPixel = 128, float amplifier = 1.0f, long offset = 0,
			System.Drawing.Color? graphColor = null, System.Drawing.Color? backgroundColor = null, bool smoothEdges = true, int workerCount = -2)
		{
			// Normalisiere Bilddimensionen
			width = Math.Max(100, width);
			height = Math.Max(100, height);
			offset = this.position * this.Channels;

			// Normalisiere Farben
			graphColor ??= System.Drawing.Color.BlueViolet;
			backgroundColor ??= System.Drawing.Color.White;

			// Erstelle ein neues Bitmap-Objekt
			Bitmap bitmap = new(width, height, PixelFormat.Format32bppArgb);

			// Verifiziere Daten
			data ??= this.Data;
			if (data == null || data.LongLength <= 0)
			{
				return bitmap;
			}

			// Passen den Offset an, falls erforderlich
			if (data.Length <= offset)
			{
				offset = 0;
			}

			// Normalisiere die Worker-Anzahl
			if (workerCount <= 0)
			{
				workerCount = Environment.ProcessorCount;
			}
			workerCount = Math.Min(workerCount, 16);

			// Berechne die Anzahl der zu verarbeitenden Samples
			long totalSamples = Math.Min(data.Length - offset, (long) width * samplesPerPixel);
			if (totalSamples <= 0)
			{
				return bitmap;
			}

			// Kopiere die Samples in ein separates Array für threadsichere Lesevorgänge
			float[] samples = new float[totalSamples];
			Array.Copy(data, offset, samples, 0, totalSamples);

			// Führe die gesamte Operation asynchron in einem Hintergrund-Thread aus
			await Task.Run(() =>
			{
				// Sperre die Bitmap-Bits, um direkten Speicherzugriff zu ermöglichen
				BitmapData bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
				IntPtr ptr = bitmapData.Scan0;

				int bytesPerPixel = 4; // 32bppArgb
				int stride = bitmapData.Stride;
				byte[] pixels = new byte[stride * height];

				// Lese die vorhandenen Pixeldaten in ein Array.
				// Das ermöglicht uns, es parallel und sicher zu manipulieren.
				System.Runtime.InteropServices.Marshal.Copy(ptr, pixels, 0, pixels.Length);

				// Fülle den Hintergrund
				int backgroundArgb = backgroundColor.Value.ToArgb();
				Parallel.For(0, pixels.Length / bytesPerPixel, i =>
				{
					int index = i * bytesPerPixel;
					pixels[index] = backgroundColor.Value.B;
					pixels[index + 1] = backgroundColor.Value.G;
					pixels[index + 2] = backgroundColor.Value.R;
					pixels[index + 3] = backgroundColor.Value.A;
				});

				// Verarbeite die Chunks parallel
				int chunkWidth = width / workerCount;
				Parallel.For(0, workerCount, i =>
				{
					int xStart = i * chunkWidth;
					int xEnd = (i == workerCount - 1) ? width : xStart + chunkWidth;

					// Zeichne innerhalb dieses Worker-Bereichs
					for (int x = xStart; x < xEnd; x++)
					{
						// Berechne den Index im Sample-Array
						int sampleIndex = x * samplesPerPixel;
						if (sampleIndex < samples.Length)
						{
							// Finden des Maximums und Minimums innerhalb der Samples für dieses Pixel
							float maxPeak = 0.0f;
							float minPeak = 0.0f;

							int startSample = x * samplesPerPixel;
							int endSample = Math.Min(startSample + samplesPerPixel, samples.Length);

							for (int s = startSample; s < endSample; s++)
							{
								if (samples[s] > maxPeak)
								{
									maxPeak = samples[s];
								}

								if (samples[s] < minPeak)
								{
									minPeak = samples[s];
								}
							}

							// Skaliere die Peak-Werte auf die Bildhöhe
							int yMax = (int) (((maxPeak * amplifier) + 1.0f) / 2.0f * height);
							int yMin = (int) (((minPeak * amplifier) + 1.0f) / 2.0f * height);
							yMax = Math.Clamp(yMax, 0, height - 1);
							yMin = Math.Clamp(yMin, 0, height - 1);

							// Sortiere yMin und yMax
							if (yMin > yMax)
							{
								(yMin, yMax) = (yMax, yMin);
							}

							// Zeichne die vertikale Linie von yMin bis yMax
							int colorArgb = graphColor.Value.ToArgb();
							for (int y = yMin; y <= yMax; y++)
							{
								int index = y * stride + x * bytesPerPixel;
								pixels[index] = graphColor.Value.B;
								pixels[index + 1] = graphColor.Value.G;
								pixels[index + 2] = graphColor.Value.R;
								pixels[index + 3] = graphColor.Value.A;
							}
						}
					}
				});

				// Kopiere die geänderten Pixel-Daten zurück in die Bitmap
				System.Runtime.InteropServices.Marshal.Copy(pixels, 0, ptr, pixels.Length);

				// Entsperre die Bitmap-Bits
				bitmap.UnlockBits(bitmapData);
			});

			return bitmap;
		}

		// Export
		public async Task<string?> Export(string outPath = "", string? outFile = null)
		{
			if (File.Exists(outFile))
			{
				// If outPath is a file, use its directory
				outPath = Path.GetDirectoryName(outFile) ?? string.Empty;
			}

			if (string.IsNullOrEmpty(outPath))
			{
				outPath = Path.GetTempPath();
			}

			string baseFileName = $"{this.Name.Replace("▶ ", "").Replace("|| ", "")}{(this.Bpm > 10 ? (" [" + this.Bpm.ToString("F1", CultureInfo.InvariantCulture) + "]") : "")}".Trim();

			// Validate and prepare output directory
			outPath = (await this.PrepareOutputPath(outPath, baseFileName)) ?? Path.GetTempPath();
			if (string.IsNullOrEmpty(outPath))
			{
				return null;
			}

			try
			{
				// Process audio data in parallel
				byte[] bytes = await this.GetBytesAsync();

				var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(
					this.SampleRate,
					this.Channels);

				await using (var memoryStream = new MemoryStream(bytes))
				await using (var audioStream = new RawSourceWaveStream(memoryStream, waveFormat))
				await using (var fileStream = new FileStream(
					outPath,
					FileMode.Create,
					FileAccess.Write,
					FileShare.None,
					bufferSize: 4096,
					useAsync: true))
				{
					await Task.Run(() =>
					{
						WaveFileWriter.WriteWavFileToStream(fileStream, audioStream);
					});
				}

				// Add BPM metadata if needed
				if (this.Bpm > 0.0f)
				{
					await this.AddBpmTag(this.Bpm, outPath)
						.ConfigureAwait(false);
				}

				this.ExportedFile = outPath;

				return outPath;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Audio export failed: {ex.Message}");
				return null;
			}
		}

		private async Task<string?> PrepareOutputPath(string outPath, string baseFileName)
		{
			// Check directory existence asynchronously
			if (!string.IsNullOrEmpty(outPath))
			{
				var dirExists = await Task.Run(() => Directory.Exists(outPath))
										.ConfigureAwait(false);
				if (!dirExists)
				{
					outPath = Path.GetDirectoryName(outPath) ?? string.Empty;
				}
			}

			// Fallback to temp directory if needed
			if (string.IsNullOrEmpty(outPath) ||
				!await Task.Run(() => Directory.Exists(outPath))
						  .ConfigureAwait(false))
			{
				outPath = Path.Combine(
					Path.GetTempPath(),
					$"{this.Name}_[{this.Bpm:F2}].wav");
			}

			// Build final file path
			if (Path.HasExtension(outPath))
			{
				return outPath;
			}

			return Path.Combine(outPath, $"{baseFileName}.wav");
		}

		private async Task AddBpmTag(float bpm = 0.0f, string filePath = "")
		{
			if (string.IsNullOrEmpty(filePath))
			{
				filePath = this.FilePath;
			}

			if (bpm <= 0)
			{
				bpm = this.Bpm;
			}

			if (!File.Exists(filePath))
			{
				Debug.WriteLine($"File not found for BPM tag writing: {filePath}");
				return;
			}

			try
			{
				await Task.Run(() =>
				{
					// Create or open the file using TagLib
					using (var file = TagLib.File.Create(filePath))
					{
						// Set BPM tag
						file.Tag.BeatsPerMinute = (uint) (bpm < 1000 ? bpm * 100 : bpm);

						// Add or update TBPM frame
						var id3v2Tag = (TagLib.Id3v2.Tag) file.GetTag(TagLib.TagTypes.Id3v2);
						var bpmFrame = TagLib.Id3v2.TextInformationFrame.Get(id3v2Tag, "TBPM", true);
						if (bpmFrame == null)
						{
							bpmFrame = new TagLib.Id3v2.TextInformationFrame("TBPM");
							id3v2Tag.AddFrame(bpmFrame);
						}
						bpmFrame.Text = [bpm.ToString(CultureInfo.InvariantCulture)];

						// Save changes
						file.Save();
					}
				}).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"BPM tag writing failed: {ex.Message}");
			}
		}

		public override string ToString()
		{
			return this.Name;
		}

		public void JumpByBeats(int beats)
		{
			if (beats == 0 || this.player == null || this.waveStream == null ||
				this.player.PlaybackState != PlaybackState.Playing)
			{
				return;
			}

			try
			{
				// Try to get the lock with timeout to avoid deadlocks
				if (Monitor.TryEnter(this.playerLock, TimeSpan.FromMilliseconds(100)))
				{
					try
					{
						// Calculate jump
						float bpm = Math.Max(this.Bpm, 60.0f);
						int bytesPerSecond = this.waveStream.WaveFormat.AverageBytesPerSecond;
						int bytesPerBeat = (int) (bytesPerSecond * 60.0f / bpm);
						int jumpBytes = bytesPerBeat * beats;

						// Calculate and set new position
						long newPosition = this.waveStream.Position + jumpBytes;
						newPosition = Math.Max(0, Math.Min(newPosition, this.waveStream.Length));
						this.waveStream.Position = newPosition;
						this.loopStart = this.waveStream.Position;

						// Restart looping if it was active
						if (!this._loopCts.IsCancellationRequested)
						{
							_ = this.StartLoop(this._currentLoopValue); // Fire-and-forget restart
						}
					}
					finally
					{
						Monitor.Exit(this.playerLock);
					}
				}
				else
				{
					Debug.WriteLine("JumpByBeats: Could not acquire lock, operation skipped");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"JumpByBeats error: {ex.Message}");
			}
		}

		public async Task StartLoop(float loopValue)
		{
			this.StopLoop();

			if (loopValue == 0 || this._isLooping || this.player?.PlaybackState != PlaybackState.Playing)
			{
				return;
			}

			this._currentLoopValue = loopValue;
			this._isLooping = true;
			this._loopCts = new CancellationTokenSource();

			try
			{
				await Task.Run(async () =>
				{
					// Vorab-Berechnungen
					int bytesPerSecond = 0;
					int loopBytes = 0;
					bool isForwardLoop = loopValue > 0;

					lock (this.playerLock)
					{
						if (this.waveStream != null)
						{
							bytesPerSecond = this.waveStream.WaveFormat.AverageBytesPerSecond;
							float bpm = Math.Max(this.Bpm, 60.0f);
							loopBytes = (int) ((bytesPerSecond * 60.0f / bpm) * Math.Abs(loopValue));
						}
					}

					while (!this._loopCts.IsCancellationRequested &&
						   this.player?.PlaybackState == PlaybackState.Playing)
					{
						long loopEndPos;

						// 1. Loop-Bereich festlegen
						lock (this.playerLock)
						{
							if (this.waveStream == null)
							{
								return;
							}

							this.loopStart = this.waveStream.Position;
							loopEndPos = isForwardLoop
								? Math.Min(this.loopStart + loopBytes, this.waveStream.Length)
								: Math.Max(0, this.loopStart - loopBytes);
						}

						// 2. Normal abspielen lassen bis zum Endpunkt
						bool reachedEnd = false;
						while (!reachedEnd &&
							   !this._loopCts.IsCancellationRequested &&
							   this.player?.PlaybackState == PlaybackState.Playing)
						{
							await Task.Delay(50); // Check-Intervall

							lock (this.playerLock)
							{
								if (this.waveStream == null)
								{
									return;
								}

								long currentPos = this.waveStream.Position;
								reachedEnd = isForwardLoop
									? currentPos >= loopEndPos
									: currentPos <= loopEndPos;
							}
						}

						// 3. Nur zurückspringen wenn natürlich beendet
						if (reachedEnd && !this._loopCts.IsCancellationRequested)
						{
							lock (this.playerLock)
							{
								if (this.waveStream != null && isForwardLoop)
								{
									this.waveStream.Position = this.loopStart;
								}
							}
						}
						else
						{
							break; // Bei Abbruch raus
						}
					}
				}, this._loopCts.Token);
			}
			catch (OperationCanceledException)
			{
				/* Expected */
			}
			finally
			{
				this._isLooping = false;
			}
		}

		public void StopLoop()
		{
			this._loopCts?.Cancel();
			this._isLooping = false;
			this._currentLoopValue = 0;
		}

		public double GetTime(long position = -1)
		{
			if (position < 0)
			{
				position = this.position;
			}
			
			if (this.SampleRate <= 0 || this.Channels <= 0)
			{
				return 0.0;
			}

			// Berechne die Zeit in Sekunden
			double timeInSeconds = (double) position / (this.SampleRate * this.Channels);
			return timeInSeconds;
		}

		public async Task<float[]> ConvertToMono(bool set = false, int maxWorkers = -2)
		{
			if (this.Data == null || this.Data.Length == 0 || this.Channels <= 0)
			{
				return [];
			}

			// Adjust maxWorkers
			maxWorkers = CommonStatics.AdjustWorkersCount(maxWorkers);

			// Calculate the number of samples in mono
			int monoSampleCount = this.Data.Length / this.Channels;
			float[] monoData = new float[monoSampleCount];

			await Task.Run(() =>
			{
				Parallel.For(0, monoSampleCount, new ParallelOptions
				{
					MaxDegreeOfParallelism = maxWorkers
				}, i =>
				{
					float sum = 0.0f;
					for (int channel = 0; channel < this.Channels; channel++)
					{
						sum += this.Data[i * this.Channels + channel];
					}
					monoData[i] = sum / this.Channels;
				});
			});

			if (set)
			{
				this.Data = monoData;
				this.Channels = 1;
			}

			return monoData;
		}

		public async Task<float[][]> SplitData(int chunkSize = 65536, int maxWorkers = -2)
		{
			if (this.Data == null || this.Data.Length == 0)
			{
				return [];
			}

			// Adjust maxWorkers
			maxWorkers = CommonStatics.AdjustWorkersCount(maxWorkers);

			// Validate chunk size
			chunkSize = Math.Max(1, chunkSize);
			
			int totalChunks = (int) Math.Ceiling((double) this.Data.Length / chunkSize);
			float[][] chunks = new float[totalChunks][];
			
			await Task.Run(() =>
			{
				Parallel.For(0, totalChunks, new ParallelOptions
				{
					MaxDegreeOfParallelism = maxWorkers
				}, i =>
				{
					int start = i * chunkSize;
					int length = Math.Min(chunkSize, this.Data.Length - start);
					chunks[i] = new float[chunkSize];
					Array.Copy(this.Data, start, chunks[i], 0, length);
				});
			});

			return chunks;
		}



		public async Task<float[]> GetCurrentWindow(int windowSize = 65536, int lookingRange = 2, bool mono = false, bool lookBackwards = false)
		{
			if (this.Data == null || this.Data.Length == 0 || this.SampleRate <= 0 || this.Channels <= 0)
			{
				return [];
			}

			// Parameter normalisieren
			windowSize = Math.Max(1, windowSize);
			lookingRange = Math.Max(1, lookingRange);
			// Für spätere FFTs oft sinnvoll: auf Potenz von 2 schnappen
			windowSize = (int) Math.Pow(2, Math.Ceiling(Math.Log(windowSize, 2)));

			// Position in Frames (position ist bereits frame-basiert)
			long posFrames = this.position;

			// Fensterbreite in Frames: um pos herum jeweils die Hälfte
			int halfWindowFrames = (windowSize * lookingRange) / 2;
			int fullWindowFrames = halfWindowFrames * 2;
			if (fullWindowFrames <= 0)
			{
				return [];
			}

			if (mono)
			{
				// Mono-Daten (Frames == Samples)
				float[] data = await this.ConvertToMono(false);
				if (data.Length == 0)
				{
					return [];
				}

				long startFrame = posFrames - (lookBackwards ? halfWindowFrames : 0);
				long endFrameExclusive = startFrame + fullWindowFrames;

				// Out-of-bounds vermeiden, verschieben
				while (endFrameExclusive > data.Length)
				{
					startFrame -= windowSize;
					endFrameExclusive -= windowSize;
				}

				while (startFrame < 0)
				{
					startFrame += windowSize;
					endFrameExclusive += windowSize;
				}

				if (endFrameExclusive > data.LongLength)
				{
					return [];
				}

				float[] current = new float[fullWindowFrames];
				await Task.Run(() => Array.Copy(data, (int) startFrame, current, 0, fullWindowFrames));
				return current;
			}
			else
			{
				// Interleaved Mehrkanal-Daten (Floats)
				float[] data = this.Data;

				long startFloatIndex = (posFrames - (lookBackwards ? halfWindowFrames : 0)) * this.Channels;
				long endFloatIndexExclusive = startFloatIndex + ((long) fullWindowFrames * this.Channels);

				while (endFloatIndexExclusive > data.Length)
				{
					startFloatIndex -= windowSize * this.Channels;
					endFloatIndexExclusive -= windowSize * this.Channels;
				}

				while (startFloatIndex < 0)
				{
					startFloatIndex += windowSize * this.Channels;
					endFloatIndexExclusive += windowSize * this.Channels;
				}

				if (endFloatIndexExclusive > data.LongLength || startFloatIndex < 0)
				{
					Debug.WriteLine("GetCurrentWindow: Out of bounds access prevented.");
					// Out-of-bounds, verschieben
					return [];
				}

				int lengthFloats = fullWindowFrames * this.Channels;
				float[] current = new float[lengthFloats];
				await Task.Run(() => Array.Copy(data, (int) startFloatIndex, current, 0, lengthFloats));
				return current;
			}
		}


	}
}
