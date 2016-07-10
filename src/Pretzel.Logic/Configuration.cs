using Pretzel.Logic.Extensions;
using System.Collections.Generic;
using System.IO.Abstractions;

namespace Pretzel.Logic
{
    public interface IConfiguration
    {
        object this[string key] { get; }

        bool ContainsKey(string key);

        bool TryGetValue(string key, out object value);

        IDictionary<string, object> ToDictionary();
    }

    internal sealed class Configuration : IConfiguration
    {
        private const string ConfigFileName = "_config.yml";

        private IDictionary<string, object> _config;
        private IFileSystem _fileSystem;
        private string _configFilePath;

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
                _config.Add("permalink", "date");
            }
            if (!_config.ContainsKey("date"))
            {
                _config.Add("date", "2012-01-01");
            }
            if (!_config.ContainsKey("configuration"))
            {
                _config.Add("configuration", "debug");
            }
        }

        internal void ReadFromFile(IDictionary<string, string> defaults = null)
        {
            _config = new Dictionary<string, object>();
            if (_fileSystem.File.Exists(_configFilePath))
            {
                _config = _fileSystem.File.ReadAllText(_configFilePath).ParseYaml();
                CheckDefaultConfig();
            }

            if (defaults != null)
            {
                foreach (var key in defaults)
                {
                    _config[key.Key] = key.Value;
                }
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
    }
}
