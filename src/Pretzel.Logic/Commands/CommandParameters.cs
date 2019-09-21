using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating;
using Pretzel.Logic.Templating.Context;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Composition;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace Pretzel.Logic.Commands
{
    [Export]
    [Shared]
    public class CommandParameters
    {
        [ImportingConstructor]
        public CommandParameters([ImportMany] IEnumerable<IHaveCommandLineArgs> commandLineExtensions, IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;

            Settings = new List<Option>
            {

            };

            // Allow extensions to register command line args
            foreach (var commandLineExtension in commandLineExtensions)
            {
                commandLineExtension.UpdateOptions(Settings);
            }
        }

        [Import("SourcePath")]
        public string Path { get; set; }

        public string Template { get; private set; }

        public string ImportPath { get; private set; }

        public string ImportType { get; private set; }

        public bool WithProject { get; private set; }

        public bool Wiki { get; private set; }

        public bool IncludeDrafts { get; private set; }

        public bool CleanTarget { get; private set; }

        public bool LaunchBrowser { get; private set; }

        public string DestinationPath { get; private set; }

        public string NewPostTitle { get; internal set; }

        private decimal port;

        public decimal Port
        {
            get { return port; }
        }

        [Export]
        private List<Option> Settings { get; set; }

        private readonly IFileSystem fileSystem;

        public void Parse(IEnumerable<string> arguments)
        {
            var argumentList = arguments.ToArray();

            //Settings.Parse(argumentList);

            if (string.IsNullOrEmpty(DestinationPath))
            {
                DestinationPath = "_site";
            }
            if (!fileSystem.Path.IsPathRooted(DestinationPath))
            {
                DestinationPath = fileSystem.Path.Combine(Path, DestinationPath);
            }
            if (string.IsNullOrEmpty(NewPostTitle))
            {
                NewPostTitle = "New post";
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

        public void WriteOptions(TextWriter writer, params string[] args)
        {
            //if (args.Length == 0)
            //    Settings.WriteOptionDescriptions(writer);
            //else
            //    WriteSubset(writer, args);
        }

        private void WriteSubset(TextWriter writer, string[] args)
        {
            var textWriter = new StringWriter();
            //Settings.WriteOptionDescriptions(textWriter);
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
