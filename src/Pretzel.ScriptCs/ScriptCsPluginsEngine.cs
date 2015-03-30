using Pretzel.ScriptCs.Contracts;
using ScriptCs.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;

namespace Pretzel.ScriptCs
{
    public sealed class ScriptCsPluginsEngine
    {
        public static ComposablePartCatalog GetScriptCsCatalog(ScriptCsPluginsEngineOptions options)
        {
            return new ScriptCsCatalog(options.PluginsFolderPath, new ScriptCsCatalogOptions { References = options.References });
        }
    }
}
