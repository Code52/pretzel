using System.Collections.Generic;
using System.IO;
using Pretzel.Logic.Extensions;

namespace Pretzel.Logic
{
    public interface IDefaultsConfiguration
    {
        IDictionary<string, object> ForScope(string path);
    }

    internal sealed class DefaultsConfiguration : IDefaultsConfiguration
    {
        private readonly IDictionary<string, IDictionary<string, object>> _scopedValues;

        public DefaultsConfiguration(IDictionary<string, object> configuration)
        {
            _scopedValues = new Dictionary<string, IDictionary<string, object>>();
            FillScopedValues(configuration);
        }

        private void FillScopedValues(IDictionary<string, object> configuration)
        {
            if (!configuration.ContainsKey("defaults")) return;

            var defaults = configuration["defaults"] as List<object>;
            if (defaults == null) return;

            foreach (var item in defaults.ConvertAll(x => x as IDictionary<string, object>))
            {
                if (item != null && item.ContainsKey("scope") && item.ContainsKey("values"))
                {
                    var scopeDictionary = item["scope"] as IDictionary<string, object>;
                    if (scopeDictionary != null && scopeDictionary.ContainsKey("path"))
                    {
                        var path = (string)scopeDictionary["path"];
                        var values = item["values"] as IDictionary<string, object>;
                        _scopedValues.Add(path, values ?? new Dictionary<string, object>());
                    }
                }
            }
        }

        public IDictionary<string, object> ForScope(string path)
        {
            IDictionary<string, object> result = new Dictionary<string, object>();

            if (path == null) return result;

            if (path.Length > 0)
            {
                result = result.Merge(ForScope(Path.GetDirectoryName(path)));
            }
            if (_scopedValues.ContainsKey(path))
            {
                result = result.Merge(_scopedValues[path]);
            }
            return result;
        }
    }
}