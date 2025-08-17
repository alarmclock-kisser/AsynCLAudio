using AsynCLAudio.Core;
using AsynCLAudio.OpenCl;
using NAudio.CoreAudioApi;
using SixLabors.ImageSharp.Formats.Tiff;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Timer = System.Threading.Timer;

namespace AsynCLAudio.Forms
{
	public partial class WindowMain : Form
	{
		private readonly AudioCollection audioCollection;
		private readonly OpenClService openClService;

		private bool disposing = false;

		public AudioObj? SelectedTrack => this.audioCollection[(this.GetSelectedTrackItemSafe()?.ToString() ?? "???")];

		private bool isProcessing = false;
		private Dictionary<NumericUpDown, long> previousNumericValues = [];

		private float Volume => 1.0f - this.vScrollBar_trackVolume.Value / 100f;
		private CancellationTokenSource? playbackCancellationToken;
		private System.Threading.Timer? playbackTimer;
		private Color graphHue = Color.Red;

		private System.Timers.Timer recordingTimer = new(120);
		private DateTime recordingTime = DateTime.Now;

		private readonly float[] beats = [0.25f, 0.5f, 1.0f, 2.0f, 4.0f, 8.0f, 16.0f, 32.0f];
		private float estimatedBeatDuration => this.SelectedTrack != null ? (float) (60.0 / this.SelectedTrack.Bpm) : 1.0f;
		private float currentLoopBeats => this.beats[this.hScrollBar_looping.Value];

		public string masterVolume => "Master" + Environment.NewLine + ((int) (100 - this.vScrollBar_masterVolume.Value)).ToString() + "%";
		public string trackVolume => "Track" + Environment.NewLine + ((int) (100 - this.vScrollBar_trackVolume.Value)).ToString() + "%";

		public WindowMain(AudioCollection audioCollection, OpenClService openClService)
		{
			this.audioCollection = audioCollection;
			this.openClService = openClService;

			this.InitializeComponent();


			this.UpdateGraphColorButton();

			this.BuildLoopButtons();

			// Event for right-click on entry -> remove context menu (selected track)
			this.SetupContextMenuForListBox();
			this.RegisterNumericToSecondPow(this.numericUpDown_chunkSize);
			this.listBox_tracks.SelectedValueChanged += this.ListBoxTracks_SelectedValueChanged;
			this.recordingTimer.Elapsed += this.RecordingTimer_Elapsed;
			this.listBox_log.DoubleClick += this.listBox_log_DoubleClick;
			// this.button_playback.MouseEnter += this.button_playback_MouseEnter;
			this.vScrollBar_masterVolume.ValueChanged += (sender, e) => this.UpdateVolumeLabels();
			this.vScrollBar_trackVolume.ValueChanged += (sender, e) => this.UpdateVolumeLabels();
			this.pictureBox_waveform.MouseWheel += this.pictureBox_waveform_MouseWheel;
			this.FillDevicesComboBox(2);
			this.UpdateInfoView();
		}


		// ----- Methods ----- \\
		protected override void Dispose(bool disposing = true)
		{
			// End recording if running
			if (AudioRecorder.IsRecording)
			{
				AudioRecorder.StopRecording();
			}

			this.playbackTimer?.Dispose();

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
					this.recordingTimer.Elapsed -= this.RecordingTimer_Elapsed;
					this.recordingTimer.Dispose();
				}
				if (this.components != null)
				{
					this.components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		// Ersetze die Methode OnFormClosing, damit sie mit der Basisklasse übereinstimmt (Rückgabetyp void)
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);
			if (this.disposing || this.IsDisposed)
			{
				return;
			}

			// Remove event for time textbox
			this.recordingTimer.Elapsed -= this.RecordingTimer_Elapsed;

			// Stop recording if running
			if (AudioRecorder.IsRecording)
			{
				AudioRecorder.StopRecording();
				this.recordingTimer.Stop();
			}

