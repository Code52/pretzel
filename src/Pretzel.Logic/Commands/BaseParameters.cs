using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Composition;
using System.IO.Abstractions;
using System.Linq;

namespace Pretzel.Logic.Commands
{
    public sealed class BaseParameters
    {
        [Export]
        public IEnumerable<Option> Options { get; }

        public string CommandName { get; }

        public bool Help { get; private set; }

        public bool Debug { get; private set; }

        public bool Safe { get; private set; }

        public string Path { get; private set; }

        public IFileSystem FileSystem { get; private set; }

        public List<string> CommandArgs { get; private set; }

        private BaseParameters(string[] arguments, IFileSystem fileSystem)
        {
            Options = new []
            {
                new Option("debug", "Enable debugging")
                {
                    Argument = new Argument<bool>()
                },
                new Option("safe", "Disable custom plugins")
                {
                    Argument = new Argument<bool>()
                },
                new Option(new [] { "d", "directory"}, "[Obsolete, use --source instead] The path to site directory")
                {
                    Argument = new Argument<string>()
                },
                new Option(new [] { "s", "source"}, "The path to the source site (default current directory)")
                {
                    Argument = new Argument<string>()
                }
            };

            FileSystem = fileSystem;

            CommandName = arguments.Take(1).FirstOrDefault();

            //CommandArgs = Options.Parse(arguments.Skip(1));

            SetPath(CommandArgs.FirstOrDefault());
        }

        public static BaseParameters Parse(string[] arguments, IFileSystem fileSystem)
        {
            return new BaseParameters(arguments, fileSystem);
        }

        private void SetPath(string firstArgument)
        {
            // take the first argument after the command
            if (firstArgument != null && !firstArgument.StartsWith("-") && !firstArgument.StartsWith("/"))
            {
                Path = FileSystem.Path.IsPathRooted(firstArgument)
                    ? firstArgument
                    : FileSystem.Path.Combine(FileSystem.Directory.GetCurrentDirectory(), firstArgument);
            }
            Path = string.IsNullOrWhiteSpace(Path)
                ? FileSystem.Directory.GetCurrentDirectory()
                : FileSystem.Path.GetFullPath(Path);
        }
    }
}
