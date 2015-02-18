using NDesk.Options;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating;
using Pretzel.Logic.Templating.Context;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

namespace Pretzel.Logic.Commands
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class CommandParameters
    {
        [ImportingConstructor]
        public CommandParameters([ImportMany] IEnumerable<IHaveCommandLineArgs> commandLineExtensions)
        {
            port = 8080;
            LaunchBrowser = true;

            Settings = new OptionSet
                {
                    { "t|template=", "The templating engine to use", v => Template = v },
                    { "d|directory=", "[Obsolete, use --source instead] The path to site directory", p => Path = p },
                    { "p|port=", "The port to test the site locally", p => decimal.TryParse(p, out port) },
                    { "i|import=", "The import type", v => ImportType = v },
                    { "f|file=", "Path to import file", v => ImportPath = v },
                    { "s|source=", "The path to the source site (default current directory)", p => Path = p},
                    { "destination=", "The path to the destination site (default _site)", d => DestinationPath = d},
                    { "drafts", "Add the posts in the drafts folder", v => IncludeDrafts = true },
                    { "nobrowser", "Do not launch a browser", v => LaunchBrowser = false },
                    { "withproject", "Includes a layout VS Solution, to give intellisense when editing razor layout files", v => WithProject = (v!=null) },
                    { "wiki", "Creates a wiki instead of a blog (razor template only)", v => Wiki = (v!=null) },
                    { "cleantarget", "Delete the target directory (_site by default)", v => CleanTarget = true },
                    { "safe", "Disable custom plugins", v => Safe = true }
                };

            // Allow extensions to register command line args
            foreach (var commandLineExtension in commandLineExtensions)
            {
                commandLineExtension.UpdateOptions(Settings);
            }
        }

        public string Path { get; private set; }

        public string Template { get; private set; }

        public string ImportPath { get; private set; }

        public string ImportType { get; private set; }

        public bool WithProject { get; private set; }

        public bool Wiki { get; private set; }

        public bool IncludeDrafts { get; private set; }

        public bool CleanTarget { get; private set; }

        public bool LaunchBrowser { get; private set; }

        public bool Safe { get; private set; }

        public string DestinationPath { get; private set; }

        private decimal port;

        public decimal Port
        {
            get { return port; }
        }

        private OptionSet Settings { get; set; }

        public void Parse(IEnumerable<string> arguments)
        {
            var argumentList = arguments.ToArray();

            Settings.Parse(argumentList);

            var firstArgument = argumentList.FirstOrDefault();

            if (firstArgument != null && !firstArgument.StartsWith("-") && !firstArgument.StartsWith("/"))
            {
                Path = System.IO.Path.IsPathRooted(firstArgument)
                    ? firstArgument
                    : System.IO.Path.Combine(Directory.GetCurrentDirectory(), firstArgument);
            }

            Path = string.IsNullOrWhiteSpace(Path) ? Directory.GetCurrentDirectory() : System.IO.Path.GetFullPath(Path);
        }

        public void DetectFromDirectory(IDictionary<string, ISiteEngine> engines, SiteContext context)
        {
            foreach (var engine in engines)
            {
                if (!engine.Value.CanProcess(context)) continue;

                Tracing.Info(String.Format("Recommended engine for directory: '{0}'", engine.Key));
                Template = engine.Key;
                return;
            }

            if (Template == null)
                Template = "liquid";
        }

        public void WriteOptions(TextWriter writer, params string[] args)
        {
            if (args.Length == 0)
                Settings.WriteOptionDescriptions(writer);
            else
                WriteSubset(writer, args);
        }

        private void WriteSubset(TextWriter writer, string[] args)
        {
            var textWriter = new StringWriter();
            Settings.WriteOptionDescriptions(textWriter);
            var output = textWriter.ToString();

            var strings = RecombineMultilineArgs(output.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));

            foreach (var line in strings)
            {
                if (args.Any(line.Contains))
                {
                    writer.WriteLine(line);
                }
            }
        }

        private IEnumerable<string> RecombineMultilineArgs(string[] split)
        {
            for (int i = 0; i < split.Length; i++)
            {
                if (i + 1 < split.Length && !split[i + 1].TrimStart().StartsWith("-"))
                {
                    yield return split[i] + "\r\n" + split[i + 1];
                    i++;
                }
                else
                {
                    yield return split[i];
                }
            }
        }
    }
}
