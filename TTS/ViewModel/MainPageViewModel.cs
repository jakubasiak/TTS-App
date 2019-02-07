using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using NAudio.Wave;
using TTS.Annotations;
using TTS.Command;

namespace TTS.ViewModel
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public MainPageViewModel()
        {
            this.Rate = Properties.Settings.Default.Rate;
            this.Volume = Properties.Settings.Default.Volume;
            this.Voices = this.SpeechSynthesizer.GetInstalledVoices().Select(v => v.VoiceInfo.Name).ToList();
            this.SelectedVoice = string.IsNullOrEmpty(Properties.Settings.Default.SelectedVoice) ? this.Voices.FirstOrDefault() : Properties.Settings.Default.SelectedVoice;
            this.ApplicationState = ApplicationState.Idle;
            this.CaretIndex = 0;
            this.SpeechSynthesizer.SpeakCompleted += this.SpeechSynthesizer_SpeakCompleted;
            this.SpeechSynthesizer.SpeakProgress += this.SpeechSynthesizer_SpeakProgress;
            this.SpeechSynthesizer.SpeakStarted += this.SpeechSynthesizer_SpeakStarted;
            this.ClipboardSpeechSynthesizer.SetOutputToDefaultAudioDevice();
            this.ConfigureSpeachRecognition();
        }

        #region Properties
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

        private SpeechSynthesizer clipboardSpeechSynthesizer;
        public SpeechSynthesizer ClipboardSpeechSynthesizer
        {
            get
            {
                if (this.clipboardSpeechSynthesizer == null)
                {
                    this.clipboardSpeechSynthesizer = new SpeechSynthesizer();
                }
                return this.clipboardSpeechSynthesizer;
            }
        }

        private SpeechRecognitionEngine speechRecognitionEngine;
        public SpeechRecognitionEngine SpeechRecognitionEngine
        {
            get
            {
                if (this.speechRecognitionEngine == null)
                {
                    this.speechRecognitionEngine = new SpeechRecognitionEngine();
                }
                return this.speechRecognitionEngine;
            }
        }

        ApplicationState applicationState;
        public ApplicationState ApplicationState
        {
            get => this.applicationState;
            set
            {
                this.applicationState = value;
                this.OnPropertyChanged(nameof(this.ApplicationState));
            }
        }

        bool speechRecognitionEnabled;
        public bool SpeechRecognitionEnabled
        {
            get => this.speechRecognitionEnabled;
            set
            {
                this.speechRecognitionEnabled = value;
                this.OnPropertyChanged(nameof(this.SpeechRecognitionEnabled));
            }
        }

        bool voiceControl;
        public bool VoiceControl
        {
            get => this.voiceControl;
            set
            {
                this.voiceControl = value;
                this.OnPropertyChanged(nameof(this.VoiceControl));
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
        #endregion

        #region Commands

        private ICommand readCommand;
        public ICommand ReadCommand
        {
            get
            {
                if (this.readCommand == null)
                    this.readCommand = new RelayCommand(
                        x =>
                        {
                            if (this.ApplicationState == ApplicationState.Idle)
                            {
                                if (!string.IsNullOrEmpty(this.Text))
                                {
                                    this.SpeechSynthesizer.Rate = this.Rate;
                                    this.SpeechSynthesizer.Volume = this.Volume;
                                    this.ApplicationState = ApplicationState.Read;
                                    this.SpeechSynthesizer.SpeakAsync(this.Text.Substring(this.CaretIndex));
                                }
                            }
                            else if (this.ApplicationState == ApplicationState.Read)
                            {
                                this.ApplicationState = ApplicationState.Pause;
                                this.SpeechSynthesizer.Pause();
                            }
                            else if (this.ApplicationState == ApplicationState.Pause)
                            {
                                this.ApplicationState = ApplicationState.Read;
                                this.SpeechSynthesizer.Resume();
                            }

                        }
                    );
                return this.readCommand;
            }
        }

        private ICommand readFromClipboardCommand;
        public ICommand ReadFromClipboardCommand
        {
            get
            {
                if (this.readFromClipboardCommand == null)
                    this.readFromClipboardCommand = new RelayCommand(
                        x =>
                        {
                            if (this.ClipboardSpeechSynthesizer.State == SynthesizerState.Speaking)
                            {
                                this.ClipboardSpeechSynthesizer.SpeakAsyncCancelAll();
                            }
                            else
                            {
                                var textToRead = Clipboard.GetText();
                                if (!string.IsNullOrEmpty(textToRead) && this.ApplicationState == ApplicationState.Idle)
                                {
                                    this.ClipboardSpeechSynthesizer.Rate = this.Rate;
                                    this.ClipboardSpeechSynthesizer.Volume = this.Volume;
                                    this.ClipboardSpeechSynthesizer.SelectVoice(this.SelectedVoice);
                                    this.ClipboardSpeechSynthesizer.SpeakAsync(textToRead);
                                }
                            }
                        }
                    );
                return this.readFromClipboardCommand;
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
                            this.SpeechSynthesizer.SpeakAsyncCancelAll();
                            this.SpeechSynthesizer.Dispose();

                            Properties.Settings.Default.Volume = this.Volume;
                            Properties.Settings.Default.Rate = this.Rate;
                            Properties.Settings.Default.SelectedVoice = this.SelectedVoice;
                            Properties.Settings.Default.Save();
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
                            this.SpeechSynthesizer.SpeakAsyncCancelAll();
                            this.selectedVoice = this.Voices.FirstOrDefault();
                            this.ApplicationState = ApplicationState.Idle;
                            this.CaretIndex = 0;
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
                            ofd.Multiselect = false;

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
                            sb.Append("github: https://github.com/jakubasiak");
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
                            if (!string.IsNullOrEmpty(this.Text) && this.ApplicationState == ApplicationState.Idle)
                            {
                                var m = Mouse.GetPosition(this.TextBox);
                                var index = this.TextBox.GetCharacterIndexFromPoint(m, false);
                                this.CaretIndex = index < 0 ? 0 : index;
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
                                this.SpeechSynthesizer.Rate = this.Rate;
                                this.SpeechSynthesizer.Volume = this.Volume;
                                this.SpeechSynthesizer.SetOutputToWaveFile(sfd.FileName);
                                this.ApplicationState = ApplicationState.Read;
                                this.SpeechSynthesizer.SpeakAsync(this.Text.Substring(this.CaretIndex));

                                void OnComplete(object sender, SpeakCompletedEventArgs args)
                                {
                                    this.SpeechSynthesizer.SetOutputToDefaultAudioDevice();
                                    this.ApplicationState = ApplicationState.Idle;
                                    this.SpeechSynthesizer.SpeakCompleted -= OnComplete;
                                }

                                this.SpeechSynthesizer.SpeakCompleted += OnComplete;

                            }
                        }
                    );

                return this.exportToWavCommand;
            }
        }

        #endregion

        #region Methods
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
                if (this.ApplicationState == ApplicationState.Read)
                {
                    this.ApplicationState = ApplicationState.Idle;
                    this.SpeechSynthesizer.SpeakAsyncCancelAll();
                }
            }

            if (propertyName == nameof(this.VoiceControl))
            {
                if (this.VoiceControl)
                {
                    this.SpeechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
                }
                else
                {
                    this.SpeechRecognitionEngine.RecognizeAsyncCancel();
                }
            }

            if (propertyName == nameof(this.Text))
            {
                this.Reset();
            }
        }

        private void Reset()
        {
            this.SpeechSynthesizer.SpeakCompleted -= this.SpeechSynthesizer_SpeakCompleted;
            this.SpeechSynthesizer.Resume();
            this.SpeechSynthesizer.SpeakAsyncCancelAll();
            this.SpeechSynthesizer.SpeakCompleted += this.SpeechSynthesizer_SpeakCompleted;
            this.ApplicationState = ApplicationState.Idle;
            this.CurrentReadedText = null;

            this.CaretIndex = 0;
            this.TextBox.Focus();
            this.TextBox.Select(0, 0);

            this.ReadProgress = this.CalculateProgress();
        }

        private void ConfigureSpeachRecognition()
        {
            try
            {
                Choices commands = new Choices();
                commands.Add(new string[] { "read", "pouse", "resum", "stop", "read clipboard", "stop reading clipboard", "disable voice control", "close" });
                GrammarBuilder grammarBuilder = new GrammarBuilder();
                grammarBuilder.Append(commands);
                Grammar grammar = new Grammar(grammarBuilder);
                this.SpeechRecognitionEngine.LoadGrammarAsync(grammar);
                this.SpeechRecognitionEngine.SetInputToDefaultAudioDevice();
                this.SpeechRecognitionEngine.SpeechRecognized += this.SpeechRecognitionEngine_SpeechRecognized;
                this.SpeechRecognitionEnabled = true;
            }
            catch (Exception e)
            {
                this.SpeechRecognitionEnabled = false;
            }

        }

        private void SpeechRecognitionEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            switch (e.Result.Text)
            {
                case "read":
                    this.ReadCommand.Execute(null);
                    break;
                case "pouse":
                    this.ReadCommand.Execute(null);
                    break;
                case "resum":
                    this.ReadCommand.Execute(null);
                    break;
                case "stop":
                    this.StopCommand.Execute(null);
                    break;
                case "read clipboard":
                    this.ReadFromClipboardCommand.Execute(null);
                    break;
                case "stop reading clipboard":
                    this.ReadFromClipboardCommand.Execute(null);
                    break;
                case "disable voice control":
                    this.VoiceControl = false;
                    break;
                case "close":
                    this.WindowCloseCommand.Execute(null);
                    break;
            }
        }

        private int beginPossition = 0;
        private void SpeechSynthesizer_SpeakStarted(object sender, SpeakStartedEventArgs e)
        {
            this.beginPossition = this.CaretIndex;
        }

        private void SpeechSynthesizer_SpeakProgress(object sender, SpeakProgressEventArgs e)
        {
            this.CaretIndex = this.beginPossition + e.CharacterPosition;
            this.CurrentReadedText = e.Text;
            this.TextBox.Select(this.CaretIndex, e.CharacterCount);
            this.ReadProgress = this.CalculateProgress();
        }

        private void SpeechSynthesizer_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            this.CurrentReadedText = null;
            this.TextBox.Focus();
            this.TextBox.Select(this.CaretIndex, 0);
            this.ReadProgress = this.CalculateProgress();
            this.ApplicationState = ApplicationState.Idle;
        }

        private float CalculateProgress()
        {
            return string.IsNullOrEmpty(this.Text) ? 0 : ((float)this.CaretIndex / this.Text.Length) * 100;
        }
        #endregion

    }
}
