
namespace AsynCLAudio.Forms
{
    partial class WindowMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.pictureBox_waveform = new PictureBox();
			this.listBox_tracks = new ListBox();
			this.button_import = new Button();
			this.button_export = new Button();
			this.comboBox_devices = new ComboBox();
			this.numericUpDown_chunkSize = new NumericUpDown();
			this.numericUpDown_overlap = new NumericUpDown();
			this.textBox_trackInfo = new TextBox();
			this.button_stretch = new Button();
			this.button_playback = new Button();
			this.numericUpDown_samplesPerPixel = new NumericUpDown();
			this.numericUpDown_initialBpm = new NumericUpDown();
			this.numericUpDown_targetBpm = new NumericUpDown();
			this.numericUpDown_stretchFactor = new NumericUpDown();
			this.label_info_initialBpm = new Label();
			this.label_info_targetBpm = new Label();
			this.label_info_timeStretchFactor = new Label();
			this.comboBox_stretchKernels = new ComboBox();
			this.button_reset = new Button();
			this.progressBar_processing = new ProgressBar();
			this.listBox_log = new ListBox();
			this.vScrollBar_volume = new VScrollBar();
			this.button_normalize = new Button();
			this.button_record = new Button();
			this.textBox_recordingTime = new TextBox();
			this.checkBox_autoExport = new CheckBox();
			this.button_pause = new Button();
			this.vScrollBar_sampleRate = new VScrollBar();
			((System.ComponentModel.ISupportInitialize) this.pictureBox_waveform).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_chunkSize).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_overlap).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_samplesPerPixel).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_initialBpm).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_targetBpm).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_stretchFactor).BeginInit();
			this.SuspendLayout();
			// 
			// pictureBox_waveform
			// 
			this.pictureBox_waveform.Location = new Point(12, 549);
			this.pictureBox_waveform.Name = "pictureBox_waveform";
			this.pictureBox_waveform.Size = new Size(680, 120);
			this.pictureBox_waveform.TabIndex = 0;
			this.pictureBox_waveform.TabStop = false;
			// 
			// listBox_tracks
			// 
			this.listBox_tracks.FormattingEnabled = true;
			this.listBox_tracks.ItemHeight = 15;
			this.listBox_tracks.Location = new Point(432, 359);
			this.listBox_tracks.Name = "listBox_tracks";
			this.listBox_tracks.Size = new Size(260, 184);
			this.listBox_tracks.TabIndex = 1;
			// 
			// button_import
			// 
			this.button_import.Location = new Point(617, 272);
			this.button_import.Name = "button_import";
			this.button_import.Size = new Size(75, 23);
			this.button_import.TabIndex = 3;
			this.button_import.Text = "Import";
			this.button_import.UseVisualStyleBackColor = true;
			this.button_import.Click += this.button_import_Click;
			// 
			// button_export
			// 
			this.button_export.Location = new Point(617, 301);
			this.button_export.Name = "button_export";
			this.button_export.Size = new Size(75, 23);
			this.button_export.TabIndex = 4;
			this.button_export.Text = "Export";
			this.button_export.UseVisualStyleBackColor = true;
			this.button_export.Click += this.button_export_Click;
			// 
			// comboBox_devices
			// 
			this.comboBox_devices.FormattingEnabled = true;
			this.comboBox_devices.Location = new Point(12, 12);
			this.comboBox_devices.Name = "comboBox_devices";
			this.comboBox_devices.Size = new Size(414, 23);
			this.comboBox_devices.TabIndex = 5;
			this.comboBox_devices.Text = "Select OpenCL-Device to initialize ...";
			// 
			// numericUpDown_chunkSize
			// 
			this.numericUpDown_chunkSize.Location = new Point(432, 301);
			this.numericUpDown_chunkSize.Maximum = new decimal(new int[] { 65536, 0, 0, 0 });
			this.numericUpDown_chunkSize.Minimum = new decimal(new int[] { 128, 0, 0, 0 });
			this.numericUpDown_chunkSize.Name = "numericUpDown_chunkSize";
			this.numericUpDown_chunkSize.Size = new Size(75, 23);
			this.numericUpDown_chunkSize.TabIndex = 6;
			this.numericUpDown_chunkSize.Value = new decimal(new int[] { 16384, 0, 0, 0 });
			// 
			// numericUpDown_overlap
			// 
			this.numericUpDown_overlap.DecimalPlaces = 2;
			this.numericUpDown_overlap.Increment = new decimal(new int[] { 5, 0, 0, 131072 });
			this.numericUpDown_overlap.Location = new Point(432, 330);
			this.numericUpDown_overlap.Maximum = new decimal(new int[] { 85, 0, 0, 131072 });
			this.numericUpDown_overlap.Name = "numericUpDown_overlap";
			this.numericUpDown_overlap.Size = new Size(75, 23);
			this.numericUpDown_overlap.TabIndex = 7;
			this.numericUpDown_overlap.Value = new decimal(new int[] { 5, 0, 0, 65536 });
			// 
			// textBox_trackInfo
			// 
			this.textBox_trackInfo.Location = new Point(512, 12);
			this.textBox_trackInfo.Multiline = true;
			this.textBox_trackInfo.Name = "textBox_trackInfo";
			this.textBox_trackInfo.ReadOnly = true;
			this.textBox_trackInfo.Size = new Size(180, 180);
			this.textBox_trackInfo.TabIndex = 8;
			// 
			// button_stretch
			// 
			this.button_stretch.Location = new Point(12, 491);
			this.button_stretch.Name = "button_stretch";
			this.button_stretch.Size = new Size(75, 23);
			this.button_stretch.TabIndex = 9;
			this.button_stretch.Text = "Stretch";
			this.button_stretch.UseVisualStyleBackColor = true;
			this.button_stretch.Click += this.button_stretch_Click;
			// 
			// button_playback
			// 
			this.button_playback.Location = new Point(588, 330);
			this.button_playback.Name = "button_playback";
			this.button_playback.Size = new Size(23, 23);
			this.button_playback.TabIndex = 10;
			this.button_playback.Text = "▶";
			this.button_playback.UseVisualStyleBackColor = true;
			this.button_playback.Click += this.button_playback_Click;
			// 
			// numericUpDown_samplesPerPixel
			// 
			this.numericUpDown_samplesPerPixel.Location = new Point(617, 330);
			this.numericUpDown_samplesPerPixel.Maximum = new decimal(new int[] { 4192, 0, 0, 0 });
			this.numericUpDown_samplesPerPixel.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			this.numericUpDown_samplesPerPixel.Name = "numericUpDown_samplesPerPixel";
			this.numericUpDown_samplesPerPixel.Size = new Size(75, 23);
			this.numericUpDown_samplesPerPixel.TabIndex = 11;
			this.numericUpDown_samplesPerPixel.Value = new decimal(new int[] { 128, 0, 0, 0 });
			// 
			// numericUpDown_initialBpm
			// 
			this.numericUpDown_initialBpm.DecimalPlaces = 4;
			this.numericUpDown_initialBpm.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
			this.numericUpDown_initialBpm.Location = new Point(93, 491);
			this.numericUpDown_initialBpm.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
			this.numericUpDown_initialBpm.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
			this.numericUpDown_initialBpm.Name = "numericUpDown_initialBpm";
			this.numericUpDown_initialBpm.Size = new Size(75, 23);
			this.numericUpDown_initialBpm.TabIndex = 12;
			this.numericUpDown_initialBpm.Value = new decimal(new int[] { 150, 0, 0, 0 });
			this.numericUpDown_initialBpm.ValueChanged += this.numericUpDown_initialBpm_ValueChanged;
			// 
			// numericUpDown_targetBpm
			// 
			this.numericUpDown_targetBpm.DecimalPlaces = 4;
			this.numericUpDown_targetBpm.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
			this.numericUpDown_targetBpm.Location = new Point(174, 491);
			this.numericUpDown_targetBpm.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
			this.numericUpDown_targetBpm.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
			this.numericUpDown_targetBpm.Name = "numericUpDown_targetBpm";
			this.numericUpDown_targetBpm.Size = new Size(75, 23);
			this.numericUpDown_targetBpm.TabIndex = 13;
			this.numericUpDown_targetBpm.Value = new decimal(new int[] { 150, 0, 0, 0 });
			this.numericUpDown_targetBpm.ValueChanged += this.numericUpDown_targetBpm_ValueChanged;
			// 
			// numericUpDown_stretchFactor
			// 
			this.numericUpDown_stretchFactor.DecimalPlaces = 12;
			this.numericUpDown_stretchFactor.Location = new Point(255, 491);
			this.numericUpDown_stretchFactor.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
			this.numericUpDown_stretchFactor.Minimum = new decimal(new int[] { 5, 0, 0, 131072 });
			this.numericUpDown_stretchFactor.Name = "numericUpDown_stretchFactor";
			this.numericUpDown_stretchFactor.Size = new Size(128, 23);
			this.numericUpDown_stretchFactor.TabIndex = 14;
			this.numericUpDown_stretchFactor.Value = new decimal(new int[] { 1, 0, 0, 0 });
			this.numericUpDown_stretchFactor.ValueChanged += this.numericUpDown_stretchFactor_ValueChanged;
			// 
			// label_info_initialBpm
			// 
			this.label_info_initialBpm.AutoSize = true;
			this.label_info_initialBpm.Location = new Point(93, 473);
			this.label_info_initialBpm.Name = "label_info_initialBpm";
			this.label_info_initialBpm.Size = new Size(55, 15);
			this.label_info_initialBpm.TabIndex = 15;
			this.label_info_initialBpm.Text = "Init. BPM";
			// 
			// label_info_targetBpm
			// 
			this.label_info_targetBpm.AutoSize = true;
			this.label_info_targetBpm.Location = new Point(174, 473);
			this.label_info_targetBpm.Name = "label_info_targetBpm";
			this.label_info_targetBpm.Size = new Size(68, 15);
			this.label_info_targetBpm.TabIndex = 16;
			this.label_info_targetBpm.Text = "Target BPM";
			// 
			// label_info_timeStretchFactor
			// 
			this.label_info_timeStretchFactor.AutoSize = true;
			this.label_info_timeStretchFactor.Location = new Point(255, 473);
			this.label_info_timeStretchFactor.Name = "label_info_timeStretchFactor";
			this.label_info_timeStretchFactor.Size = new Size(109, 15);
			this.label_info_timeStretchFactor.TabIndex = 17;
			this.label_info_timeStretchFactor.Text = "Time-stretch factor";
			// 
			// comboBox_stretchKernels
			// 
			this.comboBox_stretchKernels.FormattingEnabled = true;
			this.comboBox_stretchKernels.Location = new Point(12, 359);
			this.comboBox_stretchKernels.Name = "comboBox_stretchKernels";
			this.comboBox_stretchKernels.Size = new Size(371, 23);
			this.comboBox_stretchKernels.TabIndex = 18;
			this.comboBox_stretchKernels.Text = "Select OpenCL-Kernel for time stretching ...";
			// 
			// button_reset
			// 
			this.button_reset.Location = new Point(513, 330);
			this.button_reset.Name = "button_reset";
			this.button_reset.Size = new Size(69, 23);
			this.button_reset.TabIndex = 19;
			this.button_reset.Text = "Reset";
			this.button_reset.UseVisualStyleBackColor = true;
			this.button_reset.Click += this.button_reset_Click;
			// 
			// progressBar_processing
			// 
			this.progressBar_processing.Location = new Point(12, 520);
			this.progressBar_processing.Name = "progressBar_processing";
			this.progressBar_processing.Size = new Size(414, 23);
			this.progressBar_processing.TabIndex = 20;
			// 
			// listBox_log
			// 
			this.listBox_log.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point,  0);
			this.listBox_log.FormattingEnabled = true;
			this.listBox_log.ItemHeight = 13;
			this.listBox_log.Location = new Point(12, 154);
			this.listBox_log.Name = "listBox_log";
			this.listBox_log.Size = new Size(414, 199);
			this.listBox_log.TabIndex = 21;
			// 
			// vScrollBar_volume
			// 
			this.vScrollBar_volume.Location = new Point(406, 359);
			this.vScrollBar_volume.Name = "vScrollBar_volume";
			this.vScrollBar_volume.Size = new Size(20, 158);
			this.vScrollBar_volume.TabIndex = 22;
			this.vScrollBar_volume.Scroll += this.vScrollBar_volume_Scroll;
			// 
			// button_normalize
			// 
			this.button_normalize.Location = new Point(12, 422);
			this.button_normalize.Name = "button_normalize";
			this.button_normalize.Size = new Size(75, 23);
			this.button_normalize.TabIndex = 23;
			this.button_normalize.Text = "Normalize";
			this.button_normalize.UseVisualStyleBackColor = true;
			this.button_normalize.Click += this.button_normalize_Click;
			// 
			// button_record
			// 
			this.button_record.ForeColor = Color.Red;
			this.button_record.Location = new Point(588, 272);
			this.button_record.Name = "button_record";
			this.button_record.Size = new Size(23, 23);
			this.button_record.TabIndex = 24;
			this.button_record.Text = "●";
			this.button_record.UseVisualStyleBackColor = true;
			this.button_record.Click += this.button_record_Click;
			// 
			// textBox_recordingTime
			// 
			this.textBox_recordingTime.Location = new Point(513, 301);
			this.textBox_recordingTime.Name = "textBox_recordingTime";
			this.textBox_recordingTime.PlaceholderText = "00:00.000";
			this.textBox_recordingTime.ReadOnly = true;
			this.textBox_recordingTime.Size = new Size(69, 23);
			this.textBox_recordingTime.TabIndex = 25;
			// 
			// checkBox_autoExport
			// 
			this.checkBox_autoExport.AutoSize = true;
			this.checkBox_autoExport.Checked = true;
			this.checkBox_autoExport.CheckState = CheckState.Checked;
			this.checkBox_autoExport.Location = new Point(12, 451);
			this.checkBox_autoExport.Name = "checkBox_autoExport";
			this.checkBox_autoExport.Size = new Size(59, 34);
			this.checkBox_autoExport.TabIndex = 26;
			this.checkBox_autoExport.Text = "Auto\r\nExport";
			this.checkBox_autoExport.UseVisualStyleBackColor = true;
			// 
			// button_pause
			// 
			this.button_pause.Location = new Point(588, 301);
			this.button_pause.Name = "button_pause";
			this.button_pause.Size = new Size(23, 23);
			this.button_pause.TabIndex = 27;
			this.button_pause.Text = "||";
			this.button_pause.UseVisualStyleBackColor = true;
			this.button_pause.Click += this.button_pause_Click;
			// 
			// vScrollBar_sampleRate
			// 
			this.vScrollBar_sampleRate.Location = new Point(386, 359);
			this.vScrollBar_sampleRate.Maximum = 65536;
			this.vScrollBar_sampleRate.Name = "vScrollBar_sampleRate";
			this.vScrollBar_sampleRate.Size = new Size(20, 158);
			this.vScrollBar_sampleRate.TabIndex = 28;
			this.vScrollBar_sampleRate.Value = 32768;
			this.vScrollBar_sampleRate.Scroll += this.vScrollBar_sampleRate_Scroll;
			// 
			// WindowMain
			// 
			this.AutoScaleDimensions = new SizeF(7F, 15F);
			this.AutoScaleMode = AutoScaleMode.Font;
			this.ClientSize = new Size(704, 681);
			this.Controls.Add(this.vScrollBar_sampleRate);
			this.Controls.Add(this.button_pause);
			this.Controls.Add(this.checkBox_autoExport);
			this.Controls.Add(this.textBox_recordingTime);
			this.Controls.Add(this.button_record);
			this.Controls.Add(this.button_normalize);
			this.Controls.Add(this.vScrollBar_volume);
			this.Controls.Add(this.listBox_log);
			this.Controls.Add(this.progressBar_processing);
			this.Controls.Add(this.button_reset);
			this.Controls.Add(this.comboBox_stretchKernels);
			this.Controls.Add(this.label_info_timeStretchFactor);
			this.Controls.Add(this.label_info_targetBpm);
			this.Controls.Add(this.label_info_initialBpm);
			this.Controls.Add(this.numericUpDown_stretchFactor);
			this.Controls.Add(this.numericUpDown_targetBpm);
			this.Controls.Add(this.numericUpDown_initialBpm);
			this.Controls.Add(this.numericUpDown_samplesPerPixel);
			this.Controls.Add(this.button_playback);
			this.Controls.Add(this.button_stretch);
			this.Controls.Add(this.textBox_trackInfo);
			this.Controls.Add(this.numericUpDown_overlap);
			this.Controls.Add(this.numericUpDown_chunkSize);
			this.Controls.Add(this.comboBox_devices);
			this.Controls.Add(this.button_export);
			this.Controls.Add(this.button_import);
			this.Controls.Add(this.listBox_tracks);
			this.Controls.Add(this.pictureBox_waveform);
			this.MaximumSize = new Size(720, 720);
			this.MinimumSize = new Size(720, 720);
			this.Name = "WindowMain";
			this.Text = "AsynCLAudio (Forms)";
			((System.ComponentModel.ISupportInitialize) this.pictureBox_waveform).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_chunkSize).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_overlap).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_samplesPerPixel).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_initialBpm).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_targetBpm).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_stretchFactor).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private void vScrollBar_sampleRate_Scroll(Object sender, ScrollEventArgs e) => throw new NotImplementedException();

		#endregion

		private PictureBox pictureBox_waveform;
		private ListBox listBox_tracks;
		private Button button_import;
		private Button button_export;
		private ComboBox comboBox_devices;
		private NumericUpDown numericUpDown_chunkSize;
		private NumericUpDown numericUpDown_overlap;
		private TextBox textBox_trackInfo;
		private Button button_stretch;
		private Button button_playback;
		private NumericUpDown numericUpDown_samplesPerPixel;
		private NumericUpDown numericUpDown_initialBpm;
		private NumericUpDown numericUpDown_targetBpm;
		private NumericUpDown numericUpDown_stretchFactor;
		private Label label_info_initialBpm;
		private Label label_info_targetBpm;
		private Label label_info_timeStretchFactor;
		private ComboBox comboBox_stretchKernels;
		private Button button_reset;
		private ProgressBar progressBar_processing;
		private ListBox listBox_log;
		private VScrollBar vScrollBar_volume;
		private Button button_normalize;
		private Button button_record;
		private TextBox textBox_recordingTime;
		private CheckBox checkBox_autoExport;
		private Button button_pause;
		private VScrollBar vScrollBar_sampleRate;
	}
}
