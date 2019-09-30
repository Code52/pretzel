using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using NSubstitute;
using Pretzel.Logic.Commands;

namespace Pretzel.Tests.Commands
{
    public abstract class ParametersTests<T> where T : BaseCommandArguments
    {
        readonly IConsole Console = Substitute.For<IConsole>();
        protected readonly MockFileSystem fileSystem = new MockFileSystem();
        protected abstract T CreateParameters(IFileSystem fileSystem);
        protected T BuildParameters(params string[] args)
        {
            var rootCommand = new RootCommand();

            var parameters = CreateParameters(fileSystem);
            parameters.OnImportsSatisfied();

            foreach (var option in parameters.Options)
                rootCommand.AddOption(option);

            var context = new InvocationContext(new Parser(rootCommand).Parse(args), Console);

            new ModelBinder(parameters.GetType())
                .UpdateInstance(parameters, context.BindingContext);

            parameters.BindingCompleted();

            return parameters;
        }
    }
}