			// Stop playback if running
			if (this.SelectedTrack != null && this.SelectedTrack.Playing)
			{
				string name = this.SelectedTrack.Name;
				this.SelectedTrack.Stop();
				this.playbackTimer?.Dispose();
				this.button_playback.Text = "▶";
				this.Log("Playback stopped ■", name);
			}

			this.Dispose();
		}

		private object? GetSelectedTrackItemSafe()
		{
			if (this.listBox_tracks.InvokeRequired)
			{
				return this.listBox_tracks.Invoke(new Func<object?>(() => this.listBox_tracks.SelectedItem));
			}
			else
			{
				return this.listBox_tracks.SelectedItem;
			}
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

		private void BuildLoopButtons()
		{
			this.panel_loop.Controls.Clear();
			int[] tags = [-16, -8, -4, -2, -1, 1, 2, 4, 8, 16];

			int buttonSize = this.panel_loop.Height - 2;
			if ((buttonSize + 2) * 10 > this.panel_loop.Width)
			{
				buttonSize = ((this.panel_loop.Width - 2) / 10) - 2;
			}
			float fontSize = 0.20f * (float) buttonSize;

			// Build 10 square (23x23) buttons for loop control (-16, -8, -4, -2, -1, +1, +2, +4, +8, +16)
			for (int i = 0; i < tags.Length; i++)
			{
				Button button = new()
				{
					Name = $"button_loop_{tags[i].ToString()}",
					Text = tags[i].ToString(),
					Font = new Font("Consolas", fontSize, FontStyle.Bold),
					Size = new Size(buttonSize, buttonSize),
					Tag = tags[i]
				};
				button.Click += this.ButtonLoop_Click;
				this.panel_loop.Controls.Add(button);
				button.Location = new Point(((this.panel_loop.Width / 10 - buttonSize) / 2) + (i) * (buttonSize + 2), (this.panel_loop.Height - buttonSize) / 2);
			}
		}

		private void ButtonLoop_Click(object? sender, EventArgs e)
		{
			Button? button = sender as Button;

			if (button == null)
			{
				this.Log("Loop button click error", "Button is null", true);
				return;
			}

			int beats = int.Parse(button.Tag?.ToString() ?? "1");

			if (this.SelectedTrack == null || beats == 0)
			{
				this.Log("No track selected for loop control", "Error");
				return;
			}

			// Handle single jump
			this.SelectedTrack.JumpByBeats(beats);
			this.Log($"Jumped track: {beats} beats", this.SelectedTrack?.Name ?? "N/A");
		}

