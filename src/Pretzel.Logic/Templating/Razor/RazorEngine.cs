using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Abstractions;
using DotLiquid;
using MarkdownDeep;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Logic.Templating.Razor
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [SiteEngineInfo(Engine = "razor")]
    public class RazorEngine : ISiteEngine
    {
        bool ISiteEngine.CanProcess(string directory)
        {
            throw new System.NotImplementedException();
        }

        void ISiteEngine.Initialize()
        {
            throw new System.NotImplementedException();
        }

        void ISiteEngine.Process(Jekyll.SiteContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
