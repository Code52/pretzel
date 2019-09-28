using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Pretzel.Logic.Extensions;
using YamlDotNet.RepresentationModel;

namespace Pretzel.Logic.Templating.Context.DataParsing
{
    internal class YamlJsonDataParser : AbstractDataParser
    {
        internal YamlJsonDataParser(IFileSystem fileSystem, string extension) : base(fileSystem, extension)
        {

        }

        public override object Parse(string folder, string method)
        {
            var text = FileSystem.File.ReadAllText(BuildFilePath(folder, method));

            var input = new StringReader(text);

            var yaml = new YamlStream();
            yaml.Load(input);

            if (yaml.Documents.Count == 0)
            {
                return null;
            }

            var root = yaml.Documents[0].RootNode;
            if (root is YamlSequenceNode seq)
            {
                return seq;
            }

            return text.ParseYaml();
        }
    }
}
