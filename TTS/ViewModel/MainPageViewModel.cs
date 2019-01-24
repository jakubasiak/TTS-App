using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TTS.Annotations;
using TTS.Command;

namespace TTS.ViewModel
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public MainPageViewModel()
        {
            this.Rate = 0;
            this.Volume = 50;
            this.Text = "Twoja stara klaszcze u rubika. To jest zdanie testowe 1. To jest zdanie testowe 2. To jest jeszcze dłuższe zdanie testowe, które pozwoli mi na przetestowanie wielu funkcjonalności.";
            this.Voices = this.SpeechSynthesizer.GetInstalledVoices().Select(v => v.VoiceInfo.Name).ToList();
            this.selectedVoice = this.Voices.FirstOrDefault();
            this.IsRunning = false;
            this.Position = 0; 
            this.SpeechSynthesizer.SpeakCompleted += this.SpeechSynthesizer_SpeakCompleted;
            this.SpeechSynthesizer.SpeakProgress += this.SpeechSynthesizer_SpeakProgress;
            this.SpeechSynthesizer.SpeakStarted += this.SpeechSynthesizer_SpeakStarted;
        }

        private SpeechSynthesizer speechSynthesizer;
        public SpeechSynthesizer SpeechSynthesizer
        {
            get
            {
                if (this.speechSynthesizer == null)
                {
                    this.speechSynthesizer = new SpeechSynthesizer();
                }
                return this.speechSynthesizer;
            }
        }

        bool isRunning;
        public bool IsRunning
        {
            get => this.isRunning;
            set
            {
                this.isRunning = value;
                this.OnPropertyChanged(nameof(this.IsRunning));
            }
        }

        int volume;
        public int Volume
        {
            get => this.volume;
            set
            {
                this.volume = value;
                this.OnPropertyChanged(nameof(this.Volume));
            }
        }

        int rate;
        public int Rate
        {
            get => this.rate;
            set
            {
                this.rate = value;
                this.OnPropertyChanged(nameof(this.Rate));
            }
        }

        ICollection<string> voices;
        public ICollection<string> Voices
        {
            get => this.voices;
            set
            {
                this.voices = value;
                this.OnPropertyChanged(nameof(this.Voices));
            }
        }

        string selectedVoice;
        public string SelectedVoice
        {
            get => this.selectedVoice;
            set
            {
                this.selectedVoice = value;
                this.OnPropertyChanged(nameof(this.SelectedVoice));
            }
        }

        string currentText;
        public string CurrentText
        {
            get => this.currentText;
            set
            {
                this.currentText = value;
                this.OnPropertyChanged(nameof(this.CurrentText));
            }
        }

        int position;
        public int Position
        {
            get => this.position;
            set
            {
                this.position = value;
                this.OnPropertyChanged(nameof(this.Position));
            }
        }

        private string orginalText;

        string text;
        public string Text
        {
            get => this.text;
            set
            {
                this.text = value;
                this.OnPropertyChanged(nameof(this.Text));
            }
        }

        private ICommand playCommand;
        public ICommand PlayCommand
        {
            get
            {
                if (this.playCommand == null)
                    this.playCommand = new RelayCommand(
                        x =>
                        {
                            this.SpeechSynthesizer.Rate = this.Rate;
                            this.SpeechSynthesizer.Volume = this.Volume;
                            this.IsRunning = true;
                            this.SpeechSynthesizer.SpeakAsync(this.Text);
                        }
                    );
                return this.playCommand;
            }
        }

        private ICommand pauseCommand;
        public ICommand PauseCommand
        {
            get
            {
                if (this.pauseCommand == null)
                    this.pauseCommand = new RelayCommand(
                        x =>
                        {
                            if (this.IsRunning)
                            {
                                this.IsRunning = false;
                                this.SpeechSynthesizer.Pause();
                            }
                            else
                            {
                                this.IsRunning = true;
                                this.SpeechSynthesizer.Resume();
                            }

                        }
                    );
                return this.pauseCommand;
            }
        }

        private ICommand stopCommand;
        public ICommand StopCommand
        {
            get
            {
                if (this.stopCommand == null)
                    this.stopCommand = new RelayCommand(
                        x =>
                        {
                            this.IsRunning = false;
                            this.SpeechSynthesizer.SpeakAsyncCancelAll();
                            this.CurrentText = null;
                            this.Position = 0;
                        }
                    );
                return this.stopCommand;
            }
        }

        private ICommand windowCloseCommand;
        public ICommand WindowCloseCommand
        {
            get
            {
                if (this.windowCloseCommand == null)
                    this.windowCloseCommand = new RelayCommand(
                        x =>
                        {
                            ((Window)x).Close();
                        }
                    );

                return this.windowCloseCommand;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (propertyName == nameof(this.SelectedVoice))
            {
                this.SpeechSynthesizer.SelectVoice(this.SelectedVoice);
            }
            if (propertyName == nameof(this.Volume) || propertyName == nameof(this.Rate) || propertyName == nameof(this.SelectedVoice))
            {
                if (this.IsRunning)
                {
                    this.IsRunning = false;
                    this.SpeechSynthesizer.SpeakAsyncCancelAll();
                }
            }
        }

        private void SpeechSynthesizer_SpeakStarted(object sender, SpeakStartedEventArgs e)
        {
            this.orginalText = this.Text;
        }

        private void SpeechSynthesizer_SpeakProgress(object sender, SpeakProgressEventArgs e)
        {
            this.CurrentText = e.Text;
            this.Position = e.CharacterPosition;
            this.Text = this.text.Substring(0, e.CharacterPosition).ToUpper() + this.text.Substring(e.CharacterPosition, this.text.Length - e.CharacterPosition);
        }

        private void SpeechSynthesizer_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            this.IsRunning = false;
            this.CurrentText = null;
            this.Position = 0;
            this.Text = this.orginalText;
        }
    }
}
