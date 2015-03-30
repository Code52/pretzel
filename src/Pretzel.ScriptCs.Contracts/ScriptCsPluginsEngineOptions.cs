using System;

namespace Pretzel.ScriptCs.Contracts
{
    public sealed class ScriptCsPluginsEngineOptions
    {
        public string PluginsFolderPath { get; set; }

        public Type[] References { get; set; }
    }
}
