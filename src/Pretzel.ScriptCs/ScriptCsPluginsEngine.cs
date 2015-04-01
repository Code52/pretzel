using ScriptCs.ComponentModel.Composition;
using System;
using System.ComponentModel.Composition.Primitives;

namespace Pretzel.ScriptCs
{
    public sealed class ScriptCsCatalogFactory
    {
        public static ComposablePartCatalog CreateScriptCsCatalog(string pluginsFolderPath, Type[] references)
        {
            return new ScriptCsCatalog(pluginsFolderPath, new ScriptCsCatalogOptions { References = references });
        }
    }
}
