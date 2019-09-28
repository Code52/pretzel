using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensions;
using System;
using System.Collections.Generic;
using System.Composition;
using System.IO.Abstractions;

namespace Pretzel.Logic
{
    public interface ISourcePathProvider
    {
        string Source { get; }
    }

    public interface IConfiguration
    {
        object this[string key] { get; }

        bool ContainsKey(string key);

        bool TryGetValue(string key, out object value);

        IDictionary<string, object> ToDictionary();

        IDefaultsConfiguration Defaults { get; }

        void ReadFromFile(string path);
    }

    [Shared]
    [Export(typeof(IConfiguration))]
    internal sealed class Configuration : IConfiguration
    {
        private const string ConfigFileName = "_config.yml";
        public const string DefaultPermalink = "date";

        private IDictionary<string, object> _config;
        private readonly IFileSystem _fileSystem;
        public object this[string key] => _config[key];

        public IDefaultsConfiguration Defaults { get; private set; }
        
        internal Configuration()
        {
            _config = new Dictionary<string, object>();
            EnsureDefaults();
        }

        [ImportingConstructor]
        public Configuration(IFileSystem fileSystem)
            : this()
        {
            _fileSystem = fileSystem;
        }

        private void EnsureDefaults()
        {
            if (!_config.ContainsKey("permalink"))
            {
                _config.Add("permalink", DefaultPermalink);
            }

            Defaults = new DefaultsConfiguration(_config);
        }

        public void ReadFromFile(string path)
        {
            var configFilePath = _fileSystem.Path.Combine(path, ConfigFileName);
            _config = new Dictionary<string, object>();
            if (_fileSystem.File.Exists(configFilePath))
            {
                _config = _fileSystem.File.ReadAllText(configFilePath).ParseYaml();
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
