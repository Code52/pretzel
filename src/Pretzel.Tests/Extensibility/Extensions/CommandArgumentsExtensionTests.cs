using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using NSubstitute;
using Pretzel.Logic.Extensibility;

namespace Pretzel.Tests.Extensibility.Extensions
{
    public abstract class CommandArgumentsExtensionTests<T> where T : ICommandArgumentsExtension
    {
        readonly IConsole Console = Substitute.For<IConsole>();
        protected readonly MockFileSystem fileSystem = new MockFileSystem();
        protected abstract T CreateArguments();
        protected T BuildArguments(params string[] args)
        {
            var rootCommand = new RootCommand();

            var arguments = CreateArguments();
            
            foreach (var option in arguments.Options)
                rootCommand.AddOption(option);

            var context = new InvocationContext(new Parser(rootCommand).Parse(args), Console);

            new ModelBinder(arguments.GetType())
                .UpdateInstance(arguments, context.BindingContext);

            arguments.BindingCompleted();

            return arguments;
        }
    }
}
