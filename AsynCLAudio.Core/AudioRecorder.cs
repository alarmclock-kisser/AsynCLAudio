using NAudio.Wave;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace AsynCLAudio.Core
{
	public class AudioRecorder : IDisposable
	{
		private WasapiLoopbackCapture? loopbackCapture;
		private WaveFileWriter? waveWriter;
		private readonly ConcurrentDictionary<long, byte[]> recordedChunks = new();
		private long chunkId = 0;
		private bool isRecording = false;
		public bool Recording => this.isRecording;

		public int SampleRate { get; private set; } = 48000;
		public int BitDepth { get; private set; } = 16;
		public int Channels { get; private set; } = 2;

		// MyMusic + AsynCLAudio + Output
		public string OutputPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "AsynCLAudio", "Output");
		public string OutputFile { get; private set; } = string.Empty;

		public AudioRecorder(int sampleRate = 48000, int bitDepth = 16, int channels = 2)
		{
			this.SampleRate = sampleRate;
			this.BitDepth = bitDepth;
			this.Channels = channels;
			Directory.CreateDirectory(this.OutputPath);
		}

		public async Task StartRecordingAsync()
		{
			if (this.isRecording)
			{
				return;
			}

			this.loopbackCapture = new WasapiLoopbackCapture();
			this.loopbackCapture.DataAvailable += this.OnDataAvailable;
			this.loopbackCapture.RecordingStopped += this.OnRecordingStopped;

			this.isRecording = true;
			this.loopbackCapture.StartRecording();

			await Task.CompletedTask;
		}

		private void OnDataAvailable(object? sender, WaveInEventArgs e)
		{
			byte[] audioChunk = new byte[e.BytesRecorded];
			Buffer.BlockCopy(e.Buffer, 0, audioChunk, 0, e.BytesRecorded);
			this.recordedChunks.TryAdd(this.chunkId++, audioChunk);

			// Optional: Direkt in eine WAV-Datei schreiben
			this.waveWriter?.Write(e.Buffer, 0, e.BytesRecorded);
		}

		private void OnRecordingStopped(object? sender, StoppedEventArgs e)
		{
			this.waveWriter?.Dispose();
			this.waveWriter = null;
			this.isRecording = false;
		}

		public async Task StopRecordingAsync(string fileName = "output")
		{
			if (!this.isRecording)
			{
				return;
			}

			Guid guid = Guid.NewGuid();
			
			fileName = $"{fileName}_{guid:N}.wav";

			string filePath = Path.Combine(this.OutputPath, fileName);
			this.OutputFile = filePath;
			this.waveWriter = new WaveFileWriter(filePath, this.loopbackCapture?.WaveFormat);

			this.loopbackCapture?.StopRecording();
			await Task.CompletedTask;
		}

		public void Dispose()
		{
			this.loopbackCapture?.Dispose();
			this.waveWriter?.Dispose();
		}
	}
}