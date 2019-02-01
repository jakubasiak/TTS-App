using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using NAudio.Lame;
using NAudio.Wave;
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
            //this.Text = "Twoja stara klaszcze u rubika. To jest zdanie testowe 1. To jest zdanie testowe 2. To jest jeszcze dłuższe zdanie testowe, które pozwoli mi na przetestowanie wielu funkcjonalności.";
            this.Voices = this.SpeechSynthesizer.GetInstalledVoices().Select(v => v.VoiceInfo.Name).ToList();
            this.selectedVoice = this.Voices.FirstOrDefault();
            this.IsRunning = false;
            this.CharacterPosition = 0; 
            this.SpeechSynthesizer.SpeakCompleted += this.SpeechSynthesizer_SpeakCompleted;
            this.SpeechSynthesizer.SpeakProgress += this.SpeechSynthesizer_SpeakProgress;
            this.SpeechSynthesizer.SpeakStarted += this.SpeechSynthesizer_SpeakStarted;
        }

        public System.Windows.Controls.TextBox TextBox { get; set; }

        public string FilePath { get; set; }

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

        string currentReadedReadedText;
        public string CurrentReadedText
        {
            get => this.currentReadedReadedText;
            set
            {
                this.currentReadedReadedText = value;
                this.OnPropertyChanged(nameof(this.CurrentReadedText));
            }
        }

        float readProgress;
        public float ReadProgress
        {
            get => this.readProgress;
            set
            {
                this.readProgress = value;
                this.OnPropertyChanged(nameof(this.ReadProgress));
            }
        }

        int characterPosition;
        public int CharacterPosition
        {
            get => this.characterPosition;
            set
            {
                this.characterPosition = value;
                this.OnPropertyChanged(nameof(this.CharacterPosition));
            }
        }

        int caretIndex;
        public int CaretIndex
        {
            get => this.caretIndex;
            set
            {
                this.caretIndex = value;
                this.OnPropertyChanged(nameof(this.CaretIndex));
            }
        }

        private string orginalText;
        public string OrginalText
        {
            get => this.orginalText;
            set
            {
                this.orginalText = value;
                this.OnPropertyChanged(nameof(this.OrginalText));
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
                            this.OrginalText = this.Text;
                            this.SpeechSynthesizer.Rate = this.Rate;
                            this.SpeechSynthesizer.Volume = this.Volume;
                            this.IsRunning = true;
                            this.SpeechSynthesizer.SpeakAsync(this.OrginalText.Substring(this.CaretIndex));
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
                            this.SpeechSynthesizer.Resume();
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

        private ICommand newDocumentCommand;
        public ICommand NewDocumentCommand
        {
            get
            {
                if (this.newDocumentCommand == null)
                    this.newDocumentCommand = new RelayCommand(
                        x =>
                        {
                            this.Text = string.Empty;
                            this.OrginalText = string.Empty;
                            this.SpeechSynthesizer.SpeakAsyncCancelAll();
                            this.selectedVoice = this.Voices.FirstOrDefault();
                            this.IsRunning = false;
                            this.CharacterPosition = 0;
                            this.ReadProgress = this.CalculateProgress();
                        }
                    );

                return this.newDocumentCommand;
            }
        }

        private ICommand openDuumentCommand;
        public ICommand OpenDuumentCommand
        {
            get
            {
                if (this.openDuumentCommand == null)
                    this.openDuumentCommand = new RelayCommand(
                        x =>
                        {
                            OpenFileDialog ofd = new OpenFileDialog();
                            ofd.Multiselect = false;
                            ofd.DefaultExt = ".txt";
                            ofd.Filter = "Text files|*.txt| RTF files|*.rtf";
                            ofd.Title = "Open document";

                             ofd.ShowDialog();

                            if (!string.IsNullOrEmpty(ofd.FileName))
                            {
                                this.FilePath = ofd.FileName;
                                using (var fileStream = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read))
                                {
                                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true))
                                    {
                                        this.Text = streamReader.ReadToEnd();
                                    }
                                }
                            }
                        }
                    );

                return this.openDuumentCommand;
            }
        }

        private ICommand saveDocumentCommand;
        public ICommand SaveDocumentCommand
        {
            get
            {
                if (this.saveDocumentCommand == null)
                    this.saveDocumentCommand = new RelayCommand(
                        x =>
                        {
                            if (!string.IsNullOrEmpty(this.FilePath))
                            {
                                using (var fileStream = new FileStream(this.FilePath, FileMode.OpenOrCreate,
                                    FileAccess.Write))
                                {
                                    using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8))
                                    {
                                        writer.Write(this.Text);
                                    }
                                }
                            }
                            else
                            {
                                this.SaveAsDocumentCommand.Execute(null);
                            }
                        }
                    );

                return this.saveDocumentCommand;
            }
        }

        private ICommand saveAsDocumentCommand;
        public ICommand SaveAsDocumentCommand
        {
            get
            {
                if (this.saveAsDocumentCommand == null)
                    this.saveAsDocumentCommand = new RelayCommand(
                        x =>
                        {
                            if (FilePath == null)
                            {
                                SaveFileDialog sfd = new SaveFileDialog();
                                sfd.DefaultExt = ".txt";
                                sfd.Filter = "Text files|*.txt| RTF files|*.rtf";
                                sfd.Title = "Save document";
                                sfd.FileName = "Document1";

                                sfd.ShowDialog();

                                if (!string.IsNullOrEmpty(sfd.FileName))
                                {
                                    this.FilePath = sfd.FileName;
                                    using (var fileStream = new FileStream(sfd.FileName, FileMode.OpenOrCreate,
                                        FileAccess.Write))
                                    {
                                        using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8))
                                        {
                                            writer.Write(this.Text);
                                        }
                                    }
                                }
                            }
                        }
                    );

                return this.saveAsDocumentCommand;
            }
        }

        private ICommand aboutCommand;
        public ICommand AboutCommand
        {
            get
            {
                if (this.aboutCommand == null)
                    this.aboutCommand = new RelayCommand(
                        x =>
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append("Author: Jakub Kubasiak");
                            sb.AppendLine();
                            sb.Append("e-mail: kubakubasiak@gmail.com");
                            sb.AppendLine();
                            sb.Append("website: http://kubakubasiak.wixsite.com/home");
                            sb.AppendLine();
                            MessageBox.Show(sb.ToString(), "About", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    );

                return this.aboutCommand;
            }
        }

        private ICommand setCursorCommand;
        public ICommand SetCursorCommand
        {
            get
            {
                if (this.setCursorCommand == null)
                    this.setCursorCommand = new RelayCommand(
                        x =>
                        {
                            if (this.TextBox != null && !this.IsRunning)
                            {
                                var m = Mouse.GetPosition(this.TextBox);
                                var index = this.TextBox.GetCharacterIndexFromPoint(m, false);
                                this.CaretIndex = index >= 0 ? index : this.CaretIndex;
                                this.ReadProgress = this.CalculateProgress();
                            }
                        }
                    );

                return this.setCursorCommand;
            }
        }

        private ICommand exportToWavCommand;
        public ICommand ExportToWavCommand
        {
            get
            {
                if (this.exportToWavCommand == null)
                    this.exportToWavCommand = new RelayCommand(
                        x =>
                        {
                            SaveFileDialog sfd = new SaveFileDialog();
                            sfd.DefaultExt = ".wav";
                            sfd.Filter = "Wav files|*.wav";
                            sfd.Title = "Save to file";
                            sfd.FileName = "Track1";

                            sfd.ShowDialog();

                            if (!string.IsNullOrEmpty(sfd.FileName))
                            {

                                this.OrginalText = this.Text;
                                this.SpeechSynthesizer.Rate = this.Rate;
                                this.SpeechSynthesizer.Volume = this.Volume;
                                this.SpeechSynthesizer.SetOutputToWaveFile(sfd.FileName);
                                this.IsRunning = true;
                                ;

                                while (this.SpeechSynthesizer.SpeakAsync(this.OrginalText.Substring(this.CaretIndex)).IsCompleted)
                                {
                                    this.SpeechSynthesizer.SetOutputToDefaultAudioDevice();
                                    this.IsRunning = false;
                                }
                               
                            }
                        }
                    );

                return this.exportToWavCommand;
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

        public static void ConvertWavStreamToMp3File(ref MemoryStream ms, string savetofilename)
        {
            //rewind to beginning of stream
            ms.Seek(0, SeekOrigin.Begin);

            using (var retMs = new MemoryStream())
            using (var rdr = new WaveFileReader(ms))
            using (var wtr = new LameMP3FileWriter(savetofilename, rdr.WaveFormat, LAMEPreset.VBR_90))
            {
                rdr.CopyTo(wtr);
            }
        }

        private void SpeechSynthesizer_SpeakStarted(object sender, SpeakStartedEventArgs e)
        {
            this.orginalText = this.Text;
        }

        private void SpeechSynthesizer_SpeakProgress(object sender, SpeakProgressEventArgs e)
        {
            this.CharacterPosition = this.CaretIndex + e.CharacterPosition;
            this.CurrentReadedText = e.Text;
            this.TextBox.Select(this.CaretIndex + e.CharacterPosition, e.CharacterCount);
            this.ReadProgress = this.CalculateProgress();
        }

        private void SpeechSynthesizer_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            this.CurrentReadedText = null;
            this.Text = this.orginalText;
            this.TextBox.Focus();
            this.TextBox.Select(this.CharacterPosition, 0);
            this.CaretIndex = this.CharacterPosition;
            this.IsRunning = false;
            this.ReadProgress = this.CalculateProgress();
        }

        private float CalculateProgress()
        {
            if (string.IsNullOrEmpty(this.Text))
                return 0;
            if(this.IsRunning)
                return ((float) this.CharacterPosition / this.Text.Length) * 100;
            return ((float)this.CaretIndex / this.Text.Length) * 100;
        }
    }
}
