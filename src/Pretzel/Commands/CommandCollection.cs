using System;
using System.CommandLine;
using System.Composition;
using System.Linq;
using Pretzel.Logic;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensibility;

namespace Pretzel.Commands
{
    [Export]
    [Shared]
    public sealed class CommandCollection
    {
        ExportFactory<IPretzelCommand, CommandInfoAttribute>[] commands;
        [ImportMany]
        public ExportFactory<IPretzelCommand, CommandInfoAttribute>[] Commands
        {
            get => commands ?? new ExportFactory<IPretzelCommand, CommandInfoAttribute>[] { };
            set => commands = value;
        }

        ExportFactory<ICommandArguments, CommandArgumentsAttribute>[] commandArguments;
        [ImportMany]
        public ExportFactory<ICommandArguments, CommandArgumentsAttribute>[] CommandArguments
        {
            get => commandArguments ?? new ExportFactory<ICommandArguments, CommandArgumentsAttribute>[] { };
            set => commandArguments = value;
        }

        ExportFactory<ICommandArgumentsExtension, CommandArgumentsExtensionAttribute>[] argumentExtensions;
        [ImportMany]
        public ExportFactory<ICommandArgumentsExtension, CommandArgumentsExtensionAttribute>[] ArgumentExtensions
        {
            get
            {
                return argumentExtensions ?? new ExportFactory<ICommandArgumentsExtension, CommandArgumentsExtensionAttribute>[] { };
            }
            set
            {
                argumentExtensions = value;
            }
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

                    foreach (var argumentExtensionsExport in ArgumentExtensions.Where(a => a.Metadata.CommandNames.Contains(commandArgumentsExport.Metadata.CommandName)))
                    {
                        var arugumentExtension = argumentExtensionsExport.CreateExport().Value;
                        args.Extensions.Add(arugumentExtension);
                    }

                    foreach(var arugumentExtension in args.Extensions)
                    {
                        arugumentExtension.UpdateOptions(args.Options);
                    }

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
