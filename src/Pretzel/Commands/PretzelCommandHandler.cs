using System;
using System.Linq;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.Composition;
using System.Threading.Tasks;
using Pretzel.Logic;
using Pretzel.Logic.Commands;

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
}
