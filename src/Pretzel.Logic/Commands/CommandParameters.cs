using NDesk.Options;
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

            Settings = new[]
            {
                new Option(new []{ "template", "t" },"The templating engine to use")
                {
                    Argument = new Argument<string>()
                },
                new Option(new [] {"port", "p"}, "The port to test the site locally")
                {
                    Argument = new Argument<int>(() => 8080)
                },
                new Option(new [] {"import", "i"}, "The import type")
                {
                    Argument = new Argument<string>()
                },
                new Option(new [] {"file", "f"}, "Path to import file")
                {
                    Argument = new Argument<string>()
                },
                new Option("destination", "The path to the destination site (default _site)")
                {
                    Argument = new Argument<string>(() => "_site")
                },
                new Option("drafts", "Add the posts in the drafts folder")
                {
                    Argument = new Argument<bool>()
                },
                new Option("nobrowser", "Do not launch a browser (false by default)")
                {
                    Argument = new Argument<bool>(() => false)
                },
                new Option("withproject", "Includes a layout VS Solution, to give intellisense when editing razor layout files")
                {
                    Argument = new Argument<bool>()
                },
                new Option("wiki", "Creates a wiki instead of a blog (razor template only)")
                {
                    Argument = new Argument<bool>()
                },
                new Option("cleantarget", "Delete the target directory (_site by default)")
                {
                    Argument = new Argument<bool>()
                },
                new Option(new [] { "newposttitle", "n" }, "The title of the new post (\"New post\" by default")
                {
                    Argument = new Argument<string>(() => "New post")
                }
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
        private IEnumerable<Option> Settings { get; set; }

        private readonly IFileSystem fileSystem;

        public void Parse(IEnumerable<string> arguments)
        {
            var argumentList = arguments.ToArray();

            Settings.Parse(argumentList);

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
