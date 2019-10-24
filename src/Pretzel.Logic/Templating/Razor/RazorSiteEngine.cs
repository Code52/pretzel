using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating.Context;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using Pretzel.Logic.Extensibility;
using System.Collections.Generic;
using System.Composition;
using Microsoft.AspNetCore.Mvc.Razor.Extensions;

namespace Pretzel.Logic.Templating.Razor
{
    [Shared]
    [SiteEngineInfo(Engine = "razor")]
    public class RazorSiteEngine : JekyllEngineBase
    {
        private static readonly string[] layoutExtensions = { ".cshtml" };

        private string includesPath;

        private readonly List<ITag> _allTags = new List<ITag>();

        public override void Initialize()
        {
        }

        private class TagComparer : IEqualityComparer<ITag>
        {
            public bool Equals(ITag x, ITag y)
            {
                if (x == null || y == null)
                {
                    return false;
                }

                return x.Name == y.Name;
            }

            public int GetHashCode(ITag obj)
            {
                return obj.Name.GetHashCode();
            }
        }

        protected override void PreProcess()
        {
            includesPath = Path.Combine(Context.SourceFolder, "_includes");

            if (Tags != null)
            {
                var toAdd = Tags.Except(_allTags, new TagComparer()).ToList();
                _allTags.AddRange(toAdd);
            }

            if (TagFactories != null)
            {
                var toAdd = TagFactories.Select(factory =>
                {
                    factory.Initialize(Context);
                    return factory.CreateTag();
                }).Except(_allTags, new TagComparer()).ToList();

                _allTags.AddRange(toAdd);
            }
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
                BaseTemplateType = typeof(ExtensibleTemplate<>),
                DisableTempFileLocking = true,
                CachingProvider = new DefaultCachingProvider(t => { }),
                ConfigureCompilerBuilder = builder => ModelDirective.Register(builder)
            };
            serviceConfiguration.Activator = new ExtensibleActivator(serviceConfiguration.Activator, Filters, _allTags);

            Engine.Razor = RazorEngineService.Create(serviceConfiguration);

            content = Regex.Replace(content, "<p>(@model .*?)</p>", "$1");

            var pageContent = pageData.Content;
            pageData.Content = pageData.FullContent;

            try
            {
                content = Engine.Razor.RunCompile(content, pageData.Page.File, typeof(PageContext), pageData);
                pageData.Content = pageContent;
                return content;
            }
            catch (Exception e)
            {
                Tracing.Error(@"Failed to render template, falling back to direct content");
                Tracing.Debug(e.Message);
                Tracing.Debug(e.StackTrace);
                return content;
            }
        }
    }
}
