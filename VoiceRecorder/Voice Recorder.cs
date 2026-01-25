namespace VoiceRecorder
{
    public partial class VoiceRecorder : Form
    {
        public VoiceRecorder()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // VoiceRecorder
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(902, 261);
            Name = "VoiceRecorder";
            ShowIcon = false;
            Text = "Voice Recorder";
            ResumeLayout(false);

        }
    }
}
