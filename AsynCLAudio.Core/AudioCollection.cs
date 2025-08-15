using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsynCLAudio.Core
{
	public class AudioCollection
	{
		private ConcurrentDictionary<Guid, AudioObj> tracks = [];
		public IReadOnlyList<AudioObj> Tracks => this.tracks.Values.OrderBy(t => t.CreatedAt).ToList();
		public int Count => this.Tracks.Count;
		public string[] Entries => this.Tracks.Select(t => t.Name).ToArray();
		public string[] Playing => this.tracks.Values.Where(t => t.Playing).Select(t => t.Name).ToArray();

		public Color GraphColor { get; set; } = Color.BlueViolet;
		public Color BackColor { get; set; } = Color.White;

		public AudioObj? this[Guid guid]
		{
			get => this.tracks[guid];
		}

		public AudioObj? this[string name]
		{
			get => this.tracks.Values.FirstOrDefault(t => t.Name.ToLower() == name.ToLower());
		}

		public AudioObj? this[int index]
		{
			get => index >= 0 && index < this.Count ? this.tracks.Values.ElementAt(index) : null;
		}

		public AudioObj? this[IntPtr pointer]
		{
			get => pointer != IntPtr.Zero ? this.tracks.Values.FirstOrDefault(t => t.Pointer == pointer) : null;
		}

		public AudioCollection(Color? graphColor = null)
		{
			this.GraphColor = graphColor ?? Color.BlueViolet;
		}

		public async Task<AudioObj?> ImportAsync(string filePath, bool linearLoad = false)
		{
			AudioObj? obj = null;
			if (linearLoad)
			{
				obj = new AudioObj(filePath, true);
			}
			else
			{
				obj = await AudioObj.CreateAsync(filePath);
			}
			if (obj == null)
			{
				return null;
			}

			// Try add to tracks
			if (!this.tracks.TryAdd(obj.Id, obj))
			{
				obj.Dispose();
				return null;
			}

			return obj;
		}

		public async Task RemoveAsync(AudioObj? obj)
		{
			if (obj == null)
			{
				return;
			}

			// Remove from tracks + Dispose obj
			await Task.Run(() =>
			{
				if (this.tracks.TryRemove(obj.Id, out var removed))
				{
					removed.Dispose();
				}
			});
		}

		public void StopAll(bool remove = false)
		{
			foreach (var track in this.tracks.Values)
			{
				track.Stop();
				if (remove)
				{
					this.tracks.TryRemove(track.Id, out var t);
					t?.Dispose();
				}
			}
		}

		public void SetMasterVolume(float percentage)
		{
			// Ensure percentage is between 0 and 1
			percentage = Math.Clamp(percentage, 0.0f, 1.0f);

			
			foreach (var track in this.tracks.Values)
			{
				int volume = (int) (track.Volume * percentage);
				track.SetVolume(volume);
			}	


		}

		public async Task DisposeAsync()
		{
			await Task.Run(() =>
			{
				foreach (var track in this.tracks.Values)
				{
					track.Dispose();
				}

				this.tracks.Clear();
			});
		}

		public static async Task<AudioObj?> LevelAudioFileAsync(string filePath, float duration = 1.0f, float normalize = 1.0f)
		{
			AudioObj? obj = await AudioObj.CreateAsync(filePath);
			if (obj == null)
			{
				return null;
			}

			await obj.Level(duration, normalize);

			return obj;
		}
	}
}
