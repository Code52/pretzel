using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace Pretzel.Logic.Templating.Context.DataParsing
{
    internal abstract class AbstractDataParser : IDataParser
    {
        protected IFileSystem FileSystem { get; }
        public string Extension { get; private set; }

        protected AbstractDataParser(IFileSystem fileSystem, string extension)
        {
            FileSystem = fileSystem;
            Extension = extension;
        }

        protected string BuildFilePath(string folder, string method) => Path.Combine(folder, $"{method}.{Extension}");

        public virtual bool CanParse(string folder, string method) => FileSystem.File.Exists(BuildFilePath(folder, method));

        public abstract object Parse(string folder, string method);
    }
}