		private void pictureBox_waveform_MouseWheel(object? sender, MouseEventArgs e)
		{
			if (e.Delta < 0)
			{
				// Zoom in
				if (this.numericUpDown_samplesPerPixel.Value < this.numericUpDown_samplesPerPixel.Maximum)
				{
					this.numericUpDown_samplesPerPixel.Value = Math.Min(this.numericUpDown_samplesPerPixel.Value + 1, this.numericUpDown_samplesPerPixel.Maximum);
				}
			}
			else if (e.Delta > 0)
			{
				// Zoom out
				if (this.numericUpDown_samplesPerPixel.Value > this.numericUpDown_samplesPerPixel.Minimum)
				{
					this.numericUpDown_samplesPerPixel.Value = Math.Max(this.numericUpDown_samplesPerPixel.Value - 1, this.numericUpDown_samplesPerPixel.Minimum);
				}
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
			// Temporär Event-Handler entfernen
			this.listBox_tracks.SuspendLayout();
			this.listBox_tracks.SelectedValueChanged -= this.ListBoxTracks_SelectedValueChanged;

			// Aktuellen Selection-Index merken
			var selectedIndex = this.listBox_tracks.SelectedIndex;

			// Data Binding korrekt einrichten
			this.listBox_tracks.DataSource = null; // Erst zurücksetzen
			this.listBox_tracks.DisplayMember = "Name"; // Property die angezeigt werden soll
			this.listBox_tracks.ValueMember = "Id"; // Optional: Wert-Property
			this.listBox_tracks.DataSource = this.audioCollection.Tracks;

			// Alte Selection wiederherstellen falls gewünscht
			if (keepSelection && selectedIndex > 0 || selectedIndex < this.audioCollection.Tracks.Count)
			{
				try
				{
					this.listBox_tracks.SelectedIndex = selectedIndex;
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error restoring selection: {ex.Message}");
				}

			}

			// Event-Handler wieder anhängen
			this.listBox_tracks.SelectedValueChanged += this.ListBoxTracks_SelectedValueChanged;
			this.listBox_tracks.ResumeLayout();
		}

		// Separater Event-Handler (besser als Lambda)
		private void ListBoxTracks_SelectedValueChanged(object? sender, EventArgs e)
		{
			this.UpdateInfoView();
			this.vScrollBar_trackVolume.Value = (100 - this.SelectedTrack?.Volume) ?? 100;
			if (this.SelectedTrack != null && this.SelectedTrack.Playing)
			{
				this.playbackTimer?.Dispose();
				this.playbackTimer = new System.Threading.Timer(async _ =>
				{
					if (this.disposing || this.IsDisposed || !this.IsHandleCreated)
					{
						return;
					}
					if (this.SelectedTrack?.Playing == true)
					{
						await this.UpdateWaveform();
					}
				}, null, 0, 1000 / (int) this.numericUpDown_fps.Value);
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
			this.button_playback.Text = this.SelectedTrack?.Playing == true ? "■" : "▶";
			this.button_pause.Text = this.SelectedTrack?.Paused == false ? "||" : "▶";
			this.button_pause.Enabled = this.SelectedTrack?.Playing == true || this.SelectedTrack?.Paused == true;
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
			else if (track.Playing)
			{
				this.playbackTimer?.Dispose();
				this.playbackTimer = new System.Threading.Timer(async _ =>
				{
					if (this.disposing || this.IsDisposed || !this.IsHandleCreated)
					{
						return;
					}
					if (track.Playing)
					{
						await this.UpdateWaveform();
					}
				}, null, 0, 100);
			}

			// Fill time stretch kernels
			this.FillStretchKernelsComboBox();

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


			this.label_info_beatDuration.Text = $"1 beat ≈  {this.estimatedBeatDuration:F3} sec.";

			this.numericUpDown_initialBpm.Value = (decimal) Math.Max((float) this.numericUpDown_initialBpm.Minimum, (float) (track.Bpm));

			if (track.IsProcessing)
			{
				this.numericUpDown_chunkSize.Enabled = false;
				this.numericUpDown_overlap.Enabled = false;
				this.button_export.Enabled = false;
				this.button_playback.Enabled = false;
				this.button_stretch.Enabled = false;
				this.comboBox_stretchKernels.Enabled = false;
			}
			else
			{
				this.button_export.Enabled = true;
				this.button_playback.Enabled = true;
				this.numericUpDown_chunkSize.Enabled = true;
				this.numericUpDown_overlap.Enabled = true;
				this.button_stretch.Enabled = true;
				this.button_reset.Enabled = true;
				this.comboBox_stretchKernels.Enabled = true;
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
			}
			else if (this.SelectedTrack.Playing)
			{
				// Update time textBox
				TimeSpan duration = this.SelectedTrack.CurrentTime;
				this.textBox_time.Text = $"{duration.Hours:D1}:{duration.Minutes:D2}:{duration.Seconds:D2}.{duration.Milliseconds:D3}";
			}
			else
			{
				// Reset time textBox
				this.textBox_time.ResetText();
			}

			// Get peak volume
			if (this.checkBox_detect.Checked)
			{
				float peakVolume = AudioRecorder.GetPeakVolume();
				this.label_peakVolume.ForeColor = peakVolume > 0.0f ? Color.Green : Color.DarkGray;
				this.label_peakVolume.Text = $"Peak Volume: {peakVolume:F6}";
				this.comboBox_captureDevices.Text = AudioRecorder.CaptureDeviceName + " (" + AudioRecorder.MMDeviceName + ")";

				float detectedBeat = AudioRecorder.EstimatedBpm;
				this.label_detectedBeat.ForeColor = Color.Green;
				this.label_detectedBeat.Text = $"Detected BPM: {detectedBeat:F3}";
				// this.numericUpDown_initialBpm.Value = (decimal) detectedBeat;
			}
			else
			{
				this.label_peakVolume.ForeColor = Color.DarkGray;
				this.label_peakVolume.Text = "Peak Volume: N/A";

				this.label_detectedBeat.ForeColor = Color.DarkGray;
				this.label_detectedBeat.Text = "Detected BPM: N/A";
			}

			// If hueShift checked, apply hue shift to graph color
			if (this.checkBox_hueGraph.Checked)
			{
				this.GetHueShiftedColor();
			}

			if (this.SelectedTrack == null)
			{
				// No track selected, clear waveform
				if (this.pictureBox_waveform.Image != null)
				{
					this.pictureBox_waveform.Image.Dispose();
					this.pictureBox_waveform.Image = null;
				}
				return;
			}

			// Update image + GC (previous disposed & not set image)
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
			ContextMenuStrip contextMenu = new();
			ToolStripMenuItem removeItem = new("Remove");

			removeItem.Click += async (sender, e) =>
			{
				if (this.SelectedTrack != null)
				{
					await this.audioCollection.RemoveAsync(this.SelectedTrack);
					this.FillTracksListBox();
					this.UpdateInfoView();
				}
			};

			contextMenu.Items.Add(removeItem);

			// Diese Lösung behält das ContextMenuStrip bei
			this.listBox_tracks.MouseDown += (sender, e) =>
			{
				if (e.Button == MouseButtons.Right)
				{
					int index = this.listBox_tracks.IndexFromPoint(e.Location);
					if (index != ListBox.NoMatches)
					{
						this.listBox_tracks.SelectedIndex = index;
						// Kurze Verzögerung für bessere UX
						Task.Delay(50).ContinueWith(_ =>
						{
							this.listBox_tracks.Invoke((MethodInvoker) delegate
							{
								Point center = new Point(Cursor.Position.X - 15, Cursor.Position.Y - 10);
								contextMenu.Show(center);
							});
						}, TaskScheduler.FromCurrentSynchronizationContext());
					}
				}
			};
		}

		private void UpdateVolumeLabels()
		{
			// Set initial values
			this.label_info_masterVolume.Text = this.masterVolume;
			this.label_info_trackVolume.Text = this.trackVolume;
		}

		private async Task StretchAll()
		{
			string kernelName = this.comboBox_stretchKernels.SelectedItem?.ToString() ?? string.Empty;

			// Create output directory if not exists
			string outputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "AsynCLAudio", "Output");
			if (!Directory.Exists(outputDir) || this.checkBox_autoExport.Checked == false)
			{
				Directory.CreateDirectory(outputDir);
				this.Log("Created output directory", outputDir);
			}

			// Progress handler for progress bar batch
			this.progressBar_batch.Value = 0;
			this.progressBar_batch.Maximum = this.audioCollection.Tracks.Count;

			ConcurrentBag<AudioObj> selectedTracks = [];

			foreach (var track in this.audioCollection.Tracks)
			{
				if (track.Bpm < 60)
				{
					this.Log("Stretch error", $"Track '{track.Name}' has a BPM below 60, skipping", true);
					this.progressBar_batch.Invoke(new Action(() => this.progressBar_batch.Increment(1)));
					track.IsProcessing = false;
					continue;
				}

				// Skip if bpm is same or matches (*2 or /2)
				if (track.Bpm == (float) this.numericUpDown_targetBpm.Value ||
					Math.Abs(track.Bpm - (float) this.numericUpDown_targetBpm.Value) < 0.01 ||
					Math.Abs(track.Bpm - (float) this.numericUpDown_targetBpm.Value * 2) < 0.01 ||
					Math.Abs(track.Bpm - (float) this.numericUpDown_targetBpm.Value / 2) < 0.01)
				{
					this.Log($"Skipping '{track.Name}' ({track.Bpm} BPM), already matches target BPM", "No processing needed");
					this.progressBar_batch.Invoke(new Action(() => this.progressBar_batch.Increment(1)));
					track.IsProcessing = false;
					continue;
				}

				// Add track to bag
				selectedTracks.Add(track);

				track.IsProcessing = true;
				this.FillTracksListBox();
				this.UpdateInfoView();

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
					this.playbackTimer?.Dispose();
					this.button_playback.Text = "▶";
					this.Log("Playback stopped ■", track.Name);
				}

				this.Log($"Started stretching '{track.Name}' ({track.Bpm} BPM) to {this.numericUpDown_targetBpm.Value} BPM with factor {factor:F2}", $"{chunkSize} samples, {overlap:F2} overlap");

				// Call time stretch
				var result = await this.openClService.TimeStretch(track, kernelName, "", factor, chunkSize, overlap, progressHandler);

				// Update info
				this.UpdateInfoView();
				this.Log($"Successfully stretched '{track.Name}'", $"{track.ElapsedProcessingTime:F1} ms elapsed");

				track.IsProcessing = false;

				// Optionally: Export all processed tracks
				if (this.checkBox_autoExport.Checked)
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

				// Reset progress bar
				if (this.progressBar_processing.InvokeRequired)
				{
					this.progressBar_processing.Invoke(new Action(() => this.progressBar_processing.Value = 0));
				}
				else
				{
					this.progressBar_processing.Value = 0;
				}

				this.progressBar_batch.Invoke(new Action(() => this.progressBar_batch.Increment(1)));
			}

			// Reset progress bar
			this.progressBar_batch.Invoke(new Action(() => this.progressBar_batch.Value = 0));

			// Log total processed tracks + time
			int count = selectedTracks.Count;
			long totalTime = (long) selectedTracks.Sum(t => t.ElapsedProcessingTime);
			double totalSeconds = totalTime / 1000.0;
			this.Log($"Total processed tracks: {count}, Total processing time: {totalSeconds:F1} seconds");

			this.FillTracksListBox();
			this.UpdateInfoView();

			// Play windows complement sound
			if (!AudioRecorder.IsRecording)
			{
				System.Media.SystemSounds.Asterisk.Play();
			}
		}





