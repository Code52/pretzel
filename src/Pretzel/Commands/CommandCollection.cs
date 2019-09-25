using System;
using System.CommandLine;
using System.Composition;
using System.Linq;
using Pretzel.Logic;
using Pretzel.Logic.Commands;

namespace Pretzel.Commands
{
    [Export]
    [Shared]
    public sealed class CommandCollection
    {
        ExportFactory<ICommand, CommandInfoAttribute>[] commands;
        [ImportMany]
        public ExportFactory<ICommand, CommandInfoAttribute>[] Commands
        {
            get => commands ?? new ExportFactory<ICommand, CommandInfoAttribute>[] { };
            set => commands = value;
        }

        ExportFactory<ICommandParameters, CommandArgumentsAttribute>[] commandArguments;
        [ImportMany]
        public ExportFactory<ICommandParameters, CommandArgumentsAttribute>[] CommandArguments
        {
            get => commandArguments ?? new ExportFactory<ICommandParameters, CommandArgumentsAttribute>[] { };
            set => commandArguments = value;
        }

        [Export]
        public RootCommand RootCommand { get; set; }

        [Import]
        public IConfiguration Configuration { get; set; }

        [OnImportsSatisfied]
        internal void OnImportsSatisfied()
        {
            RootCommand = new RootCommand();

            foreach (var command in Commands)
            {
                var subCommand = new Command(command.Metadata.CommandName, command.Metadata.CommandDescription);

                foreach (var commandArgumentsExport in CommandArguments?.Where(a => a.Metadata.CommandName == command.Metadata.CommandName))
                {
                    var args = commandArgumentsExport.CreateExport().Value;

                    foreach (var option in args.Options)
                    {
                        subCommand.AddOption(option);
                    }

                    subCommand.Handler = new PretzelCommandHandler(Configuration, args, command);
                }

                if (subCommand.Handler == null)
                {
                    subCommand.Handler = new PretzelCommandHandler(Configuration, null, command);
                }

                RootCommand.AddCommand(subCommand);
            }
        }
    }
}
