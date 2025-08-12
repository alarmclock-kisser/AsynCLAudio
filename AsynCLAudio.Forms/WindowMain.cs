using AsynCLAudio.Core;
using AsynCLAudio.OpenCl;
using NAudio.CoreAudioApi;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Timer = System.Threading.Timer;

namespace AsynCLAudio.Forms
{
	public partial class WindowMain : Form
	{
		private readonly AudioCollection audioCollection;
		private readonly OpenClService openClService;

		private bool disposing = false;

		public AudioObj? SelectedTrack => this.audioCollection[this.listBox_tracks.SelectedItem?.ToString() ?? string.Empty];

		private bool isProcessing = false;
		private Dictionary<NumericUpDown, long> previousNumericValues = [];

		private CancellationToken? playbackCancellationToken;
		private float Volume => 1.0f - this.vScrollBar_volume.Value / 100f;
		private System.Timers.Timer playbackTimer = new(50);
		private Color graphHue = Color.Red;

		private bool recording = false;
		private System.Timers.Timer recordingTimer = new(120);
		private DateTime recordingTime = DateTime.Now;

		public WindowMain(AudioCollection audioCollection, OpenClService openClService)
		{
			this.audioCollection = audioCollection;
			this.openClService = openClService;

			this.InitializeComponent();


			this.UpdateGraphColorButton();

			// Event for right-click on entry -> remove context menu (selected track)
			this.SetupContextMenuForListBox();
			this.RegisterNumericToSecondPow(this.numericUpDown_chunkSize);
			this.listBox_tracks.SelectedIndexChanged += (sender, e) => this.UpdateInfoView();
			this.recordingTimer.Elapsed += RecordingTimer_Elapsed;
			this.listBox_log.DoubleClick += this.listBox_log_DoubleClick;
			this.FillDevicesComboBox(2);
			this.comboBox_captureDevices.Items.AddRange(AudioRecorder.GetCaptureDevices());
			var standardCaptureDevice = AudioRecorder.GetDefaultPlaybackDevice();
			if (standardCaptureDevice != null)
			{
				this.comboBox_captureDevices.SelectedItem = standardCaptureDevice;
			}
			else if (this.comboBox_captureDevices.Items.Count > 0)
			{
				this.comboBox_captureDevices.SelectedIndex = 0;
			}
			this.comboBox_captureDevices.SelectedIndexChanged += (sender, e) =>
			{
				if (this.comboBox_captureDevices.SelectedItem is MMDevice device)
				{
					AudioRecorder.SetCaptureDevice(device);
					this.Log("Set capture device", device.FriendlyName);
				}
			};
			this.UpdateInfoView();
		}


