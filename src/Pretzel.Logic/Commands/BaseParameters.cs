using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Composition;
using System.IO.Abstractions;
using System.Linq;

namespace Pretzel.Logic.Commands
{

    [Export]
    public sealed class BaseParameters
    {
        public OptionSet Options { get; private set; }

        public string CommandName { get; private set; }

        public bool Help { get; private set; }

        public bool Debug { get; private set; }

        public bool Safe { get; private set; }

        [Export]
        public SourcePathProvider PathProvider { get; private set; }

        public IFileSystem FileSystem { get; private set; }

        public List<string> CommandArgs { get; private set; }

        private BaseParameters(string[] arguments, IFileSystem fileSystem)
        {
            SetDefaults(arguments, fileSystem);
        }

        [ImportingConstructor]
        public BaseParameters(IFileSystem fileSystem) : this(Environment.GetCommandLineArgs().Skip(1).ToArray(), fileSystem) { }

        private void SetDefaults(string[] arguments, IFileSystem fileSystem)
        {
            Options = new OptionSet
                {
                    { "help", "Display help mode", p => Help = true },
                    { "debug", "Enable debugging", p => Debug = true },
                    { "safe", "Disable custom plugins", v => Safe = true },
                    { "d|directory=", "[Obsolete, use --source instead] The path to site directory", p => PathProvider = new SourcePathProvider(p) },
                    { "s|source=", "The path to the source site (default current directory)", p => PathProvider = new SourcePathProvider(p) }
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
                PathProvider = new SourcePathProvider(FileSystem.Path.IsPathRooted(firstArgument)
                    ? firstArgument
                    : FileSystem.Path.Combine(FileSystem.Directory.GetCurrentDirectory(), firstArgument));
            }
            PathProvider = new SourcePathProvider((PathProvider == null || string.IsNullOrWhiteSpace(PathProvider.Path))
                ? FileSystem.Directory.GetCurrentDirectory()
                : FileSystem.Path.GetFullPath(PathProvider.Path));
        }
    }
}
