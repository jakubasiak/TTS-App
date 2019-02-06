using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GlobalHotKey;
using MahApps.Metro.Controls;
using TTS.ViewModel;
using Clipboard = System.Windows.Clipboard;
using HotKey = GlobalHotKey.HotKey;
using MessageBox = System.Windows.MessageBox;

namespace TTS
{
    public partial class MainWindow : MetroWindow
    {
        private System.Windows.Forms.NotifyIcon notifyIcon;
        public HotKeyManager HotKeyManager { get; set; }
        public HotKey ReadClipboardHotKey { get; set; }
        public MainPageViewModel ViewModel { get; set; }
        public MainWindow()
        {
            this.InitializeComponent();
            this.ViewModel = (MainPageViewModel) this.DataContext;
            this.ViewModel.TextBox = this.TextBox;
            this.CreateNotifyIconMenu();

            this.HotKeyManager = new HotKeyManager();
            this.ReadClipboardHotKey = this.HotKeyManager.Register(Key.Q, ModifierKeys.Control);
            this.HotKeyManager.KeyPressed += this.HotKeyManager_KeyPressed;
        }

        private void HotKeyManager_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (e.HotKey.Key == Key.Q)
                this.PlayClipboard();
        }

        private void CreateNotifyIconMenu()
        {
            this.notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                BalloonTipTitle = Properties.Resources.MainWindow_MainWindow_Tip_Title,
                BalloonTipText = Properties.Resources.MainWindow_MainWindow_Tip_Text,
                Text = Properties.Resources.MainWindow_MainWindow_Tip_Title,
                Icon = new System.Drawing.Icon(@"../../Assets/speaker.ico", new System.Drawing.Size(16, 16)),
                Visible = true
            };
            this.notifyIcon.DoubleClick += this.NotifyIcon_DoubleClick;
            this.notifyIcon.ContextMenuStrip = new ContextMenuStrip();

            var closeMenuItem = new ToolStripMenuItem
            {
                Text = Properties.Resources.MainWindow_MainWindow_CloseCommandTitle
            };
            closeMenuItem.Click += this.CloseMenuItem_Click;

            var readclipboardMenuItem = new ToolStripMenuItem
            {
                Text = Properties.Resources.MainWindow_MainWindow_ReadClipboardCommandTitle
            };
            readclipboardMenuItem.Click += this.ReadClipboardMenuItem_Click;

            var voiceControlMenuItem = new ToolStripMenuItem
            {
                Text = Properties.Resources.MainWindow_CreateNotifyIconMenu_VoiceControl,
                Checked = this.ViewModel.VoiceControl,
                CheckOnClick = true,
                Enabled = this.ViewModel.SpeechRecognitionEnabled
            };
            voiceControlMenuItem.CheckedChanged += this.VoiceControlMenuItem_CheckedChanged;

            this.notifyIcon.ContextMenuStrip.Items.AddRange(new ToolStripItem[]
                {readclipboardMenuItem, voiceControlMenuItem, new ToolStripSeparator(), closeMenuItem});
        }

        private void VoiceControlMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            this.ViewModel.VoiceControl = !this.ViewModel.VoiceControl;
        }

        private void CloseMenuItem_Click(object sender, EventArgs e)
        {
            this.notifyIcon.Visible = false;
            this.ViewModel.WindowCloseCommand.Execute(this);
        }

        private void ReadClipboardMenuItem_Click(object sender, EventArgs e)
        {
            this.PlayClipboard();
        }

        private void PlayClipboard()
        {
            this.ViewModel.ReadFromClipboardCommand.Execute(null);
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        private void MainWindow_OnStateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
                this.notifyIcon.Visible = true;
                this.notifyIcon.ShowBalloonTip(1000);
            }
            else if (this.WindowState == WindowState.Normal)
            {
                this.notifyIcon.Visible = false;
            }
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            this.HotKeyManager.Unregister(this.ReadClipboardHotKey);
            this.HotKeyManager.Dispose();
            this.notifyIcon.Dispose();
            this.ViewModel.WindowCloseCommand.Execute(this);
        }
    }
}
