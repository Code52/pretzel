using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating.Context;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;

namespace Pretzel.Commands
{
    [Shared]
    [CommandInfo(CommandName = "bake")]
    public sealed class BakeCommand : ICommand
    {
#pragma warning disable 649

        [Import]
        public TemplateEngineCollection templateEngines { get; set; }

        [Import]
        public SiteContextGenerator Generator { get; set; }

        [Import]
        public CommandParameters parameters { get; set; }

        [ImportMany]
        public IEnumerable<ITransform> transforms { get; set; }

        [Import]
        public IFileSystem FileSystem { get; set; }

#pragma warning restore 649

        public void Execute(IEnumerable<string> arguments)
        {
            Tracing.Info("bake - transforming content into a website");

            parameters.Parse(arguments);

            var siteContext = Generator.BuildContext(parameters.Path, parameters.DestinationPath, parameters.IncludeDrafts);

            if (parameters.CleanTarget && FileSystem.Directory.Exists(siteContext.OutputFolder))
            {
                FileSystem.Directory.Delete(siteContext.OutputFolder, true);
            }

            if (string.IsNullOrWhiteSpace(parameters.Template))
            {
                parameters.DetectFromDirectory(templateEngines.Engines, siteContext);
            }

            var engine = templateEngines[parameters.Template];
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
                Tracing.Info("Cannot find engine for input: '{0}'", parameters.Template);
            }
        }

        public void WriteHelp(TextWriter writer)
        {
            parameters.WriteOptions(writer, "-t", "-p", "-d", "-cleantarget", "-s", "-destination");
        }
    }
}
