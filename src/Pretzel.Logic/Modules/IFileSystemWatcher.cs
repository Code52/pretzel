using System;

namespace Pretzel.Logic.Modules
{
    public interface IFileSystemWatcher
    {
        void OnChange(string path, Action<string> fileChangedCallback);
    }
}
