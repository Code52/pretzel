using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using YamlDotNet.RepresentationModel;
using YamlDotNet.RepresentationModel.Serialization;

namespace Pretzel.Logic.Extensions
{
    public static class YamlExtensions
    {
        static readonly Regex r = new Regex(@"^---([\d\D\w\W\s\S]+)---", RegexOptions.Multiline);
        public static IDictionary<string, object> YamlHeader(this string text, bool skipHeader = false)
        {
            StringReader input;
            var results = new Dictionary<string, object>();

            if (skipHeader)
            {
                input = new StringReader(text);
            }
            else
            {
                var m = r.Matches(text);
                if (m.Count == 0)
                    return results;

                input = new StringReader(m[0].Groups[1].Value);
            }

            var yaml = new YamlStream();
            yaml.Load(input);

            if (yaml.Documents.Count == 0)
                return results;

            var root = yaml.Documents[0].RootNode;

            var collection = root as YamlMappingNode;
            if (collection != null)
            {
                foreach (var entry in collection.Children)
                {
                    var node = entry.Key as YamlScalarNode;
                    if (node != null)
                    {
                        results.Add(node.Value, GetValue(entry.Value));    
                    }
                }
            }

            return results;
        }

        private static object GetValue(YamlNode value)
        {
            var collection = value as YamlMappingNode;
            if (collection != null)
            {
                var results = new Dictionary<string, object>();
                foreach (var entry in collection.Children)
                {
                    var node = entry.Key as YamlScalarNode;
                    if (node != null)
                    {
                        results.Add(node.Value, GetValue(entry.Value));
                    }
                }

                return results;
            }

            return value.ToString();
        }

        public static string ToYaml<T>(this T model)
        {
            var serializer = new YamlSerializer(typeof(T));
            var stringWriter = new StringWriter();

            serializer.Serialize(stringWriter, model);

            return stringWriter.ToString();
        }

        public static string ExcludeHeader(this string text)
        {
            var m = r.Matches(text);
            if (m.Count == 0)
                return text;

            return text.Replace(m[0].Groups[0].Value, "").Trim();
        }
    }
}
