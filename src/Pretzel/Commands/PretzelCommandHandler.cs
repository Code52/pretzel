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
        public ICommandArguments CommandArguments { get; }
        public ExportFactory<ICommand, CommandInfoAttribute> Command { get; }
        public IConfiguration Configuration { get; }

        public PretzelCommandHandler(IConfiguration configuration, ICommandArguments commandParameters, ExportFactory<ICommand, CommandInfoAttribute> command)
        {
            Configuration = configuration;
            CommandArguments = commandParameters;
            Command = command;
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var bindingContext = context.BindingContext;

            if (CommandArguments != null)
            {
                new ModelBinder(CommandArguments.GetType())
                   .UpdateInstance(CommandArguments, bindingContext);

                CommandArguments.BindingCompleted();
            }

            if (CommandArguments is ISourcePathProvider pathProvider)
            {
                Configuration.ReadFromFile(pathProvider.Source);
            }

            foreach (var argumentsExtension in CommandArguments.Extensions)
            {
                new ModelBinder(argumentsExtension.GetType())
                    .UpdateInstance(argumentsExtension, bindingContext);

                argumentsExtension.BindingCompleted();
            }

            return await Command.CreateExport().Value.Execute(CommandArguments);
        }
    }
}
