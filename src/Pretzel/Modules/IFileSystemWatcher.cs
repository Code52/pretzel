using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pretzel.Modules
{
    public interface IFileSystemWatcher
    {
        void OnChange(string path, Action<string> fileChangedCallback);
    }
}
