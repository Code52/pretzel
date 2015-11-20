﻿using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating.Context;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using Pretzel.Logic.Extensibility;
using System.Collections.Generic;

namespace Pretzel.Logic.Templating.Razor
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [SiteEngineInfo(Engine = "razor")]
    public class RazorSiteEngine : JekyllEngineBase
    {
        private static readonly string[] layoutExtensions = { ".cshtml" };

        private string includesPath;

        private List<ITag> _allTags = new List<ITag>();

        public override void Initialize()
        {
        }

        protected override void PreProcess()
        {
            includesPath = Path.Combine(Context.SourceFolder, "_includes");

            if (Tags != null)
            {
                _allTags.AddRange(Tags);
            }
            
            if (TagFactories != null)
            {
                _allTags.AddRange(TagFactories.Select(factory =>
                    {
                        factory.Initialize(Context);
                        return factory.CreateTag();
                    }));
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
                CachingProvider = new DefaultCachingProvider(t => { })
            };
            serviceConfiguration.Activator = new ExtensibleActivator(serviceConfiguration.Activator, Filters, _allTags);

            Engine.Razor = RazorEngineService.Create(serviceConfiguration);

            content = Regex.Replace(content, "<p>(@model .*?)</p>", "$1");

            try
            {
                return Engine.Razor.RunCompile(content, pageData.Page.File, typeof(PageContext), pageData);
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
