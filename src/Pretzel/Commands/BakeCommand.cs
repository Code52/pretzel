using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating;
using Pretzel.Logic.Templating.Context;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Composition;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pretzel.Commands
{
    [Shared]
    [Export]
    [CommandArguments(CommandName = BuiltInCommands.Bake)]
    public sealed class BakeCommandParameters : BakeBaseCommandParameters
    {
        [ImportingConstructor]
        public BakeCommandParameters(IFileSystem fileSystem) : base(fileSystem) { }
    }

    public abstract class PretzelBaseCommandParameters : BParameters
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
                new Option(new []{ "-t", "--template" },"The templating engine to use")
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
        // Default Option that get injected from Program
        public string Source { get; set; }
        // Default Option that get injected from Program
        public bool Debug { get; set; }
        // Default Option that get injected from Program
        public bool Safe { get; set; }


        public string Template { get; set; }
        public string Destination { get; set; }
        public bool Drafts { get; set; }
        public string Path => Source;

        public override void BindingCompleted()
        {
            if (string.IsNullOrEmpty(Destination))
            {
                Destination = "_site";
            }
            if (!fileSystem.Path.IsPathRooted(Destination))
            {
                Destination = fileSystem.Path.Combine(Path, Destination);
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

    public abstract class BakeBaseCommandParameters : PretzelBaseCommandParameters
    {
        protected BakeBaseCommandParameters(IFileSystem fileSystem) : base(fileSystem) { }

        protected override void WithOptions(List<Option> options)
        {
            options.AddRange(new[]
            {
                new Option(new [] { "-c", "--cleantarget" }, "Delete the target directory (_site by default)")
                {
                    Argument = new Argument<bool>()
                },
            });
        }

        public bool CleanTarget { get; set; }
    }

    [Shared]
    [CommandInfo(CommandName = BuiltInCommands.Bake, CommandDescription = "transforming content into a website")]
    public sealed class BakeCommand : ICommand
    {
#pragma warning disable 649

        [Import]
        public TemplateEngineCollection templateEngines { get; set; }

        [Import]
        public SiteContextGenerator Generator { get; set; }

        [ImportMany]
        public IEnumerable<ITransform> transforms { get; set; }

        [Import]
        public BakeCommandParameters Parameters { get; set; }

        [Import]
        public IFileSystem FileSystem { get; set; }

#pragma warning restore 649

        public async Task Execute()
        {
            Tracing.Info("bake - transforming content into a website");

            var siteContext = Generator.BuildContext(Parameters.Path, Parameters.Destination, Parameters.Drafts);

            if (Parameters.CleanTarget && FileSystem.Directory.Exists(siteContext.OutputFolder))
            {
                FileSystem.Directory.Delete(siteContext.OutputFolder, true);
            }

            if (string.IsNullOrWhiteSpace(Parameters.Template))
            {
                Parameters.DetectFromDirectory(templateEngines.Engines, siteContext);
            }

            var engine = templateEngines[Parameters.Template];
            if (engine != null)
            {
                var watch = new Stopwatch();
                watch.Start();
                engine.Initialize();
                engine.Process(siteContext);
                foreach (var t in transforms)
                    t.Transform(siteContext);

                engine.CompressSitemap(siteContext, FileSystem);

                watch.Stop();
                Tracing.Info("done - took {0}ms", watch.ElapsedMilliseconds);
            }
            else
            {
                Tracing.Info("Cannot find engine for input: '{0}'", Parameters.Template);
            }
        }
    }
}
