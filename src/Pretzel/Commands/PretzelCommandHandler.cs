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
        public ICommandParameters CommandParameters { get; }
        public ExportFactory<IPretzelCommand, CommandInfoAttribute> Command { get; }
        public IConfiguration Configuration { get; }

        public PretzelCommandHandler(IConfiguration configuration, ICommandParameters commandParameters, ExportFactory<IPretzelCommand, CommandInfoAttribute> command)
        {
            Configuration = configuration;
            CommandParameters = commandParameters;
            Command = command;
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var bindingContext = context.BindingContext;

            if (CommandParameters != null)
            {
                new ModelBinder(CommandParameters.GetType())
                   .UpdateInstance(CommandParameters, bindingContext);

                CommandParameters.BindingCompleted();
            }

            if (CommandParameters is IPathProvider pathProvider)
            {
                Configuration.ReadFromFile(pathProvider.Path);
            }

            if (CommandParameters is ICommandParametersExtendable commandParametersExtendable)
            {
                foreach (var factory in commandParametersExtendable.GetCommandExtentions())
                {
                    var extentedArguments = factory.CreateExport().Value;

                    new ModelBinder(extentedArguments.GetType())
                        .UpdateInstance(extentedArguments, bindingContext);

                    extentedArguments.BindingCompleted();
                }
            }

            await Command.CreateExport().Value.Execute();

            return 0;
        }
    }
}
