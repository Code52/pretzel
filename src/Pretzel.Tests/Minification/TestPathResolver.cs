using System.IO;
using dotless.Core.Input;

namespace Pretzel.Tests.Minification
{
    public class TestPathResolver : IPathResolver
    {
        private readonly string directory;

        public TestPathResolver(string directory)
        {
            this.directory = directory;
        }

        public string GetFullPath(string path)
        {
            return Path.Combine(directory, path);
        }
    }
}