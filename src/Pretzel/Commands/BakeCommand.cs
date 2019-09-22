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

    [Shared]
    [CommandInfo(CommandName = BuiltInCommands.Bake, CommandDescription = "transforming content into a website")]
    public sealed class BakeCommand : ICommand
    {
        [Import]
        public TemplateEngineCollection TemplateEngines { get; set; }

        [Import]
        public SiteContextGenerator Generator { get; set; }

        [ImportMany]
        public IEnumerable<ITransform> Transforms { get; set; }

        [Import]
        public BakeCommandParameters Parameters { get; set; }

        [Import]
        public IFileSystem FileSystem { get; set; }
        
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
                Parameters.DetectFromDirectory(TemplateEngines.Engines, siteContext);
            }

            var engine = TemplateEngines[Parameters.Template];
            if (engine != null)
            {
                var watch = new Stopwatch();
                watch.Start();
                engine.Initialize();
                engine.Process(siteContext);
                foreach (var t in Transforms)
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
