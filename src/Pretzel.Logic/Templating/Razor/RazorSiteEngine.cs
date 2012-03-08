using System.ComponentModel.Composition;
using System.IO;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Logic.Templating.Razor
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [SiteEngineInfo(Engine = "razor")]
    public class RazorSiteEngine : ISiteEngine
    {
        public bool CanProcess(string directory)
        {
            return false;
        }

        public void Initialize()
        {
            
        }

        public void Process(SiteContext context)
        {
        }

        public string GetOutputDirectory(string path)
        {
            return Path.Combine(path, "_site");
        }
    }
}
