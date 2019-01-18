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
    public class MainPageViewModel: INotifyPropertyChanged
    {
        public MainPageViewModel()
        {
            this.Rate = 0;
            this.Volume = 50;
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

        ObservableCollection<string> voices;
        public ObservableCollection<string> Voices
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
                    this.pauseCommand = new PauseCommand();
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
                            this.SpeechSynthesizer.Dispose();
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
        }
    }
}
