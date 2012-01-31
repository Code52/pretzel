﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.RepresentationModel;
using System.IO;

namespace Pretzel.Logic.Extensions
{
    public static class YamlExtensions
    {
        static Regex r = new Regex(@"^---([\d\D\w\W\s\S]+)---", RegexOptions.Multiline);
        public static Dictionary<string, object> YamlHeader(this string text)
        {
            var results = new Dictionary<string, object>();
            var m = r.Matches(text);
            if (m.Count == 0)
                return null;

            var input = new StringReader(m[0].Groups[1].Value);

            var yaml = new YamlStream();
            yaml.Load(input);
            var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
            foreach (KeyValuePair<YamlNode, YamlNode> entry in mapping.Children)
            {
                var key = ((YamlScalarNode)entry.Key).Value;
                results.Add(key, entry.Value);
            }

            return results;
        }
    }
}
