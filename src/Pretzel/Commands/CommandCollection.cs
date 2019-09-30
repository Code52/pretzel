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
        ExportFactory<Logic.Commands.ICommand, CommandInfoAttribute>[] commands;
        [ImportMany]
        public ExportFactory<Logic.Commands.ICommand, CommandInfoAttribute>[] Commands
        {
            get => commands ?? Array.Empty<ExportFactory<Logic.Commands.ICommand, CommandInfoAttribute>>();
            set => commands = value;
        }

        ICommandArguments[] commandArguments;
        [ImportMany]
        public ICommandArguments[] CommandArguments
        {
            get => commandArguments ?? Array.Empty<ICommandArguments>();
            set => commandArguments = value;
        }

        ExportFactory<ICommandArgumentsExtension, CommandArgumentsExtensionAttribute>[] argumentExtensions;
        [ImportMany]
        public ExportFactory<ICommandArgumentsExtension, CommandArgumentsExtensionAttribute>[] ArgumentExtensions
        {
            get => argumentExtensions ?? Array.Empty<ExportFactory<ICommandArgumentsExtension, CommandArgumentsExtensionAttribute>>();
            set => argumentExtensions = value;
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

                var argument = CommandArguments.First(c => c.GetType() == command.Metadata.CommandArgumentsType);

                if (argument is BaseCommandArguments baseCommandArguments)
                {
                    baseCommandArguments.BuildOptions();
                }

                foreach (var argumentExtensionsExport in ArgumentExtensions.Where(a => a.Metadata.CommandNames.Contains(command.Metadata.CommandName)))
                {
                    var arugumentExtension = argumentExtensionsExport.CreateExport().Value;
                    argument.Extensions.Add(arugumentExtension);
                }

                foreach (var arugumentExtension in argument.Extensions)
                {
                    foreach (var option in arugumentExtension.Options)
                        argument.Options.Add(option);
                }

                foreach (var option in argument.Options)
                {
                    subCommand.AddOption(option);
                }

                subCommand.Handler = new PretzelCommandHandler(Configuration, argument, command);

                RootCommand.AddCommand(subCommand);
            }
        }
    }
}
