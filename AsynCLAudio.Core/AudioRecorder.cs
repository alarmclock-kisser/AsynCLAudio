using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

public static class AudioRecorder
{
	private static WasapiLoopbackCapture? _capture;
	private static MMDevice? _mmDevice;
	public static string CaptureDeviceName => _capture?.WaveFormat.Encoding.ToString() ?? "N/A";
	public static string MMDeviceName => _mmDevice?.FriendlyName ?? "N/A";
	private static WaveFileWriter? _writer;

	public static bool IsRecording { get; private set; } = false;
	public static string? RecordedFile { get; private set; } = null;

	public static float GetPeakVolume(MMDevice? useDevice = null)
	{
		if (useDevice != null)
		{
			try
			{
				return useDevice.AudioMeterInformation.MasterPeakValue;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Fehler beim Abrufen der Lautstärke: {ex.Message}");
				return 0.0f;
			}
		}

		if (_mmDevice == null)
		{
			Console.WriteLine("Kein Gerät ausgewählt.");
			return 0.0f;
		}
		try
		{
			return _mmDevice.AudioMeterInformation.MasterPeakValue;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Fehler beim Abrufen der Lautstärke: {ex.Message}");
			return 0.0f;
		}
	}

	public static void StartRecording(string filePath, MMDevice? mmDevice = null)
	{
		if (IsRecording)
		{
			Console.WriteLine("Aufnahme läuft bereits.");
			return;
		}

		RecordedFile = Path.GetFullPath(filePath);

		try
		{
			MMDevice? captureDevice = null;
			if (mmDevice != null)
			{
				captureDevice = mmDevice;
			}

			// Nutze das gefundene Gerät oder das Standardgerät als Fallback
			_capture = captureDevice != null ? new WasapiLoopbackCapture(captureDevice) : new WasapiLoopbackCapture();

			_writer = new WaveFileWriter(filePath, _capture.WaveFormat);

			_capture.DataAvailable += OnDataAvailable;
			_capture.RecordingStopped += OnRecordingStopped;

			_capture.StartRecording();
			IsRecording = true;
			Console.WriteLine($"Aufnahme gestartet. Gerät: {captureDevice?.FriendlyName ?? "Standard"}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Fehler beim Starten der Aufnahme: {ex.Message}");
			Cleanup();
		}
	}

	/// <summary>
	/// Stoppt die laufende Aufnahme.
	/// </summary>
	public static void StopRecording()
	{
		if (!IsRecording)
		{
			Console.WriteLine("Keine Aufnahme aktiv.");
			return;
		}

		IsRecording = false;

		Console.WriteLine("Aufnahme wird gestoppt...");
		_capture?.StopRecording();
	}

	public static MMDevice? GetActivePlaybackDevice()
	{
		var enumerator = new MMDeviceEnumerator();
		var devices = enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.All);

		MMDevice? activeDevice = null;
		float maxPeak = 0.0f;

		foreach (var device in devices)
		{
			float peak = device.AudioMeterInformation.MasterPeakValue;
			if (peak > maxPeak)
			{
				maxPeak = peak;
				activeDevice = device;
			}
		}

		_mmDevice = activeDevice;
		return activeDevice;
	}

	public static MMDevice? GetDefaultPlaybackDevice()
	{
		var enumerator = new MMDeviceEnumerator();
		return enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
	}

	public static MMDevice[] GetCaptureDevices()
	{
		var enumerator = new MMDeviceEnumerator();
		return enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.All).ToArray();
	}

	public static void SetCaptureDevice(MMDevice? device)
	{
		if (device == null)
		{
			Console.WriteLine("Ungültiges Gerät.");
			return;
		}
		if (_capture != null && IsRecording)
		{
			Console.WriteLine("Aufnahme läuft bereits. Stoppe die Aufnahme, bevor du das Gerät änderst.");
			return;
		}
		try
		{
			_capture?.Dispose();
			_capture = new WasapiLoopbackCapture(device);
			Console.WriteLine($"Gerät auf {device.FriendlyName} gesetzt.");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Fehler beim Setzen des Geräts: {ex.Message}");
		}
	}

	private static void OnDataAvailable(object? sender, WaveInEventArgs e)
	{
		_writer?.Write(e.Buffer, 0, e.BytesRecorded);
	}

	private static void OnRecordingStopped(object? sender, StoppedEventArgs e)
	{
		Console.WriteLine("Aufnahme gestoppt.");

		if (e.Exception != null)
		{
			Console.WriteLine($"Fehler während der Aufnahme: {e.Exception.Message}");
		}

		Cleanup();
	}

	private static void Cleanup()
	{
		_writer?.Dispose();
		_writer = null;
		_capture?.Dispose();
		_capture = null;
		IsRecording = false;
	}
}