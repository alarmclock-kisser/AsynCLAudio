using AsynCLAudio.Core;
using AsynCLAudio.OpenCl;

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
		private System.Timers.Timer playbackTimer = new(50);

		public WindowMain(AudioCollection audioCollection, OpenClService openClService)
		{
			this.audioCollection = audioCollection;
			this.openClService = openClService;

			this.InitializeComponent();

			// Event for right-click on entry -> remove context menu (selected track)
			this.SetupContextMenuForListBox();
			this.RegisterNumericToSecondPow(this.numericUpDown_chunkSize);
			this.listBox_tracks.SelectedIndexChanged += (sender, e) => this.UpdateInfoView();
			this.FillDevicesComboBox(2);
			this.UpdateInfoView();
		}

		// ----- Methods ----- \\
		protected override void Dispose(bool disposing = true)
		{
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
				if (this.components != null)
				{
					this.components.Dispose();
				}
			}
			base.Dispose(disposing);
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
			}
		}

		private void FillTracksListBox(bool keepSelection = true)
		{
			int selectedIndex = this.listBox_tracks.SelectedIndex;
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
				this.button_stretch.Enabled = false;
				this.pictureBox_waveform.Image = null;
				this.button_reset.Enabled = false;
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

			// Dispose previous image
			if (this.pictureBox_waveform.Image != null)
			{
				this.pictureBox_waveform.Image.Dispose();
				this.pictureBox_waveform.Image = null;

				GC.Collect();
			}
			this.pictureBox_waveform.Image = await this.SelectedTrack.GetWaveformImageSimpleAsync(null, this.pictureBox_waveform.Width, this.pictureBox_waveform.Height, (int) this.numericUpDown_samplesPerPixel.Value);
			this.pictureBox_waveform.Invalidate();
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
					this.UpdateInfoView() ;
					GC.Collect();
				}
			};

			// Füge den Menüpunkt dem Kontextmenü hinzu
			contextMenu.Items.Add(removeItem);

			// Weise das Kontextmenü deiner ListBox zu
			this.listBox_tracks.ContextMenuStrip = contextMenu;
		}





		// ----- Events ----- \\
		private async void button_import_Click(object sender, EventArgs e)
		{
			// OFD at MyMusic
			OpenFileDialog ofd = new()
			{
				Title = "Import Audio File(s)",
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
				Multiselect = true,
				Filter = "Audio Files|*.mp3;*.wav;*.flac;"
			};

			if (ofd.ShowDialog() == DialogResult.OK)
			{
				foreach (string filePath in ofd.FileNames)
				{
					try
					{
						var audio = await this.audioCollection.ImportAsync(filePath);
					}
					catch (Exception ex)
					{
						MessageBox.Show($"Error importing {filePath}: {ex.Message}", "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}

				// Fill tracks
				this.FillTracksListBox();

				// Select last entry
				this.listBox_tracks.SelectedIndex = this.listBox_tracks.Items.Count - 1;
			}
		}

		private async void button_export_Click(object sender, EventArgs e)
		{
			// Check if a track is selected
			if (this.SelectedTrack == null)
			{
				MessageBox.Show("Please select a track to export.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
				try
				{
					await this.SelectedTrack.Export(sfd.FileName);
					MessageBox.Show($"Track exported successfully to {sfd.FileName}", "Export Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				catch (Exception ex)
				{
					MessageBox.Show($"Error exporting track: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

			float volume = 1.0f;

			if (this.SelectedTrack.Playing)
			{
				this.playbackTimer.Stop();
				this.SelectedTrack.Stop();
				this.button_playback.Text = "▶";
				this.playbackTimer.Elapsed -= (sender, e) => this.UpdateWaveform().Wait();
			}
			else
			{
				this.playbackTimer.Elapsed += (sender, e) => this.UpdateWaveform().GetAwaiter().GetResult();
				this.playbackTimer.Start();
				this.playbackCancellationToken = new();
				await this.SelectedTrack.Play(this.playbackCancellationToken.Value, null, volume);
				this.button_playback.Text = "■";
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
			// Check selected track
			if (this.SelectedTrack == null)
			{
				MessageBox.Show("Please select a track to stretch.", "Stretch Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			// Stop playback if running
			if (this.SelectedTrack.Playing)
			{
				this.SelectedTrack.Stop();
				this.playbackTimer.Stop();
				this.button_playback.Text = "▶";
			}

			// Check selected kernel
			string? kernelName = this.comboBox_stretchKernels.SelectedItem?.ToString();
			if (string.IsNullOrEmpty(kernelName))
			{
				MessageBox.Show("Please select a stretching kernel.", "Stretch Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			string kernelVersion = kernelName.Substring(kernelName.Length - 2, 2);

			var track = this.SelectedTrack;

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

			// Re-enable listbox + select previous track
			this.listBox_tracks.Enabled = true;
			if (selectedIndex >= 0 && selectedIndex < this.listBox_tracks.Items.Count)
			{
				this.listBox_tracks.SelectedIndex = selectedIndex;
			}
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
			}

			await this.SelectedTrack.ReloadAsync();

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
		}
	}
}
