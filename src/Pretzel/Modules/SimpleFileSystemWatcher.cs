using System;
using System.IO;

namespace Pretzel.Modules
{
    public class SimpleFileSystemWatcher : IFileSystemWatcher, IDisposable
    {
        readonly FileSystemWatcher watcher = new FileSystemWatcher();
        Action<string> callback;

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

        string lastFile;
        private void WatcherOnChanged(object sender, FileSystemEventArgs args)
        {
            if (args.FullPath.Contains("_site"))
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