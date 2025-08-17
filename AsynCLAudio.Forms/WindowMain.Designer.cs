
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
			this.components = new System.ComponentModel.Container();
			this.pictureBox_waveform = new PictureBox();
			this.listBox_tracks = new ListBox();
			this.audioCollectionBindingSource = new BindingSource(this.components);
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
			this.vScrollBar_trackVolume = new VScrollBar();
			this.button_normalize = new Button();
			this.button_record = new Button();
			this.textBox_time = new TextBox();
			this.checkBox_autoExport = new CheckBox();
			this.button_pause = new Button();
			this.button_graphColor = new Button();
			this.textBox_recording = new TextBox();
			this.label_zoom = new Label();
			this.label_info_chunkSize = new Label();
			this.label_info_overlap = new Label();
			this.checkBox_hueGraph = new CheckBox();
			this.numericUpDown_hueShift = new NumericUpDown();
			this.numericUpDown_fps = new NumericUpDown();
			this.label_info_fps = new Label();
			this.button_backColor = new Button();
			this.button_strobe = new Button();
			this.comboBox_captureDevices = new ComboBox();
			this.label_peakVolume = new Label();
			this.button_level = new Button();
			this.numericUpDown_levelDuration = new NumericUpDown();
			this.label_info_levelDuration = new Label();
			this.button_browse = new Button();
			this.checkBox_removeAfterPlayback = new CheckBox();
			this.label_info_playbackInfo = new Label();
			this.vScrollBar_masterVolume = new VScrollBar();
			this.label_info_trackVolume = new Label();
			this.label_info_masterVolume = new Label();
			this.windowMainBindingSource = new BindingSource(this.components);
			this.panel_loop = new Panel();
			this.label_info_beatDuration = new Label();
			this.progressBar_batch = new ProgressBar();
			this.hScrollBar_looping = new HScrollBar();
			this.label_info_looping = new Label();
			this.button_loop = new Button();
			this.label_detectedBeat = new Label();
			this.checkBox_detect = new CheckBox();
			this.button_scan = new Button();
			this.numericUpDown_bpmLookingRange = new NumericUpDown();
			this.numericUpDown_bpmWindowSize = new NumericUpDown();
			this.label_info_bpmWindowSize = new Label();
			this.label_info_bpmLookingRange = new Label();
			this.numericUpDown_bpm_max = new NumericUpDown();
			this.numericUpDown_bpm_min = new NumericUpDown();
			this.label_info_bpmMin = new Label();
			this.label_info_bpmMax = new Label();
			this.textBox_bpm_scanned = new TextBox();
			this.textBox_timing_scanned = new TextBox();
			this.button_scanTime = new Button();
			((System.ComponentModel.ISupportInitialize) this.pictureBox_waveform).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.audioCollectionBindingSource).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_chunkSize).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_overlap).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_samplesPerPixel).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_initialBpm).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_targetBpm).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_stretchFactor).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_hueShift).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_fps).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_levelDuration).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.windowMainBindingSource).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_bpmLookingRange).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_bpmWindowSize).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_bpm_max).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_bpm_min).BeginInit();
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
			this.listBox_tracks.DataBindings.Add(new Binding("DataContext", this.audioCollectionBindingSource, "Tracks", true));
			this.listBox_tracks.DataSource = this.audioCollectionBindingSource;
			this.listBox_tracks.DisplayMember = "Tracks";
			this.listBox_tracks.FormattingEnabled = true;
			this.listBox_tracks.ItemHeight = 15;
			this.listBox_tracks.Location = new Point(432, 359);
			this.listBox_tracks.Name = "listBox_tracks";
			this.listBox_tracks.Size = new Size(260, 154);
			this.listBox_tracks.TabIndex = 1;
			// 
			// audioCollectionBindingSource
			// 
			this.audioCollectionBindingSource.DataSource = typeof(Core.AudioCollection);
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
			this.numericUpDown_chunkSize.Location = new Point(92, 439);
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
			this.numericUpDown_overlap.Location = new Point(173, 439);
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
			this.button_stretch.Location = new Point(11, 483);
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
			this.numericUpDown_samplesPerPixel.Location = new Point(432, 330);
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
			this.numericUpDown_initialBpm.Location = new Point(92, 483);
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
			this.numericUpDown_targetBpm.Location = new Point(173, 483);
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
			this.numericUpDown_stretchFactor.Location = new Point(254, 483);
			this.numericUpDown_stretchFactor.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
			this.numericUpDown_stretchFactor.Minimum = new decimal(new int[] { 5, 0, 0, 131072 });
			this.numericUpDown_stretchFactor.Name = "numericUpDown_stretchFactor";
			this.numericUpDown_stretchFactor.Size = new Size(110, 23);
			this.numericUpDown_stretchFactor.TabIndex = 14;
			this.numericUpDown_stretchFactor.Value = new decimal(new int[] { 1, 0, 0, 0 });
			this.numericUpDown_stretchFactor.ValueChanged += this.numericUpDown_stretchFactor_ValueChanged;
			// 
			// label_info_initialBpm
			// 
			this.label_info_initialBpm.AutoSize = true;
			this.label_info_initialBpm.Location = new Point(92, 465);
			this.label_info_initialBpm.Name = "label_info_initialBpm";
			this.label_info_initialBpm.Size = new Size(55, 15);
			this.label_info_initialBpm.TabIndex = 15;
			this.label_info_initialBpm.Text = "Init. BPM";
			// 
			// label_info_targetBpm
			// 
			this.label_info_targetBpm.AutoSize = true;
			this.label_info_targetBpm.Location = new Point(173, 465);
			this.label_info_targetBpm.Name = "label_info_targetBpm";
			this.label_info_targetBpm.Size = new Size(68, 15);
			this.label_info_targetBpm.TabIndex = 16;
			this.label_info_targetBpm.Text = "Target BPM";
			// 
			// label_info_timeStretchFactor
			// 
			this.label_info_timeStretchFactor.AutoSize = true;
			this.label_info_timeStretchFactor.Location = new Point(254, 465);
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
			this.button_reset.Location = new Point(617, 330);
			this.button_reset.Name = "button_reset";
			this.button_reset.Size = new Size(75, 23);
			this.button_reset.TabIndex = 19;
			this.button_reset.Text = "Reset";
			this.button_reset.UseVisualStyleBackColor = true;
			this.button_reset.Click += this.button_reset_Click;
			// 
			// progressBar_processing
			// 
			this.progressBar_processing.Location = new Point(11, 512);
			this.progressBar_processing.Name = "progressBar_processing";
			this.progressBar_processing.Size = new Size(353, 15);
			this.progressBar_processing.TabIndex = 20;
			// 
			// listBox_log
			// 
			this.listBox_log.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point,  0);
			this.listBox_log.FormattingEnabled = true;
			this.listBox_log.ItemHeight = 13;
			this.listBox_log.Location = new Point(12, 167);
			this.listBox_log.Name = "listBox_log";
			this.listBox_log.Size = new Size(414, 186);
			this.listBox_log.TabIndex = 21;
			// 
			// vScrollBar_trackVolume
			// 
			this.vScrollBar_trackVolume.Cursor = Cursors.NoMoveVert;
			this.vScrollBar_trackVolume.Location = new Point(412, 385);
			this.vScrollBar_trackVolume.Name = "vScrollBar_trackVolume";
			this.vScrollBar_trackVolume.Size = new Size(17, 135);
			this.vScrollBar_trackVolume.TabIndex = 22;
			this.vScrollBar_trackVolume.Scroll += this.vScrollBar_volumeTrack_Scroll;
			// 
			// button_normalize
			// 
			this.button_normalize.Location = new Point(617, 198);
			this.button_normalize.Name = "button_normalize";
			this.button_normalize.Size = new Size(75, 23);
			this.button_normalize.TabIndex = 23;
			this.button_normalize.Text = "Normalize";
			this.button_normalize.UseVisualStyleBackColor = true;
			this.button_normalize.Click += this.button_normalize_Click;
			// 
			// button_record
			// 
			this.button_record.ForeColor = Color.Black;
			this.button_record.Location = new Point(588, 272);
			this.button_record.Name = "button_record";
			this.button_record.Size = new Size(23, 23);
			this.button_record.TabIndex = 24;
			this.button_record.Text = "●";
			this.button_record.UseVisualStyleBackColor = true;
			this.button_record.Click += this.button_record_Click;
			// 
			// textBox_time
			// 
			this.textBox_time.Location = new Point(513, 330);
			this.textBox_time.Name = "textBox_time";
			this.textBox_time.PlaceholderText = "00:00.000";
			this.textBox_time.ReadOnly = true;
			this.textBox_time.Size = new Size(69, 23);
			this.textBox_time.TabIndex = 25;
			// 
			// checkBox_autoExport
			// 
			this.checkBox_autoExport.AutoSize = true;
			this.checkBox_autoExport.Checked = true;
			this.checkBox_autoExport.CheckState = CheckState.Checked;
			this.checkBox_autoExport.Location = new Point(11, 443);
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
			// button_graphColor
			// 
			this.button_graphColor.Location = new Point(432, 12);
			this.button_graphColor.Name = "button_graphColor";
			this.button_graphColor.Size = new Size(74, 23);
			this.button_graphColor.TabIndex = 29;
			this.button_graphColor.Text = "Color";
			this.button_graphColor.UseVisualStyleBackColor = true;
			this.button_graphColor.Click += this.button_graphColor_Click;
			// 
			// textBox_recording
			// 
			this.textBox_recording.Location = new Point(513, 273);
			this.textBox_recording.Name = "textBox_recording";
			this.textBox_recording.PlaceholderText = "00:00.000";
			this.textBox_recording.ReadOnly = true;
			this.textBox_recording.Size = new Size(69, 23);
			this.textBox_recording.TabIndex = 30;
			// 
			// label_zoom
			// 
			this.label_zoom.AutoSize = true;
			this.label_zoom.Location = new Point(432, 312);
			this.label_zoom.Name = "label_zoom";
			this.label_zoom.Size = new Size(39, 15);
			this.label_zoom.TabIndex = 31;
			this.label_zoom.Text = "Zoom";
			// 
			// label_info_chunkSize
			// 
			this.label_info_chunkSize.AutoSize = true;
			this.label_info_chunkSize.Location = new Point(92, 422);
			this.label_info_chunkSize.Name = "label_info_chunkSize";
			this.label_info_chunkSize.Size = new Size(64, 15);
			this.label_info_chunkSize.TabIndex = 32;
			this.label_info_chunkSize.Text = "Chunk size";
			// 
			// label_info_overlap
			// 
			this.label_info_overlap.AutoSize = true;
			this.label_info_overlap.Location = new Point(173, 421);
			this.label_info_overlap.Name = "label_info_overlap";
			this.label_info_overlap.Size = new Size(48, 15);
			this.label_info_overlap.TabIndex = 33;
			this.label_info_overlap.Text = "Overlap";
			// 
			// checkBox_hueGraph
			// 
			this.checkBox_hueGraph.AutoSize = true;
			this.checkBox_hueGraph.Location = new Point(432, 100);
			this.checkBox_hueGraph.Name = "checkBox_hueGraph";
			this.checkBox_hueGraph.Size = new Size(74, 19);
			this.checkBox_hueGraph.TabIndex = 34;
			this.checkBox_hueGraph.Text = "Hue shift";
			this.checkBox_hueGraph.UseVisualStyleBackColor = true;
			this.checkBox_hueGraph.CheckedChanged += this.checkBox_hueGraph_CheckedChanged;
			// 
			// numericUpDown_hueShift
			// 
			this.numericUpDown_hueShift.DecimalPlaces = 2;
			this.numericUpDown_hueShift.Enabled = false;
			this.numericUpDown_hueShift.Increment = new decimal(new int[] { 25, 0, 0, 131072 });
			this.numericUpDown_hueShift.Location = new Point(432, 125);
			this.numericUpDown_hueShift.Maximum = new decimal(new int[] { 1799, 0, 0, 65536 });
			this.numericUpDown_hueShift.Minimum = new decimal(new int[] { 5, 0, 0, 131072 });
			this.numericUpDown_hueShift.Name = "numericUpDown_hueShift";
			this.numericUpDown_hueShift.Size = new Size(74, 23);
			this.numericUpDown_hueShift.TabIndex = 35;
			this.numericUpDown_hueShift.Value = new decimal(new int[] { 5, 0, 0, 0 });
			this.numericUpDown_hueShift.ValueChanged += this.numericUpDown_hueShift_ValueChanged;
			// 
			// numericUpDown_fps
			// 
			this.numericUpDown_fps.Location = new Point(432, 169);
			this.numericUpDown_fps.Maximum = new decimal(new int[] { 144, 0, 0, 0 });
			this.numericUpDown_fps.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			this.numericUpDown_fps.Name = "numericUpDown_fps";
			this.numericUpDown_fps.Size = new Size(45, 23);
			this.numericUpDown_fps.TabIndex = 36;
			this.numericUpDown_fps.Value = new decimal(new int[] { 45, 0, 0, 0 });
			this.numericUpDown_fps.ValueChanged += this.numericUpDown_fps_ValueChanged;
			// 
			// label_info_fps
			// 
			this.label_info_fps.AutoSize = true;
			this.label_info_fps.Location = new Point(433, 151);
			this.label_info_fps.Name = "label_info_fps";
			this.label_info_fps.Size = new Size(26, 15);
			this.label_info_fps.TabIndex = 37;
			this.label_info_fps.Text = "FPS";
			// 
			// button_backColor
			// 
			this.button_backColor.BackColor = Color.White;
			this.button_backColor.Location = new Point(432, 41);
			this.button_backColor.Name = "button_backColor";
			this.button_backColor.Size = new Size(74, 23);
			this.button_backColor.TabIndex = 38;
			this.button_backColor.Text = "Back";
			this.button_backColor.UseVisualStyleBackColor = false;
			this.button_backColor.MouseDown += this.button_backColor_Click;
			// 
			// button_strobe
			// 
			this.button_strobe.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point,  0);
			this.button_strobe.ForeColor = Color.Black;
			this.button_strobe.Location = new Point(483, 169);
			this.button_strobe.Name = "button_strobe";
			this.button_strobe.Size = new Size(23, 23);
			this.button_strobe.TabIndex = 39;
			this.button_strobe.Text = "🕱";
			this.button_strobe.UseVisualStyleBackColor = true;
			this.button_strobe.Click += this.button_strobe_Click;
			// 
			// comboBox_captureDevices
			// 
			this.comboBox_captureDevices.Enabled = false;
			this.comboBox_captureDevices.FormattingEnabled = true;
			this.comboBox_captureDevices.Location = new Point(12, 41);
			this.comboBox_captureDevices.Name = "comboBox_captureDevices";
			this.comboBox_captureDevices.Size = new Size(414, 23);
			this.comboBox_captureDevices.TabIndex = 40;
			this.comboBox_captureDevices.Text = "Select audio capture device ...";
			// 
			// label_peakVolume
			// 
			this.label_peakVolume.AutoSize = true;
			this.label_peakVolume.Location = new Point(12, 67);
			this.label_peakVolume.Name = "label_peakVolume";
			this.label_peakVolume.Size = new Size(100, 15);
			this.label_peakVolume.TabIndex = 41;
			this.label_peakVolume.Text = "Peak volume: 0.0f";
			// 
			// button_level
			// 
			this.button_level.Location = new Point(617, 227);
			this.button_level.Name = "button_level";
			this.button_level.Size = new Size(75, 23);
			this.button_level.TabIndex = 42;
			this.button_level.Text = "Level";
			this.button_level.UseVisualStyleBackColor = true;
			this.button_level.Click += this.button_level_Click;
			// 
			// numericUpDown_levelDuration
			// 
			this.numericUpDown_levelDuration.DecimalPlaces = 3;
			this.numericUpDown_levelDuration.Location = new Point(556, 229);
			this.numericUpDown_levelDuration.Maximum = new decimal(new int[] { 600, 0, 0, 0 });
			this.numericUpDown_levelDuration.Minimum = new decimal(new int[] { 5, 0, 0, 131072 });
			this.numericUpDown_levelDuration.Name = "numericUpDown_levelDuration";
			this.numericUpDown_levelDuration.Size = new Size(55, 23);
			this.numericUpDown_levelDuration.TabIndex = 43;
			this.numericUpDown_levelDuration.Value = new decimal(new int[] { 1, 0, 0, 0 });
			// 
			// label_info_levelDuration
			// 
			this.label_info_levelDuration.AutoSize = true;
			this.label_info_levelDuration.Location = new Point(556, 211);
			this.label_info_levelDuration.Name = "label_info_levelDuration";
			this.label_info_levelDuration.Size = new Size(53, 15);
			this.label_info_levelDuration.TabIndex = 44;
			this.label_info_levelDuration.Text = "Duration";
			// 
			// button_browse
			// 
			this.button_browse.Location = new Point(351, 73);
			this.button_browse.Name = "button_browse";
			this.button_browse.Size = new Size(75, 23);
			this.button_browse.TabIndex = 45;
			this.button_browse.Text = "Browse [...]";
			this.button_browse.UseVisualStyleBackColor = true;
			this.button_browse.Click += this.button_browse_Click;
			// 
			// checkBox_removeAfterPlayback
			// 
			this.checkBox_removeAfterPlayback.AutoSize = true;
			this.checkBox_removeAfterPlayback.Location = new Point(432, 519);
			this.checkBox_removeAfterPlayback.Name = "checkBox_removeAfterPlayback";
			this.checkBox_removeAfterPlayback.Size = new Size(151, 19);
			this.checkBox_removeAfterPlayback.TabIndex = 46;
			this.checkBox_removeAfterPlayback.Text = "Remove after playback?";
			this.checkBox_removeAfterPlayback.UseVisualStyleBackColor = true;
			this.checkBox_removeAfterPlayback.CheckedChanged += this.checkBox_removeAfterPlayback_CheckedChanged;
			// 
			// label_info_playbackInfo
			// 
			this.label_info_playbackInfo.AutoSize = true;
			this.label_info_playbackInfo.Location = new Point(513, 312);
			this.label_info_playbackInfo.Name = "label_info_playbackInfo";
			this.label_info_playbackInfo.Size = new Size(28, 15);
			this.label_info_playbackInfo.TabIndex = 47;
			this.label_info_playbackInfo.Text = "info";
			this.label_info_playbackInfo.Visible = false;
			// 
			// vScrollBar_masterVolume
			// 
			this.vScrollBar_masterVolume.Cursor = Cursors.NoMoveVert;
			this.vScrollBar_masterVolume.Enabled = false;
			this.vScrollBar_masterVolume.Location = new Point(388, 385);
			this.vScrollBar_masterVolume.Name = "vScrollBar_masterVolume";
			this.vScrollBar_masterVolume.Size = new Size(20, 135);
			this.vScrollBar_masterVolume.TabIndex = 48;
			this.vScrollBar_masterVolume.Visible = false;
			this.vScrollBar_masterVolume.Scroll += this.vScrollBar_masterVolume_Scroll;
			// 
			// label_info_trackVolume
			// 
			this.label_info_trackVolume.AutoSize = true;
			this.label_info_trackVolume.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point,  0);
			this.label_info_trackVolume.Location = new Point(388, 356);
			this.label_info_trackVolume.Name = "label_info_trackVolume";
			this.label_info_trackVolume.Size = new Size(34, 26);
			this.label_info_trackVolume.TabIndex = 49;
			this.label_info_trackVolume.Text = "Track\r\n100%";
			// 
			// label_info_masterVolume
			// 
			this.label_info_masterVolume.AutoSize = true;
			this.label_info_masterVolume.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point,  0);
			this.label_info_masterVolume.Location = new Point(387, 520);
			this.label_info_masterVolume.Name = "label_info_masterVolume";
			this.label_info_masterVolume.Size = new Size(42, 26);
			this.label_info_masterVolume.TabIndex = 50;
			this.label_info_masterVolume.Text = "Master\r\n100%";
			// 
			// windowMainBindingSource
			// 
			this.windowMainBindingSource.DataSource = typeof(WindowMain);
			// 
			// panel_loop
			// 
			this.panel_loop.BackColor = SystemColors.Control;
			this.panel_loop.Location = new Point(12, 690);
			this.panel_loop.Name = "panel_loop";
			this.panel_loop.Size = new Size(372, 45);
			this.panel_loop.TabIndex = 51;
			// 
			// label_info_beatDuration
			// 
			this.label_info_beatDuration.AutoSize = true;
			this.label_info_beatDuration.Location = new Point(12, 672);
			this.label_info_beatDuration.Name = "label_info_beatDuration";
			this.label_info_beatDuration.Size = new Size(146, 15);
			this.label_info_beatDuration.TabIndex = 52;
			this.label_info_beatDuration.Text = "No beat duration available";
			// 
			// progressBar_batch
			// 
			this.progressBar_batch.Location = new Point(11, 533);
			this.progressBar_batch.Name = "progressBar_batch";
			this.progressBar_batch.Size = new Size(353, 10);
			this.progressBar_batch.TabIndex = 53;
			// 
			// hScrollBar_looping
			// 
			this.hScrollBar_looping.LargeChange = 1;
			this.hScrollBar_looping.Location = new Point(388, 690);
			this.hScrollBar_looping.Maximum = 7;
			this.hScrollBar_looping.Name = "hScrollBar_looping";
			this.hScrollBar_looping.Size = new Size(304, 17);
			this.hScrollBar_looping.TabIndex = 54;
			this.hScrollBar_looping.Scroll += this.hScrollBar_looping_Scroll;
			// 
			// label_info_looping
			// 
			this.label_info_looping.AutoSize = true;
			this.label_info_looping.Location = new Point(388, 672);
			this.label_info_looping.Name = "label_info_looping";
			this.label_info_looping.Size = new Size(109, 15);
			this.label_info_looping.TabIndex = 55;
			this.label_info_looping.Text = "Looping: 0.25 beats";
			// 
			// button_loop
			// 
			this.button_loop.Location = new Point(387, 710);
			this.button_loop.Name = "button_loop";
			this.button_loop.Size = new Size(75, 25);
			this.button_loop.TabIndex = 56;
			this.button_loop.Text = "Loop";
			this.button_loop.UseVisualStyleBackColor = true;
			this.button_loop.Click += this.button_loop_Click;
			// 
			// label_detectedBeat
			// 
			this.label_detectedBeat.AutoSize = true;
			this.label_detectedBeat.Location = new Point(12, 84);
			this.label_detectedBeat.Name = "label_detectedBeat";
			this.label_detectedBeat.Size = new Size(129, 15);
			this.label_detectedBeat.TabIndex = 57;
			this.label_detectedBeat.Text = "Detected beat: 0.0 BPM";
			// 
			// checkBox_detect
			// 
			this.checkBox_detect.AutoSize = true;
			this.checkBox_detect.Location = new Point(147, 73);
			this.checkBox_detect.Name = "checkBox_detect";
			this.checkBox_detect.Size = new Size(83, 19);
			this.checkBox_detect.TabIndex = 58;
			this.checkBox_detect.Text = "Live detect";
			this.checkBox_detect.UseVisualStyleBackColor = true;
			this.checkBox_detect.CheckedChanged += this.checkBox_detect_CheckedChanged;
			// 
			// button_scan
			// 
			this.button_scan.Location = new Point(12, 138);
			this.button_scan.Name = "button_scan";
			this.button_scan.Size = new Size(50, 23);
			this.button_scan.TabIndex = 59;
			this.button_scan.Text = "Scan";
			this.button_scan.UseVisualStyleBackColor = true;
			this.button_scan.Click += this.button_scan_Click;
			// 
			// numericUpDown_bpmLookingRange
			// 
			this.numericUpDown_bpmLookingRange.Location = new Point(376, 138);
			this.numericUpDown_bpmLookingRange.Maximum = new decimal(new int[] { 64, 0, 0, 0 });
			this.numericUpDown_bpmLookingRange.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			this.numericUpDown_bpmLookingRange.Name = "numericUpDown_bpmLookingRange";
			this.numericUpDown_bpmLookingRange.Size = new Size(50, 23);
			this.numericUpDown_bpmLookingRange.TabIndex = 60;
			this.numericUpDown_bpmLookingRange.Value = new decimal(new int[] { 16, 0, 0, 0 });
			// 
			// numericUpDown_bpmWindowSize
			// 
			this.numericUpDown_bpmWindowSize.Location = new Point(300, 138);
			this.numericUpDown_bpmWindowSize.Maximum = new decimal(new int[] { 262144, 0, 0, 0 });
			this.numericUpDown_bpmWindowSize.Minimum = new decimal(new int[] { 128, 0, 0, 0 });
			this.numericUpDown_bpmWindowSize.Name = "numericUpDown_bpmWindowSize";
			this.numericUpDown_bpmWindowSize.Size = new Size(70, 23);
			this.numericUpDown_bpmWindowSize.TabIndex = 62;
			this.numericUpDown_bpmWindowSize.Value = new decimal(new int[] { 131072, 0, 0, 0 });
			// 
			// label_info_bpmWindowSize
			// 
			this.label_info_bpmWindowSize.AutoSize = true;
			this.label_info_bpmWindowSize.Location = new Point(300, 120);
			this.label_info_bpmWindowSize.Name = "label_info_bpmWindowSize";
			this.label_info_bpmWindowSize.Size = new Size(51, 15);
			this.label_info_bpmWindowSize.TabIndex = 63;
			this.label_info_bpmWindowSize.Text = "Window";
			// 
			// label_info_bpmLookingRange
			// 
			this.label_info_bpmLookingRange.AutoSize = true;
			this.label_info_bpmLookingRange.Location = new Point(376, 120);
			this.label_info_bpmLookingRange.Name = "label_info_bpmLookingRange";
			this.label_info_bpmLookingRange.Size = new Size(40, 15);
			this.label_info_bpmLookingRange.TabIndex = 64;
			this.label_info_bpmLookingRange.Text = "Range";
			// 
			// numericUpDown_bpm_max
			// 
			this.numericUpDown_bpm_max.Location = new Point(229, 138);
			this.numericUpDown_bpm_max.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
			this.numericUpDown_bpm_max.Minimum = new decimal(new int[] { 150, 0, 0, 0 });
			this.numericUpDown_bpm_max.Name = "numericUpDown_bpm_max";
			this.numericUpDown_bpm_max.Size = new Size(65, 23);
			this.numericUpDown_bpm_max.TabIndex = 65;
			this.numericUpDown_bpm_max.Value = new decimal(new int[] { 240, 0, 0, 0 });
			// 
			// numericUpDown_bpm_min
			// 
			this.numericUpDown_bpm_min.Location = new Point(158, 138);
			this.numericUpDown_bpm_min.Maximum = new decimal(new int[] { 150, 0, 0, 0 });
			this.numericUpDown_bpm_min.Minimum = new decimal(new int[] { 40, 0, 0, 0 });
			this.numericUpDown_bpm_min.Name = "numericUpDown_bpm_min";
			this.numericUpDown_bpm_min.Size = new Size(65, 23);
			this.numericUpDown_bpm_min.TabIndex = 66;
			this.numericUpDown_bpm_min.Value = new decimal(new int[] { 80, 0, 0, 0 });
			// 
			// label_info_bpmMin
			// 
			this.label_info_bpmMin.AutoSize = true;
			this.label_info_bpmMin.Location = new Point(159, 120);
			this.label_info_bpmMin.Name = "label_info_bpmMin";
			this.label_info_bpmMin.Size = new Size(59, 15);
			this.label_info_bpmMin.TabIndex = 67;
			this.label_info_bpmMin.Text = "Min. BPM";
			// 
			// label_info_bpmMax
			// 
			this.label_info_bpmMax.AutoSize = true;
			this.label_info_bpmMax.Location = new Point(229, 120);
			this.label_info_bpmMax.Name = "label_info_bpmMax";
			this.label_info_bpmMax.Size = new Size(60, 15);
			this.label_info_bpmMax.TabIndex = 68;
			this.label_info_bpmMax.Text = "Max. BPM";
			// 
			// textBox_bpm_scanned
			// 
			this.textBox_bpm_scanned.Location = new Point(68, 138);
			this.textBox_bpm_scanned.Name = "textBox_bpm_scanned";
			this.textBox_bpm_scanned.PlaceholderText = "0.000 BPM";
			this.textBox_bpm_scanned.ReadOnly = true;
			this.textBox_bpm_scanned.Size = new Size(84, 23);
			this.textBox_bpm_scanned.TabIndex = 69;
			// 
			// textBox_timing_scanned
			// 
			this.textBox_timing_scanned.Location = new Point(68, 109);
			this.textBox_timing_scanned.Name = "textBox_timing_scanned";
			this.textBox_timing_scanned.PlaceholderText = "4 / 4 time";
			this.textBox_timing_scanned.ReadOnly = true;
			this.textBox_timing_scanned.Size = new Size(84, 23);
			this.textBox_timing_scanned.TabIndex = 70;
			// 
			// button_scanTime
			// 
			this.button_scanTime.Location = new Point(12, 109);
			this.button_scanTime.Name = "button_scanTime";
			this.button_scanTime.Size = new Size(50, 23);
			this.button_scanTime.TabIndex = 71;
			this.button_scanTime.Text = "Scan";
			this.button_scanTime.UseVisualStyleBackColor = true;
			this.button_scanTime.Click += this.button_scanTime_Click;
			// 
			// WindowMain
			// 
			this.AutoScaleDimensions = new SizeF(7F, 15F);
			this.AutoScaleMode = AutoScaleMode.Font;
			this.BackColor = SystemColors.ControlLight;
			this.ClientSize = new Size(704, 801);
			this.Controls.Add(this.button_scanTime);
			this.Controls.Add(this.textBox_timing_scanned);
			this.Controls.Add(this.textBox_bpm_scanned);
			this.Controls.Add(this.label_info_bpmMax);
			this.Controls.Add(this.label_info_bpmMin);
			this.Controls.Add(this.numericUpDown_bpm_min);
			this.Controls.Add(this.numericUpDown_bpm_max);
			this.Controls.Add(this.label_info_bpmLookingRange);
			this.Controls.Add(this.label_info_bpmWindowSize);
			this.Controls.Add(this.numericUpDown_bpmWindowSize);
			this.Controls.Add(this.numericUpDown_bpmLookingRange);
			this.Controls.Add(this.button_scan);
			this.Controls.Add(this.checkBox_detect);
			this.Controls.Add(this.label_detectedBeat);
			this.Controls.Add(this.button_loop);
			this.Controls.Add(this.label_info_looping);
			this.Controls.Add(this.hScrollBar_looping);
			this.Controls.Add(this.progressBar_batch);
			this.Controls.Add(this.label_info_beatDuration);
			this.Controls.Add(this.panel_loop);
			this.Controls.Add(this.label_info_masterVolume);
			this.Controls.Add(this.label_info_trackVolume);
			this.Controls.Add(this.vScrollBar_masterVolume);
			this.Controls.Add(this.label_info_playbackInfo);
			this.Controls.Add(this.checkBox_removeAfterPlayback);
			this.Controls.Add(this.button_browse);
			this.Controls.Add(this.label_info_levelDuration);
			this.Controls.Add(this.numericUpDown_levelDuration);
			this.Controls.Add(this.button_level);
			this.Controls.Add(this.label_peakVolume);
			this.Controls.Add(this.comboBox_captureDevices);
			this.Controls.Add(this.button_strobe);
			this.Controls.Add(this.button_backColor);
			this.Controls.Add(this.label_info_fps);
			this.Controls.Add(this.numericUpDown_fps);
			this.Controls.Add(this.numericUpDown_hueShift);
			this.Controls.Add(this.checkBox_hueGraph);
			this.Controls.Add(this.label_info_overlap);
			this.Controls.Add(this.label_info_chunkSize);
			this.Controls.Add(this.label_zoom);
			this.Controls.Add(this.textBox_recording);
			this.Controls.Add(this.button_graphColor);
			this.Controls.Add(this.button_pause);
			this.Controls.Add(this.checkBox_autoExport);
			this.Controls.Add(this.textBox_time);
			this.Controls.Add(this.button_record);
			this.Controls.Add(this.button_normalize);
			this.Controls.Add(this.vScrollBar_trackVolume);
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
			this.MaximizeBox = false;
			this.MaximumSize = new Size(720, 840);
			this.MinimumSize = new Size(720, 840);
			this.Name = "WindowMain";
			this.Text = "AsynCLAudio (Forms)";
			((System.ComponentModel.ISupportInitialize) this.pictureBox_waveform).EndInit();
			((System.ComponentModel.ISupportInitialize) this.audioCollectionBindingSource).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_chunkSize).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_overlap).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_samplesPerPixel).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_initialBpm).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_targetBpm).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_stretchFactor).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_hueShift).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_fps).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_levelDuration).EndInit();
			((System.ComponentModel.ISupportInitialize) this.windowMainBindingSource).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_bpmLookingRange).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_bpmWindowSize).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_bpm_max).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_bpm_min).EndInit();
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
		private VScrollBar vScrollBar_trackVolume;
		private Button button_normalize;
		private Button button_record;
		private TextBox textBox_time;
		private CheckBox checkBox_autoExport;
		private Button button_pause;
		private Button button_graphColor;
		private TextBox textBox_recording;
		private Label label_zoom;
		private Label label_info_chunkSize;
		private Label label_info_overlap;
		private CheckBox checkBox_hueGraph;
		private NumericUpDown numericUpDown_hueShift;
		private NumericUpDown numericUpDown_fps;
		private Label label_info_fps;
		private Button button_backColor;
		private Button button_strobe;
		private ComboBox comboBox_captureDevices;
		private Label label_peakVolume;
		private Button button_level;
		private NumericUpDown numericUpDown_levelDuration;
		private Label label_info_levelDuration;
		private Button button_browse;
		private CheckBox checkBox_removeAfterPlayback;
		private BindingSource audioCollectionBindingSource;
		private Label label_info_playbackInfo;
		private VScrollBar vScrollBar_masterVolume;
		private Label label_info_trackVolume;
		private Label label_info_masterVolume;
		private BindingSource windowMainBindingSource;
		private Panel panel_loop;
		private Label label_info_beatDuration;
		private ProgressBar progressBar_batch;
		private HScrollBar hScrollBar_looping;
		private Label label_info_looping;
		private Button button_loop;
		private Label label_detectedBeat;
		private CheckBox checkBox_detect;
		private Button button_scan;
		private NumericUpDown numericUpDown_bpmLookingRange;
		private NumericUpDown numericUpDown_bpmWindowSize;
		private Label label_info_bpmWindowSize;
		private Label label_info_bpmLookingRange;
		private NumericUpDown numericUpDown_bpm_max;
		private NumericUpDown numericUpDown_bpm_min;
		private Label label_info_bpmMin;
		private Label label_info_bpmMax;
		private TextBox textBox_bpm_scanned;
		private TextBox textBox_timing_scanned;
		private Button button_scanTime;
	}
}
