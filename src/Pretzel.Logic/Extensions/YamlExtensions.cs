using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace Pretzel.Logic.Extensions
{
    public static class YamlExtensions
    {
        private static readonly Regex r = new Regex(@"(?s:^---(.*?)---)");

        public static IDictionary<string, object> YamlHeader(this string text)
        {
            var m = r.Matches(text);
            if (m.Count == 0)
            {
                return new Dictionary<string, object>();
            }

            return m[0].Groups[1].Value.ParseYaml();
        }

        public static IDictionary<string, object> ParseYaml(this string text)
        {
            var results = new Dictionary<string, object>();

            var input = new StringReader(text);

            var yaml = new YamlStream();
            yaml.Load(input);

            if (yaml.Documents.Count == 0)
            {
                return results;
            }

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

            var list = value as YamlSequenceNode;
            if (list != null)
            {
                if (list.Children.All(_ => _ is YamlScalarNode)) {
                    var listString = new List<string>();
                    foreach (var entry in list.Children)
                    {
                        var node = entry as YamlScalarNode;
                        if (node != null) {
                            listString.Add(node.Value);
                        }
                    }
                    return listString;
                } else {
                    var listResults = new List<object>();
                    foreach (var entry in list.Children)
                    {
                        listResults.Add(GetValue(entry));
                    }
                    return listResults;
                }
            }

            bool valueBool;
            if (bool.TryParse(value.ToString(), out valueBool))
            {
                return valueBool;
            }

            return value.ToString();
        }

        public static string ToYaml<T>(this T model)
        {
            var stringWriter = new StringWriter();
            var serializer = new Serializer();
            serializer.Serialize(stringWriter, model, typeof(T));
            return stringWriter.ToString();
        }

        public static string ExcludeHeader(this string text)
        {
            var m = r.Matches(text);
            if (m.Count == 0)
                return text;

            return text.Replace(m[0].Groups[0].Value, "").TrimStart('\r', '\n').TrimEnd();
        }
    }
}
