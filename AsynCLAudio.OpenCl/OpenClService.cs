using AsynCLAudio.Core;
using OpenTK.Compute.OpenCL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AsynCLAudio.OpenCl
{
	public class OpenClService
	{
		// ----- Services ----- \\
		public OpenClRegister? Register { get; private set; }
		public OpenClCompiler? Compiler { get; private set; }
		public OpenClExecutioner? Executioner { get; private set; }

		// ----- Fields ----- \\
		private Dictionary<CLDevice, CLPlatform> devicesPlatforms = [];

		public int Index { get; private set; } = -1;
		private CLContext? context = null;
		private CLPlatform? platform = null;
		private CLDevice? device = null;

		// ----- Attributes ----- \\
		public int DeviceCount => this.devicesPlatforms.Count;
		public bool Initialized => this.context != null && this.Index >= 0 && this.Register != null && this.Compiler != null && this.Executioner != null;

		public CLResultCode lastError = CLResultCode.Success;

		// ----- Constructor ----- \\
		public OpenClService()
		{
			this.GetDevicesPlatforms();
		}

		// ----- Methods ----- \\
		public void Dispose()
		{
			this.Register?.Dispose();
			this.Register = null;
			this.Compiler?.Dispose();
			this.Compiler = null;
			this.Executioner?.Dispose();
			this.Executioner = null;

			if (this.context != null)
			{
				this.lastError = CL.ReleaseContext(this.context.Value);
			}
			this.context = null;
			this.device = null;
			this.platform = null;
			this.Index = -1;
		}

		private void GetDevicesPlatforms()
		{
			CLPlatform[] platforms = [];
			CL.GetPlatformIds(out platforms);

			for (int i = 0; i < platforms.Length; i++)
			{
				CLDevice[] devices = [];
				CL.GetDeviceIds(platforms[i], DeviceType.All, out devices);
				foreach (var device in devices)
				{
					if (!this.devicesPlatforms.ContainsKey(device))
					{
						this.devicesPlatforms.Add(device, platforms[i]);
					}
				}
			}
		}

		public string? GetDeviceInfo(int deviceId = -1, DeviceInfo info = DeviceInfo.Name)
		{
			// Verify device
			CLDevice? device = null;
			if (deviceId < 0)
			{
				device = this.device;
			}
			else if (deviceId >= 0 && deviceId < this.devicesPlatforms.Count)
			{
				device = this.devicesPlatforms.Keys.ElementAt(deviceId);
			}
			if (device == null)
			{
				return null;
			}

			this.lastError = CL.GetDeviceInfo(device.Value, info, out byte[] infoCode);
			if (this.lastError != CLResultCode.Success || infoCode == null || infoCode.LongLength == 0)
			{
				return null;
			}

			return Encoding.UTF8.GetString(infoCode).Trim('\0');
		}

		public string? GetPlatformInfo(int platformId = -1, PlatformInfo info = PlatformInfo.Name)
		{
			// Verify platform
			CLPlatform? platform = null;
			if (platformId < 0)
			{
				platform = this.platform;
			}
			else if (platformId >= 0 && platformId < this.devicesPlatforms.Count)
			{
				platform = this.devicesPlatforms.Values.ElementAt(platformId);
			}
			if (platform == null)
			{
				return null;
			}

			this.lastError = CL.GetPlatformInfo(platform.Value, info, out byte[] infoCode);
			if (this.lastError != CLResultCode.Success || infoCode == null || infoCode.LongLength == 0)
			{
				return null;
			}

			return Encoding.UTF8.GetString(infoCode).Trim('\0');
		}

		public IEnumerable<string> GetDeviceEntries()
		{
			int count = this.DeviceCount;
			List<string> entries = [];
			for (int i = 0; i < count; i++)
			{
				string device = this.GetDeviceInfo(i) ?? "N/A";
				string platform = this.GetPlatformInfo(i) ?? "N/A";

				entries.Add($"[{i}] {device} ({platform})");
			}

			return entries;
		}

		public void Initialize(int index = 0)
		{
			this.Dispose();

			this.GetDevicesPlatforms();

			if (index < 0 || index >= this.devicesPlatforms.Count)
			{
				return;
			}

			this.Index = index;
			this.device = this.devicesPlatforms.Keys.ElementAt(index);
			this.platform = this.devicesPlatforms.Values.ElementAt(index);

			this.context = CL.CreateContext(0, [this.device.Value], 0, IntPtr.Zero, out CLResultCode error);
			if (error != CLResultCode.Success || this.context == null)
			{
				this.lastError = error;
				return;
			}

			this.Register = new OpenClRegister(this.context.Value, this.device.Value);
			this.Compiler = new OpenClCompiler(this.context.Value, this.device.Value, this.Register);
			this.Executioner = new OpenClExecutioner(this.context.Value, this.device.Value, this.Register, this.Compiler);

			this.Index = index;
		}

		public void Initialize(string deviceName)
		{
			var deviceNames = this.devicesPlatforms.Keys
				.Select(d => this.GetDeviceInfo(this.devicesPlatforms.Keys.ToList().IndexOf(d), DeviceInfo.Name))
				.ToList();

			var foundDeviceName = deviceNames.FirstOrDefault(name =>
				!string.IsNullOrEmpty(name) && name.ToLower().Contains(deviceName.ToLower()));

			int index = -1;
			if (foundDeviceName != null)
			{
				index = deviceNames.IndexOf(foundDeviceName);
			}

			this.Initialize(index);
		}


		// ----- ACCESSORS ----- \\
		public async Task<AudioObj> MoveAudio(AudioObj obj, int chunkSize = 16384, float overlap = 0.5f, bool keep = false)
		{
			if (this.Register == null)
			{
				Console.WriteLine("Memory Register is not initialized.");
				return obj;
			}

			try
			{
				List<float[]> chunks = [];

				// -> Device
				if (obj.OnHost)
				{
					chunks = (await obj.GetChunks(chunkSize, overlap, keep)).ToList();
					if (chunks.Count <= 0)
					{
						Console.WriteLine("Failed to get audio chunks from AudioObj.");
						return obj;
					}

					var mem = this.Register.PushChunks<float>(chunks);
					if (mem == null)
					{
						Console.WriteLine("Failed to push audio chunks to OpenCL memory.");
						return obj;
					}

					long memIndexHandle = mem[0].Handle;
					if (memIndexHandle == 0)
					{
						Console.WriteLine("Failed to parse memory index handle.");
						return obj;
					}

					obj.Pointer = (nint) memIndexHandle;
				}
				else if (obj.OnDevice)
				{
					if (obj.Form == "c")
					{
						// obj.ComplexChunks = this.Register.PullChunks<Complex>(obj.Pointer);
					}
					else if (obj.Form == "f")
					{
						chunks = this.Register.PullChunks<float>(obj.Pointer);

						await obj.AggregateStretchedChunks(chunks);
					}
				}
				else
				{
					Console.WriteLine("Error: AudioObj is neither on Host nor on Device.");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error moving audio to OpenCL memory: {ex.Message}");
				return obj;
			}
			finally
			{
				await Task.Yield();
			}

			return obj;
		}

		public async Task<AudioObj> ExecuteAudioKernel(AudioObj obj, string kernelName = "normalize", string version = "00", int chunkSize = 0, float overlap = 0.0f, Dictionary<string, object>? optionalArguments = null, bool log = false, IProgress<int>? progress = null)
		{
			// Check executioner
			if (this.Executioner == null)
			{
				Console.WriteLine("Kernel executioner not initialized (Cannot execute audio kernel)");
				return obj;
			}

			// Take time
			Stopwatch sw = Stopwatch.StartNew();

			// Optionally move audio to device
			bool moved = false;
			if (obj.OnHost)
			{
				await this.MoveAudio(obj, chunkSize, overlap);
				moved = true;
			}
			if (!obj.OnDevice)
			{
				return obj;
			}

			// Execute kernel on device
			double factor = 1.0d;

			if (progress == null)
			{
				obj.Pointer = this.Executioner.ExecuteAudioKernel(
					(nint) obj.Pointer,
					out factor, // Hier wird der 'out' Parameter für die synchrone Methode verwendet
					obj.Length,
					kernelName,
					version,
					chunkSize,
					overlap,
					obj.SampleRate,
					obj.BitDepth,
					obj.Channels,
					optionalArguments
				);
			}
			else
			{
				// Die asynchrone Methode gibt ein Tupel zurück, das wir direkt entpacken.
				// Das 'out' Keyword wird hier entfernt.
				(obj.Pointer, factor) = await this.Executioner.ExecuteAudioKernelAsync(
					(nint) obj.Pointer,
					obj.Length,
					kernelName,
					version,
					chunkSize,
					overlap,
					obj.SampleRate,
					obj.BitDepth,
					obj.Channels,
					optionalArguments,
					progress
				);
			}

			if (obj.Pointer == IntPtr.Zero && log)
			{
				// Console.WriteLine("Failed to execute audio kernel", "Pointer=" + obj.Pointer.ToString("X16"), 1);
			}

			// Reload kernel
			this.Compiler?.LoadKernel(kernelName + version, "");

			// Log factor & set new bpm
			if (factor != 1.00f)
			{
				// IMPORTANT: Set obj Factor
				obj.StretchFactor = factor;
				obj.UpdateBpm((float) (obj.Bpm / factor));
				// Console.WriteLine("Factor for audio kernel: " + factor + " Pointer=" + obj.Pointer.ToString("X16") + " BPM: " + obj.Bpm);
			}

			// Move back optionally
			if (moved && obj.OnDevice && obj.Form.StartsWith("f"))
			{
				await this.MoveAudio(obj, chunkSize, overlap);
			}

			// Log execution time
			sw.Stop();
			obj.ElapsedProcessingTime = (float) sw.Elapsed.TotalMilliseconds;

			return obj;
		}

		public async Task<AudioObj> PerformFFT(AudioObj obj, string version = "01", int chunkSize = 0, float overlap = 0.0f, bool keep = false)
		{
			// Optionally move audio to device
			bool moved = false;
			if (obj.OnHost)
			{
				await this.MoveAudio(obj, chunkSize, overlap, keep);
				moved = true;
			}
			if (!obj.OnDevice)
			{
				return obj;
			}

			// Perform FFT on device
			obj.Pointer = this.Executioner?.ExecuteFFT((nint) obj.Pointer, version, obj.Form.FirstOrDefault(), chunkSize, overlap, true) ?? obj.Pointer;

			if (obj.Pointer == IntPtr.Zero)
			{
				Console.WriteLine("Failed to perform FFT", "Pointer=" + obj.Pointer.ToString("X16"), 1);
			}
			else
			{
				obj.Form = obj.Form.StartsWith("f") ? "c" : "f";
			}

			if (moved)
			{
				// await this.MoveAudio(obj);
			}

			return obj;
		}

		public async Task<AudioObj> TimeStretch(AudioObj obj, string kernelName = "timestretch_double", string version = "03", double factor = 1.000d, int chunkSize = 16384, float overlap = 0.5f, IProgress<int>? progress = null)
		{
			if (this.Executioner == null)
			{
				Console.WriteLine("Kernel executioner is not initialized.");
				return obj;
			}

			kernelName = kernelName + version;

			try
			{
				// Optionally move obj to device
				bool moved = false;
				if (obj.OnHost)
				{
					IntPtr pointer = (await this.MoveAudio(obj, chunkSize, overlap, false)).Pointer;
					if (pointer == IntPtr.Zero)
					{
						Console.WriteLine("Failed to move audio to device memory.");
						return obj;
					}
					moved = true;
				}

				// Get optional args
				Dictionary<string, object> optionalArgs;
				if (kernelName.ToLower().Contains("double"))
				{
					// Double kernel
					optionalArgs = new()
						{
							{ "factor", (double) factor }
						};
				}
				else
				{
					optionalArgs = new()
						{
							{ "factor", (float) factor }
						};
				}

				// Execute time stretch kernel
				var ptr = (await this.ExecuteAudioKernel(obj, kernelName, "", chunkSize, overlap, optionalArgs, true, progress)).Pointer;
				if (ptr == IntPtr.Zero)
				{
					Console.WriteLine("Failed to execute time stretch kernel.", "Pointer=" + ptr.ToString("X16"));
					return obj;
				}

				// Optionally move obj back to host
				if (moved && obj.OnDevice)
				{
					IntPtr resultPointer = (await this.MoveAudio(obj, chunkSize, overlap)).Pointer;
					if (resultPointer != IntPtr.Zero)
					{
						Console.WriteLine("Failed to move audio back to host memory.");
						return obj;
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error during time stretch: {ex.Message}");
			}
			finally
			{
				await Task.Yield();
			}

			return obj;
		}

	}
}
