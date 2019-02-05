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
using MahApps.Metro.Controls;
using TTS.ViewModel;
using Clipboard = System.Windows.Clipboard;

namespace TTS
{
    public partial class MainWindow : MetroWindow
    {
        private System.Windows.Forms.NotifyIcon notifyIcon;
        public MainWindow()
        {
            this.InitializeComponent();
            ((MainPageViewModel) this.DataContext).TextBox = this.TextBox;
            this.CreateNotifyIconMenu();


        }

        private void CreateNotifyIconMenu()
        {
            this.notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                BalloonTipTitle = Properties.Resources.MainWindow_MainWindow_Tip_Title,
                BalloonTipText = Properties.Resources.MainWindow_MainWindow_Tip_Text,
                Icon = new System.Drawing.Icon(@"../../Assets/sound.ico", new System.Drawing.Size(16, 16)),
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
            readclipboardMenuItem.Click += this.ReadclipboardMenuItem_Click;
            this.notifyIcon.ContextMenuStrip.Items.AddRange(new ToolStripItem[]
                {readclipboardMenuItem, new ToolStripSeparator(), closeMenuItem});
        }

        private void ReadclipboardMenuItem_Click(object sender, EventArgs e)
        {
            var textToRead = Clipboard.GetText();
            ((MainPageViewModel)this.DataContext).PlayClipboardCommand.Execute(textToRead);
        }

        private void CloseMenuItem_Click(object sender, EventArgs e)
        {
            this.notifyIcon.Visible = false;
            ((MainPageViewModel)this.DataContext).WindowCloseCommand.Execute(this);
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
    }
}
