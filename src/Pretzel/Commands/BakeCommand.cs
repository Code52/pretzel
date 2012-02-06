using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Commands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [CommandInfo(CommandName = "bake")]
    public sealed class BakeCommand : ICommand
    {
#pragma warning disable 649
        [Import] TemplateEngineCollection templateEngines;
        [Import] CommandParameters parameters;
#pragma warning restore 649

        public void Execute(IEnumerable<string> arguments)
        {
            Tracing.Info("bake - transforming content into a website");

            parameters.Parse(arguments);

            if (string.IsNullOrWhiteSpace(parameters.Template))
            {
                parameters.Template = DetectEngineFromPath(parameters.Path);
            }

            var engine = templateEngines[parameters.Template];
            if (engine != null)
            {
                var watch = new Stopwatch();
                watch.Start();
                var context = new SiteContext { Folder = parameters.Path };
                engine.Initialize();
                engine.Process(context);
                watch.Stop();
                Tracing.Info(string.Format("done - took {0}ms", watch.ElapsedMilliseconds));
            }
            else
            {
                Tracing.Info(String.Format("Cannot find engine for input: '{0}'", parameters.Template));
            }
        }

        private string DetectEngineFromPath(string path)
        {
            foreach (var engine in templateEngines.Engines)
            {
                if (!engine.Value.CanProcess(path)) continue;
                Tracing.Info(String.Format("Recommended engine for directory: '{0}'", engine.Key));
                return engine.Key;
            }

            return string.Empty;
        }

        public void WriteHelp(TextWriter writer)
        {
            parameters.WriteOptions(writer); // TODO: output relevant messages (not all of them)
        }
    }
}
