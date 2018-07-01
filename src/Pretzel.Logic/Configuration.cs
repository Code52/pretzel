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

        IDefaultsConfiguration Defaults { get; }
    }


    internal sealed class Configuration : IConfiguration
    {
        private const string ConfigFileName = "_config.yml";
        public const string DefaultPermalink = "date";

        private IDictionary<string, object> _config;
        private IDefaultsConfiguration _defaultsConfiguration;
        private readonly IFileSystem _fileSystem;
        private readonly string _configFilePath;

        public object this[string key] => _config[key];

        public IDefaultsConfiguration Defaults => _defaultsConfiguration;

        internal Configuration()
        {
            _config = new Dictionary<string, object>();
            EnsureDefaults();
        }

        internal Configuration(IFileSystem fileSystem, string sitePath)
            : this()
        {
            _fileSystem = fileSystem;
            _configFilePath = _fileSystem.Path.Combine(sitePath, ConfigFileName);
        }

        private void EnsureDefaults()
        {
            if (!_config.ContainsKey("permalink"))
            {
                _config.Add("permalink", DefaultPermalink);
            }

            _defaultsConfiguration = new DefaultsConfiguration(_config);
        }

        internal void ReadFromFile()
        {
            _config = new Dictionary<string, object>();
            if (_fileSystem.File.Exists(_configFilePath))
            {
                _config = _fileSystem.File.ReadAllText(_configFilePath).ParseYaml();
                EnsureDefaults();
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
