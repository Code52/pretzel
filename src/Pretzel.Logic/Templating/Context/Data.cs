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
        private System.Lazy<object> cachedResult;

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

                if (cachedResult == null)
                {
                    cachedResult = new Lazy<object>(() =>
                    {
                        if (!fileSystem.Directory.Exists(dataDirectory))
                        {
                            return null;
                        }

                        object result;

                        if (TryParseYaml(dataDirectory, method.ToString(), out result))
                        {
                            return result;
                        }

                        if (TryParseYaml(dataDirectory, method.ToString(), out result, "json"))
                        {
                            return result;
                        }

                        var subFolder = Path.Combine(dataDirectory, method.ToString());
                        if (fileSystem.Directory.Exists(subFolder))
                        {
                            return new Data(fileSystem, subFolder);
                        }

                        return null;
                    });
                    
                }

                return cachedResult.Value;
            }
        }

        bool TryParseYaml(string folder, string methodName, out object yamlResult, string ext = "yml")
        {
            var yamlFileName = Path.Combine(folder, $"{methodName}.{ext}");
            if (fileSystem.File.Exists(yamlFileName))
            {
                var text = fileSystem.File.ReadAllText(yamlFileName);

                var input = new StringReader(text);

                var yaml = new YamlStream();
                yaml.Load(input);

                if (yaml.Documents.Count == 0)
                {
                    yamlResult = null;
                    return false;
                }

                var root = yaml.Documents[0].RootNode;
                if (root is YamlSequenceNode seq)
                {
                    yamlResult = seq;
                    return true;
                }

                yamlResult = text.ParseYaml();
                return yamlResult != null;
            }
            yamlResult = null;
            return false;
        }
    }
}
