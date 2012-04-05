using System.ComponentModel.Composition;
using System.IO;
using System.Text.RegularExpressions;
using Pretzel.Logic.Templating.Context;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace Pretzel.Logic.Templating.Razor
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [SiteEngineInfo(Engine = "razor")]
    public class RazorSiteEngine : JekyllEngineBase
    {
        public override void Initialize()
        {
        }

        protected override void PreProcess()
        {
        }

        protected override string LayoutExtension
        {
            get { return ".cshtml"; }
        }

        protected override string RenderTemplate(string content, PageContext pageData)
        {
           var includesPath = Path.Combine(pageData.Site.SourceFolder, "_includes");
           var serviceConfig = new TemplateServiceConfiguration { Resolver = new IncludesResolver(FileSystem, includesPath) };
           RazorEngine.Razor.SetTemplateService(new TemplateService(serviceConfig));

           content = Regex.Replace(content, "<p>(@model .*?)</p>", "$1");

           return RazorEngine.Razor.Parse(content, pageData);
        }
    }
}
