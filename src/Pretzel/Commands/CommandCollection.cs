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
using Pretzel.Logic;

namespace Pretzel.Commands
{

    public class PretzelCommandHandler : ICommandHandler
    {
        private readonly ICommandParameters commandParameters;
        private readonly ExportFactory<ICommand, CommandInfoAttribute> command;
        private readonly IConfiguration configuration;

        public PretzelCommandHandler(IConfiguration configuration, ICommandParameters commandParameters, ExportFactory<ICommand, CommandInfoAttribute> command)
        {
            this.configuration = configuration;
            this.commandParameters = commandParameters;
            this.command = command;
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var bindingContext = context.BindingContext;

            if (commandParameters != null)
            {
                new ModelBinder(commandParameters.GetType())
                   .UpdateInstance(commandParameters, bindingContext);

                commandParameters.BindingCompleted();
            }

            if (commandParameters is IPathProvider pathProvider)
            {
                configuration.ReadFromFile(pathProvider.Path);
            }

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
