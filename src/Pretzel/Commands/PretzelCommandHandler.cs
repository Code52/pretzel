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
        public ICommandArguments CommandParameters { get; }
        public ExportFactory<IPretzelCommand, CommandInfoAttribute> Command { get; }
        public IConfiguration Configuration { get; }

        public PretzelCommandHandler(IConfiguration configuration, ICommandArguments commandParameters, ExportFactory<IPretzelCommand, CommandInfoAttribute> command)
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

            if (CommandParameters is ISourcePathProvider pathProvider)
            {
                Configuration.ReadFromFile(pathProvider.Source);
            }

            foreach (var argumentsExtension in CommandParameters.Extensions)
            {
                new ModelBinder(argumentsExtension.GetType())
                    .UpdateInstance(argumentsExtension, bindingContext);

                argumentsExtension.BindingCompleted();
            }

            return await Command.CreateExport().Value.Execute();
        }
    }
}
