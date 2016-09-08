using Pretzel.Logic;
using Pretzel.Logic.Extensions;
using System.Collections.Generic;

namespace Pretzel.Tests
{
    public sealed class ConfigurationMock : IConfiguration
    {
        private IDictionary<string, object> _config;

        public object this[string key]
        {
            get
            {
                return _config[key];
            }
        }

        public ConfigurationMock()
        {
            _config = new Dictionary<string, object>();
            CheckDefaultConfig();
        }

        public ConfigurationMock(IDictionary<string, object> configContent)
        {
            _config = configContent;
            CheckDefaultConfig();
        }

        public ConfigurationMock(string configContent)
        {
            _config = configContent.ParseYaml();
            CheckDefaultConfig();
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

        public IDefaultsConfiguration Defaults
        {
            get { return new DefaultsConfigurationMock(); }
        }
    }

    internal class DefaultsConfigurationMock : IDefaultsConfiguration
    {
        public IDictionary<string, object> ForScope(string path)
        {
            return new Dictionary<string, object>();
        }
    }

}
