using System;
using System.IO;
using System.Windows.Forms;
using NAudio.Wave;

namespace VoiceRecorder
{
    public partial class Form1 : Form
    {
        private WaveInEvent waveIn;
        private MemoryStream memoryStream;
        private WaveOutEvent waveOut;
        private Button recordButton;
        private Button playbackButton;
        private Button saveButton;
        private ComboBox sampleRateDropdown;
        private ComboBox bitFormatDropdown;
        private ComboBox deviceDropdown;
        private bool isRecording = false;
        private int selectedSampleRate = 11025;
        private bool unsigned8Bit = true;
        private int selectedDeviceIndex = 0;

        public Form1()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            recordButton = new Button { Text = "Record", Size = new System.Drawing.Size(100, 30), Location = new System.Drawing.Point(10, 10) };
            recordButton.Click += RecordButton_Click;
            Controls.Add(recordButton);

            playbackButton = new Button { Text = "Playback", Size = new System.Drawing.Size(100, 30), Location = new System.Drawing.Point(120, 10) };
            playbackButton.Click += PlaybackButton_Click;
            Controls.Add(playbackButton);

            saveButton = new Button { Text = "Save", Size = new System.Drawing.Size(100, 30), Location = new System.Drawing.Point(230, 10) };
            saveButton.Click += SaveButton_Click;
            Controls.Add(saveButton);

            sampleRateDropdown = new ComboBox { Location = new System.Drawing.Point(340, 10), Size = new System.Drawing.Size(80, 30) };
            sampleRateDropdown.Items.AddRange(new object[] { "8000", "11025", "22050", "44100" });
            sampleRateDropdown.SelectedIndex = 1;
            sampleRateDropdown.SelectedIndexChanged += SampleRateDropdown_SelectedIndexChanged;
            Controls.Add(sampleRateDropdown);

            bitFormatDropdown = new ComboBox { Location = new System.Drawing.Point(430, 10), Size = new System.Drawing.Size(100, 30) };
            bitFormatDropdown.Items.AddRange(new object[] { "Unsigned 8-bit", "Signed 8-bit" });
            bitFormatDropdown.SelectedIndex = 0;
            bitFormatDropdown.SelectedIndexChanged += BitFormatDropdown_SelectedIndexChanged;
            Controls.Add(bitFormatDropdown);

            deviceDropdown = new ComboBox { Location = new System.Drawing.Point(540, 10), Size = new System.Drawing.Size(150, 30) };
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                var caps = WaveIn.GetCapabilities(i);
                deviceDropdown.Items.Add(caps.ProductName);
            }
            deviceDropdown.SelectedIndex = 0;
            deviceDropdown.SelectedIndexChanged += DeviceDropdown_SelectedIndexChanged;
            Controls.Add(deviceDropdown);

            memoryStream = new MemoryStream();
        }

        private void SampleRateDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedSampleRate = int.Parse(sampleRateDropdown.SelectedItem.ToString());
        }

        private void BitFormatDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            unsigned8Bit = bitFormatDropdown.SelectedIndex == 0;
        }

        private void DeviceDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedDeviceIndex = deviceDropdown.SelectedIndex;
        }

        private void RecordButton_Click(object sender, EventArgs e)
        {
            if (!isRecording)
            {
                memoryStream = new MemoryStream();
                waveIn = new WaveInEvent();
                waveIn.DeviceNumber = selectedDeviceIndex;
                waveIn.WaveFormat = new WaveFormat(selectedSampleRate, 16, 1);
                waveIn.DataAvailable += WaveIn_DataAvailable;
                waveIn.RecordingStopped += WaveIn_RecordingStopped;
                waveIn.StartRecording();
                recordButton.Text = "Stop";
                isRecording = true;
            }
            else
            {
                waveIn.StopRecording();
            }
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            for (int i = 0; i < e.BytesRecorded; i += 2)
            {
                short sample = (short)((e.Buffer[i + 1] << 8) | e.Buffer[i]);
                byte output = unsigned8Bit ? (byte)(sample / 256 + 128) : (byte)(sample / 256);
                memoryStream.WriteByte(output);
            }
        }

        private void WaveIn_RecordingStopped(object sender, StoppedEventArgs e)
        {
            waveIn.Dispose();
            waveIn = null;
            isRecording = false;
            recordButton.Text = "Record";
        }

        private void PlaybackButton_Click(object sender, EventArgs e)
        {
            if (memoryStream.Length == 0) return;
            memoryStream.Position = 0;
            waveOut = new WaveOutEvent();
            var tempStream = new MemoryStream();
            var writer = new BinaryWriter(tempStream);
            memoryStream.Position = 0;
            while (memoryStream.Position < memoryStream.Length)
            {
                byte b = (byte)memoryStream.ReadByte();
                short sample = unsigned8Bit ? (short)((b - 128) << 8) : (short)(b << 8);
                writer.Write(sample);
            }
            tempStream.Position = 0;
            var reader = new RawSourceWaveStream(tempStream, new WaveFormat(selectedSampleRate, 16, 1));
            waveOut.Init(reader);
            waveOut.Play();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (memoryStream.Length == 0) return;
            SaveFileDialog dialog = new SaveFileDialog { Filter = "WAV files|*.wav", FileName = "recorded_audio.wav" };
            if (dialog.ShowDialog() != DialogResult.OK) return;

            memoryStream.Position = 0;
            using var fs = new FileStream(dialog.FileName, FileMode.Create, FileAccess.Write);
            WaveFileWriter writer = new WaveFileWriter(fs, new WaveFormat(selectedSampleRate, 8, 1));
            writer.Write(memoryStream.ToArray(), 0, (int)memoryStream.Length);
            writer.Dispose();
            MessageBox.Show("Saved to " + dialog.FileName);
        }
    }
}
