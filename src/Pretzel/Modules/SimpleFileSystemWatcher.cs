using System;
using System.IO;

namespace Pretzel.Modules
{
    public class SimpleFileSystemWatcher : IFileSystemWatcher, IDisposable
    {
        private readonly FileSystemWatcher watcher;
        private readonly string destinationPath;
        private Action<string> callback;

        public SimpleFileSystemWatcher(string destinationPath)
        {
            watcher = new FileSystemWatcher();
            this.destinationPath = destinationPath;
        }

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

        private string lastFile;

        private void WatcherOnChanged(object sender, FileSystemEventArgs args)
        {
            if (args.FullPath.Contains(destinationPath))
                return;

            if (args.FullPath == lastFile)
            {
                lastFile = "";
                return;
            }

            callback(args.FullPath);
            lastFile = args.FullPath;
        }
    }
}
