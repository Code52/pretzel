using System;
using System.Linq;

namespace Pretzel.Logic.Commands
{
    public class SourcePathProvider
    {
        public string Path { get; }

        public SourcePathProvider(string path)
        {
            Path = path;
        }
    }
}
