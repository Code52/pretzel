using NDesk.Options;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO.Abstractions;
using System.Linq;

namespace Pretzel.Logic.Commands
{
    public sealed class BaseParameters
    {
        public OptionSet Options { get; private set; }

        public string CommandName { get; private set; }

        public bool Help { get; private set; }

        public bool Debug { get; private set; }

        public bool Safe { get; private set; }

        [Export("SourcePath")]
        public string Path { get; private set; }

        [Export]
        public IFileSystem FileSystem { get; private set; }

        public List<string> CommandArgs { get; private set; }

        private BaseParameters(string[] arguments, IFileSystem fileSystem)
        {
            Options = new OptionSet
                {
                    {"help", "Display help mode", p => Help = true},
                    {"debug", "Enable debugging", p => Debug = true},
                    { "safe", "Disable custom plugins", v => Safe = true },
                    { "d|directory=", "[Obsolete, use --source instead] The path to site directory", p => Path = p },
                    { "s|source=", "The path to the source site (default current directory)", p => Path = p}
                };

            FileSystem = fileSystem;

            CommandName = arguments.Take(1).FirstOrDefault();

            CommandArgs = Options.Parse(arguments.Skip(1));

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
            Path = string.IsNullOrWhiteSpace(Path) ? FileSystem.Directory.GetCurrentDirectory() : FileSystem.Path.GetFullPath(Path);
        }
    }
}
