using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Binding;
using System.Composition;
using System.Dynamic;
using System.Linq;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensibility;
using System.Threading.Tasks;
using System.Reflection;

namespace Pretzel.Commands
{

    public class PretzelCommandHandler : ICommandHandler
    {
        private ICommandParameters commandParameters;
        private ExportFactory<ICommand, CommandInfoAttribute> command;

        public PretzelCommandHandler(ICommandParameters commandArguments, ExportFactory<ICommand, CommandInfoAttribute> command)
        {
            this.commandParameters = commandArguments;
            this.command = command;
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var bindingContext = context.BindingContext;

            new ModelBinder(commandParameters.GetType())
                .UpdateInstance(commandParameters, bindingContext);

            commandParameters.BindingCompleted();

            if (commandParameters is ICommandParametersExtendable commandParametersExtendable)
            {
                foreach (var factory in commandParametersExtendable.GetCommandExtentions())
                {
                    var extentedArguments = factory.CreateExport().Value;

                    new ModelBinder(extentedArguments.GetType())
                        .UpdateInstance(extentedArguments, bindingContext);

                    extentedArguments.BindingCompleted();
                }
            }

            await command.CreateExport().Value.Execute();

            return 0;
        }
    }

    [Export]
    [Shared]
    public sealed class CommandCollection
    {
        [ImportMany]
        public ExportFactory<ICommand, CommandInfoAttribute>[] Commands { get; set; }

        [ImportMany]
        public ExportFactory<ICommandParameters, CommandArgumentsAttribute>[] CommandArguments { get; set; }

        [Import]
        public Lazy<CommandParameters> Parameters { get; set; }

        [Export]
        public RootCommand RootCommand { get; set; }

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

                    subCommand.Handler = new PretzelCommandHandler(commandArguments, command);
                }

                RootCommand.AddCommand(subCommand);
            }
        }
    }
}
