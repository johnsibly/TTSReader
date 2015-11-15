using System;
using System.Windows.Forms;
using System.Speech.Synthesis;
using System.Diagnostics;
using System.Speech.Recognition;

namespace TTSReader
{
    public partial class TextToSpeechReaderForm : Form
    {
        private enum State
        {
            Stopped,
            Playing,
            Paused
        }

        private SpeechSynthesizer _synthesizer = new SpeechSynthesizer();
        private State _state = State.Stopped;

        private void UpdateState(State newState)
        {
            _state = newState;
            buttonPlay.Enabled = _state != State.Playing;
            buttonPause.Enabled = _state == State.Playing;
            buttonStop.Enabled = _state != State.Stopped;
        }

        public TextToSpeechReaderForm()
        {
            InitializeComponent();

            _synthesizer.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(_synthesizer_SpeakCompleted);
            _synthesizer.PhonemeReached += new EventHandler<PhonemeReachedEventArgs>(_synthesizer_PhonemeReached);
            _synthesizer.SpeakProgress += new EventHandler<SpeakProgressEventArgs>(_synthesizer_SpeakProgress);
            _synthesizer.StateChanged += new EventHandler<System.Speech.Synthesis.StateChangedEventArgs>(_synthesizer_StateChanged);
        }

        private void buttonPlay_Click(object sender, EventArgs e)
        {
            if (_state == State.Stopped)
            {
                _synthesizer.SpeakAsync(textBox1.Text);
            }
            else
            {
                _synthesizer.Resume();
            }
            UpdateState(State.Playing);
        }

        void _synthesizer_StateChanged(object sender, System.Speech.Synthesis.StateChangedEventArgs e)
        {
            Debug.WriteLine("StateChanged " + e.State.ToString());
        }

        void _synthesizer_SpeakProgress(object sender, SpeakProgressEventArgs e)
        {
            textBox1.Select(e.CharacterPosition, e.CharacterCount);
        }

        void _synthesizer_PhonemeReached(object sender, PhonemeReachedEventArgs e)
        {
            labelPosition.Text = string.Format("{0}:{1}:{2}.{3}", 
                e.AudioPosition.Hours.ToString("00"), 
                e.AudioPosition.Minutes.ToString("00"), 
                e.AudioPosition.Seconds.ToString("00"), 
                e.AudioPosition.Milliseconds.ToString("000"));
        }

        void _synthesizer_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            textBox1.Select(0, 0);
            UpdateState(State.Stopped);
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            switch (_state)
            {
            case State.Playing:
                _synthesizer.SpeakAsyncCancelAll();
                break;
            default:
                break;
            }
            UpdateState(State.Stopped);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            _synthesizer.Rate = (int)numericUpDownRate.Value;
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            _synthesizer.Pause();
            UpdateState(State.Paused);
        }

        void _recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            textBox1.Text.Insert(textBox1.SelectionStart, e.Result.Text);
        }
    }
}
