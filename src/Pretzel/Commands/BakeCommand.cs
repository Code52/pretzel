using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Commands
{
    [Shared]
    [Export(typeof(IBakeCommandArguments))]
    [CommandArguments]
    public sealed class BakeCommandArguments : BakeBaseCommandArguments, IBakeCommandArguments
    {
        [ImportingConstructor]
        public BakeCommandArguments(IFileSystem fileSystem) : base(fileSystem) { }
    }

    [Shared]
    [CommandInfo(
        CommandName = BuiltInCommands.Bake,
        CommandDescription = "transforming content into a website",
        CommandArgumentsType = typeof(BakeCommandArguments)
        )]
    public sealed class BakeCommand : Command<BakeCommandArguments>
    {
        [Import]
        public TemplateEngineCollection TemplateEngines { get; set; }

        [Import]
        public SiteContextGenerator Generator { get; set; }

        [ImportMany]
        public IEnumerable<ITransform> Transforms { get; set; }
        
        [Import]
        public IFileSystem FileSystem { get; set; }

        protected override Task<int> Execute(BakeCommandArguments arguments)
        {
            Tracing.Info("bake - transforming content into a website");

            var siteContext = Generator.BuildContext(arguments.Source, arguments.Destination, arguments.Drafts);

            if (arguments.CleanTarget && FileSystem.Directory.Exists(siteContext.OutputFolder))
            {
                FileSystem.Directory.Delete(siteContext.OutputFolder, true);
            }

            if (string.IsNullOrWhiteSpace(arguments.Template))
            {
                arguments.DetectFromDirectory(TemplateEngines.Engines, siteContext);
            }

            var engine = TemplateEngines[arguments.Template];
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
                Tracing.Info("Cannot find engine for input: '{0}'", arguments.Template);
                return Task.FromResult(1);
            }
            return Task.FromResult(0);
        }
    }
}
