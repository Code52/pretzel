using System;
using System.IO;

namespace Pretzel.Modules
{
    public class SimpleFileSystemWatcher : IFileSystemWatcher, IDisposable
    {
        FileSystemWatcher watcher = new FileSystemWatcher();
        private Action<string> callback;

        public void OnChange(string path, Action<string> fileChangedCallback)
        {
            callback = fileChangedCallback;

            watcher.Path = path; 
            watcher.Filter = "*.*";
            watcher.IncludeSubdirectories = true;
            watcher.Changed += WatcherOnChanged;
            watcher.Created += WatcherOnChanged;
            watcher.EnableRaisingEvents = true;
        }
        public void Dispose()
        {
            watcher.EnableRaisingEvents = false;
            watcher.Changed -= WatcherOnChanged;
            watcher.Created -= WatcherOnChanged;
        }

        private void WatcherOnChanged(object sender, FileSystemEventArgs args)
        {
            if (args.FullPath.Contains("_site"))
                return;

            callback(args.FullPath);
        }
    }
}