		// ----- Methods ----- \\
		protected override void Dispose(bool disposing = true)
		{
			// End recording if running
			if (this.recording)
			{
				this.button_record.PerformClick();
			}

			this.playbackTimer.Stop();
			this.playbackTimer.Elapsed -= (sender, e) => this.UpdateWaveform().GetAwaiter().GetResult();

			this.disposing = true;

			if (disposing)
			{
				this.audioCollection.StopAll();
				if (this.playbackTimer != null)
				{
					this.playbackTimer.Dispose();
				}
				if (this.recordingTimer != null)
				{
					this.recordingTimer.Stop();
					this.recordingTimer.Elapsed -= RecordingTimer_Elapsed;
					this.recordingTimer.Dispose();
				}
				if (this.components != null)
				{
					this.components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		public void Log(string message = "", string inner = "", bool messageBox = false)
		{
			string timeStamp = DateTime.Now.ToString("HH:mm:ss.fff");
			string logMessage = $"[{timeStamp}]: {message}" + (string.IsNullOrEmpty(inner) ? "" : $" ({inner})");

			Console.WriteLine(logMessage);
			this.listBox_log.Items.Add(logMessage);

			// Scroll to bottom
			if (this.listBox_log.Items.Count > 0)
			{
				this.listBox_log.SelectedIndex = this.listBox_log.Items.Count - 1;
				this.listBox_log.TopIndex = this.listBox_log.Items.Count - 1;
			}

			if (messageBox)
			{
				MessageBox.Show(message, $"Log [{timeStamp}]", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		private void listBox_log_DoubleClick(Object? sender, EventArgs e)
		{
			// Copy selected log entry to clipboard
			if (this.listBox_log.SelectedItem != null)
			{
				Clipboard.SetText(this.listBox_log.SelectedItem.ToString() ?? string.Empty);
				MessageBox.Show(this.listBox_log.SelectedItem.ToString(), "Log copied to clipboard", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		private void FillDevicesComboBox(int init = -1)
		{
			this.comboBox_devices.SelectedIndexChanged -= (sender, e) => this.openClService.Initialize(this.comboBox_devices.SelectedIndex);

			this.comboBox_devices.Items.Clear();
			this.comboBox_devices.Items.AddRange(this.openClService.GetDeviceEntries().ToArray());

			this.comboBox_devices.SelectedIndexChanged += (sender, e) => this.openClService.Initialize(this.comboBox_devices.SelectedIndex);

			if (init >= 0 && init < this.comboBox_devices.Items.Count)
			{
				this.comboBox_devices.SelectedIndex = init;

				if (this.openClService.Index == init)
				{
					this.Log($"Initialized OpenCL on [{init}]", this.openClService.GetDeviceInfo() ?? "N/A");
				}
			}
		}

		private void FillTracksListBox(bool keepSelection = true)
		{
			int selectedIndex = this.listBox_tracks.SelectedIndex;
			this.listBox_tracks.SelectedValueChanged -= (sender, e) => this.UpdateInfoView();
			this.listBox_tracks.Items.Clear();

			foreach (var audio in this.audioCollection.Tracks)
			{
				this.listBox_tracks.Items.Add(audio.Name);
			}

			if (keepSelection && selectedIndex >= 0 && selectedIndex < this.listBox_tracks.Items.Count)
			{
				this.listBox_tracks.SelectedIndex = selectedIndex;
				this.UpdateInfoView();
			}
			else
			{
				this.listBox_tracks.SelectedIndex = -1;
			}

			this.listBox_tracks.SelectedIndexChanged += (sender, e) => this.UpdateInfoView();
		}

		private void FillStretchKernelsComboBox(string searchKey = "timestretch")
		{
			this.comboBox_stretchKernels.Items.Clear();

			string[] stretchKernels = this.openClService.Compiler?.KernelNames.Where(k => k.ToLower().Contains(searchKey.ToLower())).ToArray() ?? [];

			this.comboBox_stretchKernels.Items.AddRange(stretchKernels);

			if (this.comboBox_stretchKernels.Items.Count > 0)
			{
				this.comboBox_stretchKernels.SelectedIndex = 0;
			}
			else
			{
				this.comboBox_stretchKernels.SelectedIndex = -1;
			}
		}

		private void UpdateInfoView()
		{
			this.audioCollection.StopAll();
			this.button_playback.Text = "▶";
			this.numericUpDown_chunkSize.Enabled = true;
			this.numericUpDown_overlap.Enabled = true;
			this.button_stretch.Enabled = true;
			this.button_reset.Enabled = true;
			this.comboBox_stretchKernels.Enabled = true;
			this.numericUpDown_samplesPerPixel.Enabled = true;

			var track = this.SelectedTrack;
			if (track == null)
			{
				this.textBox_trackInfo.Text = "No track selected.";
				this.button_export.Enabled = false;
				this.button_playback.Enabled = false;
				this.pictureBox_waveform.Image = null;
				this.button_reset.Enabled = false;
				this.button_stretch.Enabled = false;
				this.comboBox_stretchKernels.Enabled = false;
				this.numericUpDown_samplesPerPixel.Enabled = false;
				return;
			}

			// Fill time stretch kernels
			this.FillStretchKernelsComboBox();

			// Update waveform
			// this.UpdateWaveform().Wait();

			this.textBox_trackInfo.Text = track.SampleRate + " Hz" + Environment.NewLine +
				track.Channels + " ch." + Environment.NewLine +
				track.BitDepth + " bits" + Environment.NewLine +
				track.Length.ToString("N0") + " f32" + Environment.NewLine +
				track.Duration.ToString("hh\\:mm\\:ss\\.fff") + Environment.NewLine +
				"<" + track.Pointer.ToString() + ">" + Environment.NewLine +
				"Form: " + track.Form + Environment.NewLine +
				track.Bpm.ToString("F3") + " BPM" + Environment.NewLine +
				"Load: " + track.ElapsedLoadingTime.ToString("F1") + " ms" + Environment.NewLine +
				"Process: " + track.ElapsedProcessingTime.ToString("F1") + " ms";

			if (track.Bpm > 1000)
			{
				track.UpdateBpm(track.Bpm / 100f);
			}

			this.numericUpDown_initialBpm.Value = (decimal) Math.Max((float) this.numericUpDown_initialBpm.Minimum, (float) (track.Bpm));

			this.button_export.Enabled = true;
			this.button_playback.Enabled = true;

			if (track.OnDevice)
			{
				this.numericUpDown_chunkSize.Value = track.ChunkSize;
				this.numericUpDown_chunkSize.Enabled = false;
				this.numericUpDown_overlap.Value = (decimal) track.OverlapSize / track.ChunkSize;
				this.numericUpDown_overlap.Enabled = false;
				this.button_export.Enabled = false;
				this.button_playback.Enabled = false;
			}
		}

		private async Task UpdateWaveform()
		{
			if (this.disposing || this.IsDisposed || !this.IsHandleCreated)
			{
				return;
			}

			if (this.InvokeRequired)
			{
				try
				{
					this.BeginInvoke(new Action(async () => await this.UpdateWaveform()));
				}
				catch (ObjectDisposedException)
				{
					// Form is disposed, ignore
				}
				catch (InvalidOperationException)
				{
					// Handle if invoke cannot be performed
				}
				return;
			}

			// Check selected track
			if (this.SelectedTrack == null)
			{
				// No track selected, clear waveform
				if (this.pictureBox_waveform.Image != null)
				{
					this.pictureBox_waveform.Image.Dispose();
					this.pictureBox_waveform.Image = null;
				}

				this.playbackTimer.Stop();
				return;
			}

			// Get peak volume for selected mmDevice
			MMDevice? selectedDevice = this.comboBox_captureDevices.SelectedItem as MMDevice;
			float peakVolume = AudioRecorder.GetPeakVolume(selectedDevice);
			this.label_peakVolume.ForeColor = peakVolume > 0.0f ? Color.Green : Color.DarkGray;
			this.label_peakVolume.Text = $"Peak Volume: {peakVolume:F6}";

			// If hueShift checked, apply hue shift to graph color
			if (this.checkBox_hueGraph.Checked)
			{
				this.GetHueShiftedColor();
			}

			// Dispose previous image
			this.pictureBox_waveform.Image = await this.SelectedTrack.GetWaveformImageSimpleAsync(null, this.pictureBox_waveform.Width, this.pictureBox_waveform.Height, (int) this.numericUpDown_samplesPerPixel.Value, graphColor: this.audioCollection.GraphColor, backgroundColor: this.audioCollection.BackColor);
			this.pictureBox_waveform.Invalidate();
			GC.Collect();
		}

		private void RegisterNumericToSecondPow(NumericUpDown numeric)
		{
			// Initialwert speichern
			this.previousNumericValues.Add(numeric, (int) numeric.Value);

			numeric.ValueChanged += (s, e) =>
			{
				// No recursive calls
				if (this.isProcessing)
				{
					return;
				}

				this.isProcessing = true;

				try
				{
					long newValue = (long) numeric.Value;
					long oldValue = this.previousNumericValues[numeric];
					long max = (int) numeric.Maximum;
					long min = (int) numeric.Minimum;

					// Only process if changed
					if (newValue != oldValue)
					{
						long calculatedValue;

						if (newValue > oldValue)
						{
							// Double but not beyond max
							calculatedValue = Math.Min(oldValue * 2, max);
						}
						else if (newValue < oldValue)
						{
							// Halve but not beneath min
							calculatedValue = Math.Max(oldValue / 2, min);
						}
						else
						{
							calculatedValue = oldValue;
						}

						// Only refresh if necessary
						if (calculatedValue != newValue)
						{
							numeric.Value = calculatedValue;
						}

						this.previousNumericValues[numeric] = calculatedValue;
					}
				}
				finally
				{
					this.isProcessing = false;
				}
			};
		}

		private void UpdateGraphColorButton()
		{
			this.button_graphColor.BackColor = this.audioCollection.GraphColor;
			if (this.button_graphColor.BackColor.GetBrightness() < 0.5f)
			{
				this.button_graphColor.ForeColor = Color.White;
			}
			else
			{
				this.button_graphColor.ForeColor = Color.Black;
			}
		}

		private void SetupContextMenuForListBox()
		{
			// Erstelle das Kontextmenü-Element
			ContextMenuStrip contextMenu = new();

			// Erstelle den Menüpunkt "Entfernen"
			ToolStripMenuItem removeItem = new("Remove");

			// Registriere das asynchrone Event für den Klick
			removeItem.Click += async (sender, e) =>
			{
				// Prüfe, ob ein Track ausgewählt ist
				if (this.SelectedTrack != null)
				{
					// Rufe die asynchrone Entfernen-Methode auf
					await this.audioCollection.RemoveAsync(this.SelectedTrack);

					// Optional: UI-Update, z.B. die ListBox aktualisieren
					this.FillTracksListBox();
					this.UpdateInfoView();
					GC.Collect();
				}
			};

			// Füge den Menüpunkt dem Kontextmenü hinzu
			contextMenu.Items.Add(removeItem);

			// Weise das Kontextmenü deiner ListBox zu
			this.listBox_tracks.ContextMenuStrip = contextMenu;
		}

		private async Task StretchAllParallel()
		{
			float target = (float) this.numericUpDown_targetBpm.Value;
			int chunkSize = (int) this.numericUpDown_chunkSize.Value;
			float overlap = (float) this.numericUpDown_overlap.Value;
			string kernel = this.comboBox_stretchKernels.SelectedItem?.ToString() ?? string.Empty;

			var lastUpdate = DateTime.MinValue;
			var progressHandler = new Progress<int>(value =>
			{
				if (DateTime.Now - lastUpdate < TimeSpan.FromMilliseconds(100))
				{
					return;
				}

				lastUpdate = DateTime.Now;
				if (this.progressBar_processing.InvokeRequired)
				{
					this.progressBar_processing.Invoke(new Action(() => this.progressBar_processing.Increment(value)));
				}
				else
				{
					this.progressBar_processing.Increment(value);
				}
			});

			// Create task for each track
			List<Task> stretchTasks = [];
			int chunkCount = 0;
			foreach (var track in this.audioCollection.Tracks)
			{
				if (track.Bpm < 60)
				{
					this.Log("Stretch error", $"Track '{track.Name}' has a BPM below 60, skipping", true);
					continue;
				}

				chunkCount += (int) Math.Ceiling((double) track.Length / chunkSize);

				double factor = (double) (track.Bpm / target);

				stretchTasks.Add(Task.Run(() => this.openClService.TimeStretch(track, kernel, "", factor, chunkSize, overlap, progressHandler)));
			}

			// Set progress bar (max = chunkCount * 6, value = 0)
			this.progressBar_processing.Maximum = chunkCount * 6;
			this.progressBar_processing.Value = 0;
			this.Log($"Proposed chunk count to process: " + chunkCount, $"{stretchTasks.Count} tracks");

			// Wait for all tasks to complete (iterate through each track!)
			foreach (var task in stretchTasks)
			{
				try
				{
					await task;
				}
				catch (Exception ex)
				{
					this.Log("Stretch error", ex.Message, true);
				}
			}

			// Reset progress bar
			if (this.progressBar_processing.InvokeRequired)
			{
				this.progressBar_processing.Invoke(new Action(() => this.progressBar_processing.Value = 0));
			}
			else
			{
				this.progressBar_processing.Value = 0;
			}

			// Update view
			this.FillTracksListBox();
		}

		private async Task StretchAll()
		{
			string kernelName = this.comboBox_stretchKernels.SelectedItem?.ToString() ?? string.Empty;

			// Unselect track to prevent operations on it while processing + disable listbox
			int selectedIndex = this.listBox_tracks.SelectedIndex;
			this.listBox_tracks.SelectedIndex = -1;
			this.listBox_tracks.Enabled = false;

			ConcurrentBag<AudioObj> selectedTracks = [];

			foreach (var track in this.audioCollection.Tracks)
			{
				if (track.Bpm < 60)
				{
					this.Log("Stretch error", $"Track '{track.Name}' has a BPM below 60, skipping", true);
					continue;
				}

				// Add track to bag
				selectedTracks.Add(track);

				double factor = (double) (track.Bpm / (double) this.numericUpDown_targetBpm.Value);
				int chunkSize = (int) this.numericUpDown_chunkSize.Value;
				float overlap = (float) this.numericUpDown_overlap.Value;

				int max = (int) (track.Length / chunkSize) * 6;
				this.progressBar_processing.Maximum = max;
				this.progressBar_processing.Value = 0;
				var progressHandler = new Progress<int>(value =>
				{
					if (this.progressBar_processing.InvokeRequired)
					{
						this.progressBar_processing.Invoke(new Action(() => this.progressBar_processing.Increment(value)));
					}
					else
					{
						this.progressBar_processing.Increment(value);
					}
				});

				// Stop playback if running
				if (track.Playing)
				{
					track.Stop();
					this.playbackTimer.Stop();
					this.button_playback.Text = "▶";
					this.Log("Playback stopped ■", track.Name);
				}

				this.Log($"Started stretching '{track.Name}' ({track.Bpm} BPM) to {this.numericUpDown_targetBpm.Value} BPM with factor {factor:F2}", $"{chunkSize} samples, {overlap:F2} overlap");

				// Call time stretch
				var result = await this.openClService.TimeStretch(track, kernelName, "", factor, chunkSize, overlap, progressHandler);

				// Update info
				this.UpdateInfoView();
				this.Log($"Successfully stretched '{track.Name}'", $"{track.ElapsedProcessingTime:F1} ms elapsed");

				// Reset progress bar
				if (this.progressBar_processing.InvokeRequired)
				{
					this.progressBar_processing.Invoke(new Action(() => this.progressBar_processing.Value = 0));
				}
				else
				{
					this.progressBar_processing.Value = 0;
				}
			}

			// Re-enable listbox + select previous track
			this.listBox_tracks.Enabled = true;
			if (selectedIndex >= 0 && selectedIndex < this.listBox_tracks.Items.Count)
			{
				this.listBox_tracks.SelectedIndex = selectedIndex;
			}

			// Log total processed tracks + time
			int count = selectedTracks.Count;
			long totalTime = (long) selectedTracks.Sum(t => t.ElapsedProcessingTime);
			double totalSeconds = totalTime / 1000.0;
			this.Log($"Total processed tracks: {count}, Total processing time: {totalSeconds:F1} seconds");

			// Optionally: Export all processed tracks
			if (this.checkBox_autoExport.Checked)
			{
				// Create output directory if not exists
				string outputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "AsynCLAudio", "Output");
				if (!Directory.Exists(outputDir))
				{
					Directory.CreateDirectory(outputDir);
					this.Log("Created output directory", outputDir);
				}

				foreach (var track in selectedTracks)
				{
					try
					{
						string exportPath = Path.Combine(outputDir, $"{track.Name} [{track.Bpm:F2} BPM].wav");
						await track.Export(exportPath);
						this.Log($"Exported '{track.Name}' to {exportPath}");
					}
					catch (Exception ex)
					{
						this.Log("Export error", $"Failed to export '{track.Name}': {ex.Message}", true);
					}
				}
			}

			// Play windows complement sound
			System.Media.SystemSounds.Asterisk.Play();
		}



		// ----- Events ----- \\
		private async void button_import_Click(object sender, EventArgs e)
		{
			// OFD at MyMusic
			OpenFileDialog ofd = new()
			{
				Title = "Import Audio File(s)",
				InitialDirectory = (this.audioCollection.Count > 0 ? Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) : Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\AsynCLAudio\\Input"),
				Multiselect = true,
				Filter = "Audio Files|*.mp3;*.wav;*.flac;",
			};

			if (ofd.ShowDialog() == DialogResult.OK)
			{
				this.Log("Started importing track(s)", string.Join(", ", ofd.FileNames));
				List<AudioObj> importedTracks = [];
				foreach (string filePath in ofd.FileNames)
				{
					try
					{
						var audio = await this.audioCollection.ImportAsync(filePath, true);
						if (audio != null)
						{
							importedTracks.Add(audio);
						}
					}
					catch (Exception ex)
					{
						MessageBox.Show($"Error importing {filePath}: {ex.Message}", "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}

				// Fill tracks
				if (this.SelectedTrack != null && this.SelectedTrack.Playing)
				{
					this.listBox_tracks.Items.AddRange(importedTracks.Select(t => t.Name).ToArray());
				}
				else
				{
					this.FillTracksListBox();
				}

				this.Log("Successfully imported track(s)", ofd.FileNames.Length.ToString());
			}
		}

		private async void button_export_Click(object sender, EventArgs e)
		{
			// Check if a track is selected
			if (this.SelectedTrack == null)
			{
				this.Log("Import error", "Please select a track first", true);
				return;
			}

			// SFD at MyMusic
			SaveFileDialog sfd = new()
			{
				Title = "Export Audio File",
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
				Filter = "Audio Files|*.mp3;*.wav;*.flac;",
				DefaultExt = "wav",
				FileName = this.SelectedTrack.Name + " [" + this.SelectedTrack.Bpm.ToString("F2") + "]"
			};

			if (sfd.ShowDialog() == DialogResult.OK)
			{
				this.Log("Started exporting track", sfd.FileName);

				try
				{
					await this.SelectedTrack.Export(sfd.FileName);
					this.Log("Successfully exported track", sfd.FileName);
				}
				catch (Exception ex)
				{
					this.Log("Export error", ex.Message, true);
				}
			}
		}

		private async void button_playback_Click(object sender, EventArgs e)
		{
			// Check selected track
			if (this.SelectedTrack == null)
			{
				return;
			}

			if (this.SelectedTrack.Playing)
			{
				this.playbackTimer.Stop();
				this.SelectedTrack.Stop();
				this.button_playback.Text = "▶";
				this.playbackTimer.Elapsed -= (sender, e) => this.UpdateWaveform().Wait();
				this.Log("Playback stopped ■", this.SelectedTrack.Name);
			}
			else
			{
				this.playbackTimer.Elapsed += (sender, e) => this.UpdateWaveform().GetAwaiter().GetResult();
				this.playbackTimer.Start();
				this.playbackCancellationToken = new();
				await this.SelectedTrack.Play(this.playbackCancellationToken.Value, null, this.Volume);
				this.button_playback.Text = "■";
				this.Log("Playback started ▶", this.SelectedTrack.Name);

				if (this.recording)
				{
					AudioRecorder.GetActivePlaybackDevice();
				}
			}
		}

		private void numericUpDown_initialBpm_ValueChanged(object sender, EventArgs e)
		{
			// Adjust factor
			this.numericUpDown_stretchFactor.Value = this.numericUpDown_initialBpm.Value / this.numericUpDown_targetBpm.Value;
		}

		private void numericUpDown_targetBpm_ValueChanged(object sender, EventArgs e)
		{
			// Adjust factor
			this.numericUpDown_stretchFactor.Value = Math.Min(this.numericUpDown_stretchFactor.Maximum, this.numericUpDown_initialBpm.Value / this.numericUpDown_targetBpm.Value);
		}

		private void numericUpDown_stretchFactor_ValueChanged(object sender, EventArgs e)
		{
			// Adjust target BPM
			if (this.numericUpDown_initialBpm.Value > 0)
			{
				this.numericUpDown_targetBpm.Value = Math.Min(this.numericUpDown_targetBpm.Maximum, Math.Max(this.numericUpDown_targetBpm.Minimum, this.numericUpDown_initialBpm.Value / this.numericUpDown_stretchFactor.Value));
			}
			else
			{
				this.numericUpDown_targetBpm.Value = 0;
			}
		}

		private async void button_stretch_Click(object sender, EventArgs e)
		{
			// If CTRL down, stretch all tracks
			if ((ModifierKeys & Keys.Control) == Keys.Control)
			{
				await this.StretchAll();
				return;
			}

			// Check selected track
			if (this.SelectedTrack == null)
			{
				this.Log("Stretch error", "Please select a track to stretch", true);
				return;
			}

			// Check selected kernel
			string? kernelName = this.comboBox_stretchKernels.SelectedItem?.ToString();
			if (string.IsNullOrEmpty(kernelName))
			{
				this.Log("Stretch error", "Please select a stretching kernel", true);
				return;
			}
			string kernelVersion = kernelName.Substring(kernelName.Length - 2, 2);

			var track = this.SelectedTrack;

			// Get ctrl flag
			bool ctrlPressed = (ModifierKeys & Keys.Control) == Keys.Control;

			// Stop playback if running
			if (track.Playing)
			{
				track.Stop();
				this.playbackTimer.Stop();
				this.button_playback.Text = "▶";
				this.Log("Playback stopped ■", this.SelectedTrack.Name);
			}

			// Unselect track to prevent operations on it while processing + disable listbox
			int selectedIndex = this.listBox_tracks.SelectedIndex;
			this.listBox_tracks.SelectedIndex = -1;
			this.listBox_tracks.Enabled = false;

			double factor = (double) this.numericUpDown_stretchFactor.Value;
			int chunkSize = (int) this.numericUpDown_chunkSize.Value;
			float overlap = (float) this.numericUpDown_overlap.Value;

			// Calculate max + progress handler
			int max = (int) (track.Length / chunkSize) * 6; // FFT, stretch, IFFT
			this.progressBar_processing.Maximum = max;
			this.progressBar_processing.Value = 0;
			var progressHandler = new Progress<int>(value =>
			{
				if (this.progressBar_processing.InvokeRequired)
				{
					this.progressBar_processing.Invoke(new Action(() => this.progressBar_processing.Increment((int) value)));
				}
				else
				{
					this.progressBar_processing.Increment((int) value);
				}
			});

			await track.Normalize();

			this.Log("Started stretching (" + kernelName + ")", (int) (max / 6) + " chunks, " + track.SizeInMb.ToString("F1") + " MB");

			// Call time stretch
			var result = await this.openClService.TimeStretch(track, kernelName, "", factor, chunkSize, overlap, progressHandler);

			// Update info
			this.UpdateInfoView();

			// Reset progress bar
			if (this.progressBar_processing.InvokeRequired)
			{
				this.progressBar_processing.Invoke(new Action(() => this.progressBar_processing.Value = 0));
			}
			else
			{
				this.progressBar_processing.Value = 0;
			}

			// Optionally: Export track
			if (this.checkBox_autoExport.Checked)
			{
				// Create output directory if not exists
				string outputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "AsynCLAudio", "Output");
				if (!Directory.Exists(outputDir))
				{
					Directory.CreateDirectory(outputDir);
					this.Log("Created output directory", outputDir);
				}

				await track.Export(outputDir);

				this.Log($"Exported '{track.Name}' to {outputDir}");
			}

			// Re-enable listbox + select previous track
			this.listBox_tracks.Enabled = true;
			if (selectedIndex >= 0 && selectedIndex < this.listBox_tracks.Items.Count)
			{
				this.listBox_tracks.SelectedIndex = selectedIndex;
			}

			// Play windows complement sound
			System.Media.SystemSounds.Asterisk.Play();

			this.Log("Successfully stretched '" + track.Name + "'", track.ElapsedProcessingTime.ToString("F1") + " ms elapsed");
		}

		private async void button_reset_Click(object sender, EventArgs e)
		{
			// Check selected track
			if (this.SelectedTrack == null)
			{
				return;
			}

			// Stop playback if running
			if (this.SelectedTrack.Playing)
			{
				this.SelectedTrack.Stop();
				this.playbackTimer.Stop();
				this.button_playback.Text = "▶";
				this.Log("Playback stopped ■", this.SelectedTrack.Name);
			}

			this.Log("Started reloading", this.SelectedTrack.Name);

			await Task.Run(() =>
			{
				// Reset track
				this.SelectedTrack.LoadAudioFile();
			});

			// Update info
			this.UpdateInfoView();

			// Reset progress bar
			if (this.progressBar_processing.InvokeRequired)
			{
				this.progressBar_processing.Invoke(new Action(() => this.progressBar_processing.Value = 0));
			}
			else
			{
				this.progressBar_processing.Value = 0;
			}

			// Play windows complement sound
			System.Media.SystemSounds.Asterisk.Play();

			this.Log("Successfully reloaded", this.SelectedTrack.Name);
		}

		private async void vScrollBar_volume_Scroll(object sender, ScrollEventArgs e)
		{
			int value = this.vScrollBar_volume.Value;
			float volume = 1.0f - value / 100f;

			if (this.SelectedTrack != null)
			{
				await this.SelectedTrack.SetVolume(volume);
			}
		}

		private async void button_normalize_Click(object sender, EventArgs e)
		{
			if (this.SelectedTrack == null)
			{
				this.Log("Normalize error", "Please select a track to normalize", true);
				return;
			}

			// Stop playback if running
			if (this.SelectedTrack.Playing)
			{
				this.SelectedTrack.Stop();
				this.playbackTimer.Stop();
				this.button_playback.Text = "▶";
				this.Log("Playback stopped ■", this.SelectedTrack.Name);
			}

			this.Log("Started normalizing", this.SelectedTrack.Name);
			try
			{
				await this.SelectedTrack.Normalize();
				this.Log("Successfully normalized", this.SelectedTrack.Name);
			}
			catch (Exception ex)
			{
				this.Log("Normalize error", ex.Message, true);
			}
		}

		private void button_record_Click(object sender, EventArgs e)
		{
			string outDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "AsynCLAudio", "Recorded");
			if (!Directory.Exists(outDir))
			{
				Directory.CreateDirectory(outDir);
				this.Log("Created output directory", outDir);
			}

			string fileName = Path.Combine(outDir, $"LiveRecording_{DateTime.Now:ddMMyyyy_HHmmss}.wav");

			if (!this.recording)
			{
				this.comboBox_captureDevices.Enabled = false;
				this.button_record.ForeColor = Color.Red;
				this.textBox_recording.ForeColor = Color.Red;
				this.Log("Started recording...");
				this.recordingTime = DateTime.Now;

				// Timer starten, wenn die Aufnahme beginnt
				this.recordingTimer.Start();

				MMDevice? selectedDevice = this.comboBox_captureDevices.SelectedItem as MMDevice;
				AudioRecorder.StartRecording(fileName, selectedDevice);
			}
			else
			{
				this.button_record.ForeColor = Color.Black;
				this.Log("Stopped recording.", (DateTime.Now - this.recordingTime).ToString("hh\\:mm\\:ss\\.fff"));

				// Timer stoppen, wenn die Aufnahme endet
				this.recordingTimer.Stop();

				this.textBox_recording.ForeColor = Color.Black;

				AudioRecorder.StopRecording();
				this.comboBox_captureDevices.Enabled = true;
			}

			this.recording = !this.recording;
		}

		private void RecordingTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
		{
			// Da das Timer-Event in einem anderen Thread läuft,
			// müssen wir Invoke verwenden, um auf UI-Controls zuzugreifen
			this.Invoke(() =>
			{
				TimeSpan elapsedTime = DateTime.Now - this.recordingTime;
				this.textBox_recording.Text = elapsedTime.ToString(@"mm\:ss\.fff");
			});
		}

		private async void button_pause_Click(object sender, EventArgs e)
		{
			if (this.SelectedTrack != null)
			{
				await this.SelectedTrack.Pause();
			}
		}

		private void button_graphColor_Click(object sender, EventArgs e)
		{
			// If CTRL down -> Randomize graph color
			if ((ModifierKeys & Keys.Control) == Keys.Control)
			{
				this.audioCollection.GraphColor = Color.FromArgb(
					Random.Shared.Next(256),
					Random.Shared.Next(256),
					Random.Shared.Next(256));
				this.button_graphColor.BackColor = this.audioCollection.GraphColor;
				this.Log("Graph color randomized", this.audioCollection.GraphColor.ToString());
				this.UpdateGraphColorButton();
				return;
			}

			// ColorDialog to select graph color
			ColorDialog colorDialog = new()
			{
				AllowFullOpen = true,
				AnyColor = true,
				ShowHelp = true,
				FullOpen = true,
				Color = this.audioCollection.GraphColor
			};

			if (colorDialog.ShowDialog() == DialogResult.OK)
			{
				this.audioCollection.GraphColor = colorDialog.Color;
				this.button_graphColor.BackColor = this.audioCollection.GraphColor;
				this.Log("Graph color changed", this.audioCollection.GraphColor.ToString());
			}

			// Update button color
			this.UpdateGraphColorButton();
		}

		private void checkBox_hueGraph_CheckedChanged(object sender, EventArgs e)
		{
			this.graphHue = Color.Red;

			if (this.checkBox_hueGraph.Checked)
			{
				this.numericUpDown_hueShift.Enabled = true;
			}
			else
			{
				this.numericUpDown_hueShift.Enabled = false;
			}
		}

		private async void numericUpDown_hueShift_ValueChanged(object sender, EventArgs e)
		{
			await this.UpdateWaveform();
		}

		private void numericUpDown_fps_ValueChanged(object sender, EventArgs e)
		{
			// Prevent frequent updates (cooldown = 250 ms)
			if (this.isProcessing)
			{
				return;
			}

			this.isProcessing = true;

			try
			{
				int fps = (int) this.numericUpDown_fps.Value;
				if (fps < 1)
				{
					fps = 1;
				}
				else if (fps > 144)
				{
					fps = 144;
				}

				this.playbackTimer.Interval = 1000 / fps;
			}
			finally
			{
				this.isProcessing = false;
			}
		}

		private Color GetHueShiftedColor()
		{
			Color baseColor = this.audioCollection.GraphColor;
			float shiftDelta = (float) this.numericUpDown_hueShift.Value;

			// Konvertiere RGB zu HSL
			float hue = baseColor.GetHue();
			float saturation = baseColor.GetSaturation();
			float brightness = baseColor.GetBrightness();

			// Wende den Shift an
			hue = (hue + shiftDelta) % 360;
			if (hue < 0)
			{
				hue += 360;
			}

			// Konvertiere HSL zurück zu RGB
			Color newColor = ColorFromHsl(hue, saturation, brightness);

			// Update graph color
			this.audioCollection.GraphColor = newColor;
			this.UpdateGraphColorButton();

			return newColor;
		}

		private static Color ColorFromHsl(float h, float s, float l)
		{
			if (s == 0)
			{
				int l_byte = (int) (l * 255.0f);
				return Color.FromArgb(l_byte, l_byte, l_byte);
			}

			float v1, v2;
			float h_norm = h / 360.0f;

			v2 = (l < 0.5f) ? (l * (1.0f + s)) : ((l + s) - (l * s));
			v1 = 2.0f * l - v2;

			byte r = (byte) (255.0f * HueToRgb(v1, v2, h_norm + (1.0f / 3.0f)));
			byte g = (byte) (255.0f * HueToRgb(v1, v2, h_norm));
			byte b = (byte) (255.0f * HueToRgb(v1, v2, h_norm - (1.0f / 3.0f)));

			return Color.FromArgb(r, g, b);
		}

		private static float HueToRgb(float v1, float v2, float vH)
		{
			if (vH < 0.0f) vH += 1.0f;
			if (vH > 1.0f) vH -= 1.0f;
			if ((6.0f * vH) < 1.0f) return (v1 + (v2 - v1) * 6.0f * vH);
			if ((2.0f * vH) < 1.0f) return v2;
			if ((3.0f * vH) < 2.0f) return (v1 + (v2 - v1) * ((2.0f / 3.0f) - vH) * 6.0f);
			return v1;
		}

		private async void button_backColor_Click(object sender, EventArgs e)
		{
			// ColorDialog to select background color
			ColorDialog colorDialog = new()
			{
				AllowFullOpen = true,
				AnyColor = true,
				ShowHelp = true,
				FullOpen = true,
				Color = this.BackColor
			};

			if (colorDialog.ShowDialog() == DialogResult.OK)
			{
				this.audioCollection.BackColor = colorDialog.Color;
				this.Log("Background color changed", this.BackColor.ToString());

				await this.UpdateWaveform();
			}
		}

		private async void button_strobe_Click(object sender, EventArgs e)
		{
			if (this.button_strobe.ForeColor == Color.Black)
			{
				// Enable strobe effect
				this.button_strobe.ForeColor = Color.Crimson;
				this.Log("Strobe effect enabled", "Click again to disable");

				this.checkBox_hueGraph.Checked = true;
				this.numericUpDown_hueShift.Enabled = true;
				this.numericUpDown_hueShift.Value = 100;
				this.numericUpDown_fps.Value = 75;
				this.audioCollection.BackColor = Color.Black;
			}
			else
			{
				// Disable strobe effect
				this.button_strobe.ForeColor = Color.Black;
				this.Log("Strobe effect disabled", "Click again to enable");
				this.checkBox_hueGraph.Checked = false;
				this.numericUpDown_hueShift.Enabled = false;
				this.numericUpDown_hueShift.Value = 5;
				this.numericUpDown_fps.Value = 30;
				this.audioCollection.BackColor = this.button_backColor.BackColor;
			}

			await this.UpdateWaveform();
		}
	}
}
