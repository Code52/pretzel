using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Abstractions;

namespace Pretzel.Logic.Minification
{
    public static class FileSystemExtensions
    {
        public static string BundleFiles(this IFileSystem fileSystem, IEnumerable<FileInfo> filePaths)
        {
            var outputCss = new StringBuilder();

            filePaths.Select(file => fileSystem.File.ReadAllText(file.FullName))
                .Aggregate(outputCss, (builder, val) => builder.Append(val + "\n"));

            return outputCss.ToString();
        }
    }
}
