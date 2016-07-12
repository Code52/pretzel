using Pretzel.Logic.Extensions;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

namespace Pretzel.Logic
{
    public interface IConfiguration
    {
        object this[string key] { get; }

        bool ContainsKey(string key);

        bool TryGetValue(string key, out object value);

        IDictionary<string, object> ToDictionary();

        IDefaultsConfiguration GetDefaults();
    }


    public interface IDefaultsConfiguration
    {
        IDictionary<string, object> ForScope(string path);
    }


    internal sealed class Configuration : IConfiguration
    {
        private const string ConfigFileName = "_config.yml";
        public const string DefaultPermalink = "date";

        private IDictionary<string, object> _config;
        private readonly IFileSystem _fileSystem;
        private readonly string _configFilePath;

        public object this[string key]
        {
            get
            {
                return _config[key];
            }
        }

        internal Configuration()
        {
            _config = new Dictionary<string, object>();
            CheckDefaultConfig();
        }

        internal Configuration(IFileSystem fileSystem, string sitePath)
            : this()
        {
            _fileSystem = fileSystem;
            _configFilePath = _fileSystem.Path.Combine(sitePath, ConfigFileName);
        }

        private void CheckDefaultConfig()
        {
            if (!_config.ContainsKey("permalink"))
            {
                _config.Add("permalink", DefaultPermalink);
            }
        }

        internal void ReadFromFile()
        {
            _config = new Dictionary<string, object>();
            if (_fileSystem.File.Exists(_configFilePath))
            {
                _config = _fileSystem.File.ReadAllText(_configFilePath).ParseYaml();
                CheckDefaultConfig();
            }
        }

        public bool ContainsKey(string key)
        {
            return _config.ContainsKey(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return _config.TryGetValue(key, out value);
        }

        public IDictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>(_config);
        }

        public IDefaultsConfiguration GetDefaults()
        {
            return new DefaultsConfiguration(_config);
        }
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
