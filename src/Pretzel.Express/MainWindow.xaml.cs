using System;
using System.Collections.Generic;
using System.Windows;
using Application = System.Windows.Application;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Pretzel
{
    public partial class MainWindow
    {
        private MainViewModel ViewModel { get { return (MainViewModel)DataContext; } }
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            App.Current.Exit += Current_Exit;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                Properties.Settings.Default.Minimised = true;
                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.Minimised = false;
                Properties.Settings.Default.Save();
            }

            base.OnStateChanged(e);
        }

        void Current_Exit(object sender, ExitEventArgs e)
        {
            var settings = new SiteSettings() { Sites = new List<SiteConfig>() };
            foreach (var s in ViewModel.Sites)
            {
                settings.Sites.Add(new SiteConfig(s.Directory, s.Port, true));
            }

            Properties.Settings.Default.Sites = settings;
            Properties.Settings.Default.Save();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs ie)
        {
            if (Properties.Settings.Default.Minimised)
            {
                Hide();
            }

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
        }

        private void StartNewSiteClick(object sender, RoutedEventArgs e)
        {
            var dlg = new CommonOpenFileDialog
            {
                Title = "New Pretzel Site", 
                IsFolderPicker = true
            };

            var result = dlg.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                ViewModel.StartNewSite(dlg.FileName);
            }
        }

        private void ExitClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Ni_OnTrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Show();
        }
    }
}
