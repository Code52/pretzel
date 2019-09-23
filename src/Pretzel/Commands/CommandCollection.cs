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
        [ImportMany]
        public ExportFactory<ICommand, CommandInfoAttribute>[] Commands { get; set; }

        [ImportMany]
        public ExportFactory<ICommandParameters, CommandArgumentsAttribute>[] CommandArguments { get; set; }

        [Export]
        public RootCommand RootCommand { get; set; }

        [Import]
        public IConfiguration Configuration { get; set; }

        [OnImportsSatisfied]
        public void OnImportsSatisfied()
        {
            RootCommand = new RootCommand();

            foreach (var command in Commands)
            {
                var subCommand = new Command(command.Metadata.CommandName, command.Metadata.CommandDescription);

                foreach (var commandArgumentsExport in CommandArguments.Where(a => a.Metadata.CommandName == command.Metadata.CommandName))
                {
                    var commandArguments = commandArgumentsExport.CreateExport().Value;

                    foreach (var option in commandArguments.Options)
                    {
                        subCommand.AddOption(option);
                    }

                    subCommand.Handler = new PretzelCommandHandler(Configuration, commandArguments, command);
                }

                if(subCommand.Handler == null)
                {
                    subCommand.Handler = new PretzelCommandHandler(Configuration, null, command);
                }

                RootCommand.AddCommand(subCommand);
            }
        }
    }
}
