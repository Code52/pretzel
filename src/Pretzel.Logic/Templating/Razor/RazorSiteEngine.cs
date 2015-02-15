using Pretzel.Logic.Templating.Context;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Text.RegularExpressions;

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

        private static readonly string[] layoutExtensions = { ".cshtml" };
        protected override string[] LayoutExtensions
        {
            get { return layoutExtensions; }
        }

        protected override string RenderTemplate(string content, PageContext pageData)
        {
           var includesPath = Path.Combine(pageData.Site.SourceFolder, "_includes");
           var serviceConfig = new TemplateServiceConfiguration
                               {
                                  Resolver = new IncludesResolver(FileSystem, includesPath),
                                  BaseTemplateType = typeof (ExtensibleTemplate<>)
                               };
           serviceConfig.Activator = new ExtensibleActivator(serviceConfig.Activator, Filters);
           RazorEngine.Razor.SetTemplateService(new TemplateService(serviceConfig));

           content = Regex.Replace(content, "<p>(@model .*?)</p>", "$1");
            try
            {
                return RazorEngine.Razor.Parse(content, pageData);
            }
            catch (Exception)
            {
                Console.WriteLine(@"Failed to render template, falling back to direct content");
                return content;
            }
        }
    }
}
