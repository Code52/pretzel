using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Minification;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Commands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [CommandInfo(CommandName = "bake")]
    public sealed class BakeCommand : ICommand
    {
#pragma warning disable 649
        [Import] TemplateEngineCollection templateEngines;
        [Import] SiteContextGenerator Generator { get; set; }
        [Import] CommandParameters parameters;
        [ImportMany] private IEnumerable<ITransform> transforms;
#pragma warning restore 649

        public void Execute(IEnumerable<string> arguments)
        {
            Tracing.Info("bake - transforming content into a website");

            parameters.Parse(arguments);

            if (string.IsNullOrWhiteSpace(parameters.Template))
            {
                parameters.DetectFromDirectory(templateEngines.Engines);
            }

            var engine = templateEngines[parameters.Template];
            if (engine != null)
            {
                var watch = new Stopwatch();
                watch.Start();
                engine.Initialize();
                var c = Generator.BuildContext(parameters.Path);
                engine.Process(c);
                foreach (var t in transforms)
                    t.Transform(c);
                watch.Stop();
                Tracing.Info(string.Format("done - took {0}ms", watch.ElapsedMilliseconds));
            }
            else
            {
                Tracing.Info(String.Format("Cannot find engine for input: '{0}'", parameters.Template));
            }
        }

        public void WriteHelp(TextWriter writer)
        {
            parameters.WriteOptions(writer, "-t", "-p");
        }
    }
}
