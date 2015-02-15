using Pretzel.Logic.Templating.Context;
using RazorEngine;
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
        private static readonly string[] layoutExtensions = { ".cshtml" };

        public override void Initialize()
        {
        }

        protected override void PreProcess()
        {
        }

        protected override string[] LayoutExtensions
        {
            get { return layoutExtensions; }
        }

        protected override string RenderTemplate(string content, PageContext pageData)
        {
            var includesPath = Path.Combine(pageData.Site.SourceFolder, "_includes");
            var serviceConfig = new TemplateServiceConfiguration
                                {
                                    TemplateManager = new IncludesResolver(FileSystem, includesPath),
                                    BaseTemplateType = typeof(ExtensibleTemplate<>)
                                };
            serviceConfig.Activator = new ExtensibleActivator(serviceConfig.Activator, Filters);

            Engine.Razor = RazorEngineService.Create(serviceConfig);

            content = Regex.Replace(content, "<p>(@model .*?)</p>", "$1");
            try
            {
                return Engine.Razor.RunCompile(content, pageData.Page.File, typeof(PageContext), pageData);
            }
            catch (Exception)
            {
                Console.WriteLine(@"Failed to render template, falling back to direct content");
                return content;
            }
        }
    }
}