		// ----- Events ----- \\
		private async void button_import_Click(object sender, EventArgs e)
		{
			// OFD at MyMusic
			OpenFileDialog ofd = new()
			{
				Title = "Import Audio File(s)",
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
				RestoreDirectory = true,
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
				this.FillTracksListBox();

				// If first track, set as selected
				if (this.audioCollection.Tracks.Count == 1)
				{
					this.listBox_tracks.SelectedIndex = 0; // Select first added track
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

		// Event for when mouse is over the playback button: Set ForeColor to Red while mouse is over it, afterwards reset to default
		private void button_playback_MouseEnter(object? sender, EventArgs e)
		{
			// While mouse is over the button, watch for CTRL key down, then set ForeColor to Red, else if mouse leaves, reset ForeColor to default
			while (this.button_playback.ClientRectangle.Contains(this.button_playback.PointToClient(Cursor.Position)))
			{
				if ((ModifierKeys & Keys.Control) == Keys.Control)
				{
					this.button_playback.ForeColor = Color.Red;

					this.label_info_playbackInfo.Text = "Stop all (!)";
					this.label_info_playbackInfo.ForeColor = Color.Red;
					this.label_info_playbackInfo.Visible = true;
				}
				else
				{
					this.button_playback.ForeColor = SystemColors.ControlText;
					this.label_info_playbackInfo.ResetText();
					this.label_info_playbackInfo.Visible = false;
				}

				Application.DoEvents();
				System.Threading.Thread.Sleep(100);
			}

			// Reset ForeColor to default
			this.button_playback.ForeColor = SystemColors.ControlText;
			this.label_info_playbackInfo.ResetText();
			this.label_info_playbackInfo.Visible = false;
		}

		private async void button_playback_Click(object sender, EventArgs e)
		{
			// If CTRL down stop all
			if ((ModifierKeys & Keys.Control) == Keys.Control)
			{
				this.audioCollection.StopAll(this.checkBox_removeAfterPlayback.Checked);
				this.playbackTimer?.Dispose();
				this.playbackTimer = null;

				this.label_peakVolume.ForeColor = Color.DarkGray;
				this.label_peakVolume.Text = "Peak Volume: 0.0f";

				this.FillTracksListBox();
				this.UpdateInfoView();
				return;
			}

			// Check selected track
			if (this.SelectedTrack == null)
			{
				return;
			}

			string name = this.SelectedTrack.Name.Replace("▶ ", string.Empty).Replace("|| ", "").Trim();

			if (this.SelectedTrack.Playing)
			{
				this.playbackTimer?.Dispose();
				this.playbackTimer = null;
				this.SelectedTrack.Stop();
				this.Log("Playback stopped ■", name);

				if (this.checkBox_removeAfterPlayback.Checked)
				{
					this.Log("Removing track after playback", name);
					await this.audioCollection.RemoveAsync(this.SelectedTrack);
				}
			}
			else
			{
				this.playbackCancellationToken = new CancellationTokenSource();
				this.playbackTimer = new System.Threading.Timer(
					async _ => await this.UpdateWaveform(),
					null,
					0, 1000 / (int) this.numericUpDown_fps.Value);

				await this.SelectedTrack.Play(this.playbackCancellationToken.Token, null, this.Volume);
				this.button_playback.Text = "■";
				this.Log("Playback started ▶", name);
			}

			this.FillTracksListBox();
			this.UpdateInfoView();
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

			// Stop playback if running
			if (track.Playing)
			{
				track.Stop();
				this.playbackTimer?.Dispose();
				this.button_playback.Text = "▶";
				this.Log("Playback stopped ■", this.SelectedTrack.Name);
			}

			track.UpdateBpm((float) this.numericUpDown_initialBpm.Value);
			track.IsProcessing = true;

			this.FillTracksListBox();
			this.UpdateInfoView();

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

			this.Log("Started stretching (" + kernelName + ")", (int) (max / 6) + " chunks, " + track.SizeInMb.ToString("F1") + " MB");

			// Call time stretch
			var result = await this.openClService.TimeStretch(track, kernelName, "", factor, chunkSize, overlap, progressHandler);

			track.IsProcessing = false;

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

			// Play windows complement sound
			System.Media.SystemSounds.Asterisk.Play();

			this.Log("Successfully stretched '" + track.Name + "'", track.ElapsedProcessingTime.ToString("F1") + " ms elapsed");

			// Play windows complement sound
			if (!AudioRecorder.IsRecording)
			{
				System.Media.SystemSounds.Asterisk.Play();
			}

			this.FillTracksListBox();
			this.UpdateInfoView();
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
				this.playbackTimer?.Dispose();
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

		private void vScrollBar_volumeTrack_Scroll(object sender, ScrollEventArgs e)
		{
			int masterValue = this.vScrollBar_masterVolume.Value;
			float masterVolume = 1.0f - masterValue / 100f;

			int value = this.vScrollBar_trackVolume.Value;
			float volume = 1.0f - value / 100f;

			if (this.SelectedTrack != null)
			{
				this.SelectedTrack.SetVolume(volume * masterVolume);
			}

			this.FillTracksListBox();
			this.UpdateInfoView();
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
				this.playbackTimer?.Dispose();
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

		private async void button_level_Click(object sender, EventArgs e)
		{
			if (this.SelectedTrack == null)
			{
				this.Log("Level error", "Please select a track to adjust level", true);
				return;
			}

			float level = 0.8f;
			float amplitude = 1.0f;

			this.Log("Started adjusting level", $"Level: {level}, Amplitude: {amplitude}", true);

			await this.SelectedTrack.Level(level, amplitude);

			this.Log("Successfully adjusted level", this.SelectedTrack.Name);

			// Update info
			this.UpdateInfoView();
		}

		private async void button_record_Click(object sender, EventArgs e)
		{
			string outDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "AsynCLAudio", "Recorded");
			if (!Directory.Exists(outDir))
			{
				Directory.CreateDirectory(outDir);
				this.Log("Created output directory", outDir);
			}

			string fileName = Path.Combine(outDir, $"LiveRecording_{DateTime.Now:ddMMyyyy_HHmmss}.wav");

			if (!AudioRecorder.IsRecording)
			{
				this.comboBox_captureDevices.Enabled = false;
				this.button_record.ForeColor = Color.Red;
				this.textBox_recording.ForeColor = Color.Red;
				this.Log("Started recording...");
				this.recordingTime = DateTime.Now;

				// Timer starten, wenn die Aufnahme beginnt
				this.recordingTimer.Start();

				MMDevice? selectedDevice = this.comboBox_captureDevices.SelectedItem as MMDevice;
				this.comboBox_captureDevices.Enabled = false;

				await Task.Run(() =>
				{
					// Start recording
					AudioRecorder.StartRecording(fileName, selectedDevice);
				});

				this.comboBox_captureDevices.Text = AudioRecorder.CaptureDeviceName ?? AudioRecorder.MMDeviceName ?? "Default device";
			}
			else
			{
				// Require CTRL down to stop recording
				if ((ModifierKeys & Keys.Control) != Keys.Control)
				{
					this.Log("Recording stop info", "Please CTRL-click to stop recording!");
					return;
				}

				this.button_record.ForeColor = Color.Black;
				this.Log("Stopped recording.", (DateTime.Now - this.recordingTime).ToString("hh\\:mm\\:ss\\.fff"));

				// Timer stoppen, wenn die Aufnahme endet
				this.recordingTimer.Stop();

				this.textBox_recording.ForeColor = Color.Black;

				await Task.Run(() =>
				{
					// Stop recording
					AudioRecorder.StopRecording();
				});

				// this.comboBox_captureDevices.Enabled = true;
			}
		}

		private void RecordingTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
		{
			// Da das Timer-Event in einem anderen Thread läuft,
			// müssen wir Invoke verwenden, um auf UI-Controls zuzugreifen
			try
			{
				this.Invoke(() =>
				{
					TimeSpan elapsedTime = DateTime.Now - this.recordingTime;
					this.textBox_recording.Text = elapsedTime.ToString(@"mm\:ss\.fff");
				});
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

		private async void button_pause_Click(object sender, EventArgs e)
		{
			if (this.SelectedTrack != null)
			{
				if (this.SelectedTrack.Paused)
				{
					// Restart playback timer
					this.playbackTimer = new System.Threading.Timer(
						async _ => await this.UpdateWaveform(),
						null,
						0, 1000 / (int) this.numericUpDown_fps.Value);
					this.button_pause.Text = "||";
					this.Log("Playback resumed ▶", this.SelectedTrack.Name.Replace("▶ ", "").Replace("|| ", ""));
				}
				else
				{
					this.playbackTimer?.Dispose();
					this.button_pause.Text = "▶";
					this.Log("Playback paused ||", this.SelectedTrack.Name.Replace("▶ ", "").Replace("|| ", ""));
				}

				await this.SelectedTrack.Pause();

				// Update info view
				this.UpdateInfoView();
				this.FillTracksListBox();
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
				this.playbackTimer?.Dispose();
				this.playbackTimer = new System.Threading.Timer(
					async _ => await this.UpdateWaveform(),
					null,
					0, 1000 / fps); // Start immediately, then every 1000/fps ms
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
			if (vH < 0.0f)
			{
				vH += 1.0f;
			}

			if (vH > 1.0f)
			{
				vH -= 1.0f;
			}

			if ((6.0f * vH) < 1.0f)
			{
				return (v1 + (v2 - v1) * 6.0f * vH);
			}

			if ((2.0f * vH) < 1.0f)
			{
				return v2;
			}

			if ((3.0f * vH) < 2.0f)
			{
				return (v1 + (v2 - v1) * ((2.0f / 3.0f) - vH) * 6.0f);
			}

			return v1;
		}

		private async void button_backColor_Click(object sender, EventArgs e)
		{
			// If right clicked, switch blaCK 7 WHITE BACKGROUND
			if (e is MouseEventArgs mouseEvent && mouseEvent.Button == MouseButtons.Right)
			{
				if (this.audioCollection.BackColor == Color.Black)
				{
					this.audioCollection.BackColor = Color.White;
				}
				else
				{
					this.audioCollection.BackColor = Color.Black;
				}
			}
			else
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
				}
			}

			this.button_backColor.BackColor = this.audioCollection.BackColor;

			if (this.button_backColor.BackColor.GetBrightness() < 0.5f)
			{
				this.button_backColor.ForeColor = Color.White;
			}
			else
			{
				this.button_backColor.ForeColor = Color.Black;
			}

			this.Log("Background color changed", this.audioCollection.BackColor.ToString());

			await this.UpdateWaveform();
		}

		private async void button_strobe_Click(object sender, EventArgs e)
		{
			if (this.button_strobe.ForeColor == Color.Black)
			{
				// Enable strobe effect
				this.button_strobe.ForeColor = Color.Crimson;
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

			if (this.button_strobe.ForeColor == Color.Crimson)
			{
				this.Log("Strobe effect enabled", "Click again to disable");
			}
			else
			{
				this.Log("Strobe effect disabled", "Click again to enable");
			}

			await this.UpdateWaveform();
		}

		private void button_browse_Click(object sender, EventArgs e)
		{
			// New Windows Explorer Window at MyMusic\AsynCLAudio\
			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "AsynCLAudio");
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			try
			{
				System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
				{
					FileName = path,
					UseShellExecute = true
				});
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error opening folder: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void checkBox_removeAfterPlayback_CheckedChanged(object sender, EventArgs e)
		{
			if (this.checkBox_removeAfterPlayback.Checked)
			{
				this.checkBox_removeAfterPlayback.Text = "Tracks will be removed after playback.";
				this.Log("Remove after playback enabled", "Tracks will be removed after playback");
			}
			else
			{
				this.checkBox_removeAfterPlayback.ResetText();
				this.Log("Remove after playback disabled", "Tracks will not be removed after playback");
			}
		}

		private void vScrollBar_masterVolume_Scroll(object sender, ScrollEventArgs e)
		{
			float percentage = (100f - (float) this.vScrollBar_masterVolume.Value) / 100f;

			//this.audioCollection.SetMasterVolume(percentage);

			this.FillTracksListBox();
			this.UpdateInfoView();
		}

		private async void button_loop_Click(object sender, EventArgs e)
		{
			if (this.SelectedTrack == null)
			{
				this.Log("Loop error", "Please select a track to loop", true);
				return;
			}

			if (this.SelectedTrack.Looping == 0)
			{
				this.button_loop.BackColor = Color.LightGreen;
				await this.SelectedTrack.StartLoop(this.currentLoopBeats);
			}
			else
			{
				this.button_loop.BackColor = SystemColors.Control;
				this.SelectedTrack.StopLoop();
			}


		}

		private async void hScrollBar_looping_Scroll(object sender, ScrollEventArgs e)
		{
			this.label_info_looping.Text = $"Looping: {this.currentLoopBeats} beats";

			if (this.SelectedTrack != null && (this.SelectedTrack.Looping != 0 && this.SelectedTrack.Looping != this.currentLoopBeats))
			{
				await this.SelectedTrack.StartLoop(this.currentLoopBeats);
			}
		}

		private void checkBox_detect_CheckedChanged(object sender, EventArgs e)
		{
			
		}
	}
}
