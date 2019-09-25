using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO.Abstractions;
using System.Linq;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Logic.Commands
{
    public abstract class PretzelBaseCommandParameters : BaseParameters, IPathProvider
    {
        protected readonly IFileSystem fileSystem;

        protected PretzelBaseCommandParameters(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        protected override void WithOptions(List<Option> options)
        {
            options.AddRange(new[]
            {
                new Option(new []{ "-t", "--template" }, "The templating engine to use")
                {
                    Argument = new Argument<string>()
                },
                new Option(new [] { "-d", "--destination" }, "The path to the destination site (default _site)")
                {
                    Argument = new Argument<string>(() => "_site")
                },
                new Option("--drafts", "Add the posts in the drafts folder")
                {
                    Argument = new Argument<bool>()
                },
            });
        }

        // Default Option that get's injected from Program
        public string Source { get; set; }
        // Default Option that get's injected from Program
        public bool Debug { get; set; }
        // Default Option that get's injected from Program
        public bool Safe { get; set; }
        [Obsolete("Use '" + nameof(Source) + "' instead.")]
        public string Path => Source;

        public string Template { get; set; }
        public string Destination { get; set; }
        public bool Drafts { get; set; }

        public override void BindingCompleted()
        {
            Source = string.IsNullOrWhiteSpace(Source)
                ? fileSystem.Directory.GetCurrentDirectory()
                : fileSystem.Path.GetFullPath(Source);

            if (string.IsNullOrEmpty(Destination))
            {
                Destination = "_site";
            }
            if (!fileSystem.Path.IsPathRooted(Destination))
            {
                Destination = fileSystem.Path.Combine(Source, Destination);
            }
        }

        public void DetectFromDirectory(IDictionary<string, ISiteEngine> engines, SiteContext context)
        {
            foreach (var engine in engines)
            {
                if (!engine.Value.CanProcess(context)) continue;
                Tracing.Info("Recommended engine for directory: '{0}'", engine.Key);
                Template = engine.Key;
                return;
            }

            if (Template == null)
                Template = "liquid";
        }
    }
}
