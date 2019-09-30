using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.Composition;
using System.Composition.Hosting;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using NSubstitute;
using Pretzel.Logic.Commands;
using Xunit;

namespace Pretzel.Tests.Commands
{
    public class BaseCommandArgumentsTests
    {
        [Export]
        [Shared]
        [CommandArguments]
        public class BaseCommandArgumentsImpl : BaseCommandArguments
        {
            protected override IEnumerable<Option> CreateOptions() => new[]
            {
                new Option("-base")
            };
        }
        
        [Fact]
        public void ExtentionIsPossible()
        {
            var configuration = new ContainerConfiguration();
            configuration.WithPart<BaseCommandArgumentsImpl>();
            using(var container = configuration.CreateContainer())
            {
                var sut = container.GetExport<BaseCommandArgumentsImpl>();
                sut.BuildOptions();
                Assert.NotNull(sut.Options);
                Assert.Equal(1, sut.Options.Count);
            }
        }
    }

    public abstract class BaseCommandArgumentsTests<T> where T : BaseCommandArguments
    {
        readonly IConsole Console = Substitute.For<IConsole>();
        protected readonly MockFileSystem fileSystem = new MockFileSystem();
        protected abstract T CreateArguments(IFileSystem fileSystem);
        protected T BuildArguments(params string[] args)
        {
            var rootCommand = new RootCommand();

            var arguments = CreateArguments(fileSystem);
            arguments.BuildOptions();

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
