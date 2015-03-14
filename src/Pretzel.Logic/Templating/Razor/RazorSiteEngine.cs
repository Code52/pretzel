using Pretzel.Logic.Extensions;
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

        private string includesPath;

        public override void Initialize()
        {
        }

        protected override void PreProcess()
        {
            includesPath = Path.Combine(Context.SourceFolder, "_includes");
        }

        protected override string[] LayoutExtensions
        {
            get { return layoutExtensions; }
        }

        protected override string RenderTemplate(string content, PageContext pageData)
        {
            var serviceConfiguration = new TemplateServiceConfiguration
            {
                TemplateManager = new IncludesResolver(FileSystem, includesPath),
                BaseTemplateType = typeof(ExtensibleTemplate<>)
            };
            serviceConfiguration.Activator = new ExtensibleActivator(serviceConfiguration.Activator, Filters, Tags);
            Engine.Razor = RazorEngineService.Create(serviceConfiguration);

            content = Regex.Replace(content, "<p>(@model .*?)</p>", "$1");

            try
            {
                return Engine.Razor.RunCompile(content, pageData.Page.File, typeof(PageContext), pageData);
            }
            catch (Exception ex)
            {
                Tracing.Debug(ex.Message + Environment.NewLine + ex.StackTrace);
                Console.WriteLine(@"Failed to render template, falling back to direct content");
                return content;
            }
        }
    }
}
