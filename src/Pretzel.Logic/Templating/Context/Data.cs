using System.Collections.Generic;
using DotLiquid;
using Pretzel.Logic.Extensions;
using System.IO;
using System.IO.Abstractions;
using System.Collections;
using System;
using YamlDotNet.RepresentationModel;

namespace Pretzel.Logic.Templating.Context
{
    public class Data : Drop
    {
        private readonly IFileSystem fileSystem;
        private readonly string dataDirectory;

        public Data(IFileSystem fileSystem, string dataDirectory)
        {
            this.fileSystem = fileSystem;
            this.dataDirectory = dataDirectory;
        }

        public override object this[object method]
        {
            get
            {

                var res = base[method];
                if(res != null)
                {
                    return res;
                }

                if (!fileSystem.Directory.Exists(dataDirectory))
                {
                    return null;
                }

                var yamlFileName = Path.Combine(dataDirectory, $"{method}.yml");
                if (fileSystem.File.Exists(yamlFileName))
                {
                    var text = fileSystem.File.ReadAllText(yamlFileName);

                    var input = new StringReader(text);

                    var yaml = new YamlStream();
                    yaml.Load(input);

                    if (yaml.Documents.Count == 0)
                    {
                        return null;
                    }

                    var root = yaml.Documents[0].RootNode;
                    if (root is YamlSequenceNode seq)
                        return seq;

                    return text.ParseYaml();
                }

                return null;
            }
        }
    }
}
