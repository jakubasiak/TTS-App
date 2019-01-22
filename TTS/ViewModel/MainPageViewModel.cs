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
            this.Text = "twoja stara klaszcze u rubika. To jest zdanie testowe 1. To jest zdanie testowe 2.";
            this.Voices = this.SpeechSynthesizer.GetInstalledVoices().Select(v => v.VoiceInfo.Name).ToList();
            this.selectedVoice = this.Voices.FirstOrDefault();
            this.IsRunning = false;
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
        }
    }
}
