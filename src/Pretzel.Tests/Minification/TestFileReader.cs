using IFileSystem = System.IO.Abstractions.IFileSystem;
using IPathResolver = dotless.Core.Input.IPathResolver;
using IFileReader = dotless.Core.Input.IFileReader;

namespace Pretzel.Tests.Minification
{
    public class TestFileReader : IFileReader
    {
        private readonly IFileSystem fileSystem;
        private readonly IPathResolver pathResolver;

        public TestFileReader(IFileSystem fileSystem, IPathResolver pathResolver)
        {
            this.fileSystem = fileSystem;
            this.pathResolver = pathResolver;
        }

        public byte[] GetBinaryFileContents(string fileName)
        {
            var path = pathResolver.GetFullPath(fileName);
            return fileSystem.File.ReadAllBytes(path);
        }

        public string GetFileContents(string fileName)
        {
            var path = pathResolver.GetFullPath(fileName);
            return fileSystem.File.ReadAllText(path);
        }

        public bool DoesFileExist(string fileName)
        {
            var path = pathResolver.GetFullPath(fileName);
            return fileSystem.File.Exists(path);
        }

        public bool UseCacheDependencies
        {
            get { return true; }
        }
    }
}