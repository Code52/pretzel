using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace Pretzel
{
    public partial class MainWindow
    {
        private MainViewModel viewModel { get { return (MainViewModel)DataContext; } }
        private NotifyIcon notifyIcon;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs ie)
        {
            var x = new MainViewModel();
            x.Compose();
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
            var dlg = new FolderBrowserDialog();
            DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                viewModel.StartNewSite(dlg.SelectedPath);
            }
        }
    }
}
