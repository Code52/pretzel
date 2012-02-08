using System;

namespace Pretzel.Modules
{
    public interface IFileSystemWatcher
    {
        void OnChange(string path, Action<string> fileChangedCallback);
    }
}