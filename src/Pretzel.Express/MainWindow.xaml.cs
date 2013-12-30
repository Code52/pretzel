using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Pretzel
{
    public partial class MainWindow
    {
        private MainViewModel ViewModel { get { return (MainViewModel)DataContext; } }

        private NotifyIcon notifyIcon;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            App.Current.Exit += Current_Exit;
        }

        void Current_Exit(object sender, ExitEventArgs e)
        {
            var settings = new SiteSettings() {Sites = new List<SiteConfig>()};
            foreach (var s in ViewModel.Sites)
            {
                settings.Sites.Add(new SiteConfig(s.Directory, s.Port, true));
            }

            Properties.Settings.Default.Sites = settings;
            Properties.Settings.Default.Save();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs ie)
        {
            var x = new MainViewModel();
            x.Compose();

            if (Properties.Settings.Default.Sites != null)
            {
                foreach (var s in Properties.Settings.Default.Sites.Sites)
                {
                    x.StartNewSite(s.Directory, s.Port);
                }
            }

            DataContext = x;

            Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/Pretzel.Express;component/pretzel.ico")).Stream;
            notifyIcon = new NotifyIcon
            {
                BalloonTipText = "Pretzel Express, choo choo!",
                Text = "Pretzel Express, choo choo!",
                Icon = new Icon(iconStream),
                Visible = true
            };

            var contextMenuItems = new List<MenuItem>
            {
                new MenuItem("Start New Site", (s, e) => ButtonBase_OnClick(null, null)),
                new MenuItem("-"),
                new MenuItem("Exit", (s, e) => Application.Current.Shutdown())
            };

            notifyIcon.ContextMenu = new ContextMenu(contextMenuItems.ToArray());
            notifyIcon.Click += notifyIcon_Click;
        }

        void notifyIcon_Click(object sender, EventArgs e)
        {
            var args = (MouseEventArgs)e;
            if (args.Button != MouseButtons.Left)
                return;

            if (IsVisible)
                Hide();
            else
                Show();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "New Pretzel Site";
            dlg.IsFolderPicker = true;

            var result = dlg.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                ViewModel.StartNewSite(dlg.FileName);
            }
        }
    }
}
