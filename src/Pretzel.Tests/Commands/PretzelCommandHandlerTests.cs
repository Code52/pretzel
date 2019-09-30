using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Pretzel.Commands;
using Pretzel.Logic;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensibility;
using Xunit;

namespace Pretzel.Tests.Commands
{
    public class PretzelCommandHandlerTests
    {
        public class TestArguments : ICommandArguments
        {
            public IList<Option> Options => new List<Option>();

            public void BindingCompleted()
            {
            }

            public bool SomeOption { get; set; }

            public IList<ICommandArgumentsExtension> Extensions => new List<ICommandArgumentsExtension>();
        }

        [Fact]
        public async Task CommandParametersGetsBinded()
        {
            var rootCommand = new RootCommand();
            rootCommand.AddOption(new Option("--someoption")
            {
                Argument = new Argument<bool>()
            });

            var context = new InvocationContext(new Parser(rootCommand).Parse("--someoption"), Substitute.For<IConsole>());
            var testParams = new TestArguments();

            var sut = new PretzelCommandHandler(
                Substitute.For<IConfiguration>(),
                testParams,
                new ExportFactory<Logic.Commands.ICommand, CommandInfoAttribute>(
                    () => Tuple.Create(
                            Substitute.For<Logic.Commands.ICommand>(),
                            new Action(() => { })
                        ), new CommandInfoAttribute()));

            await sut.InvokeAsync(context);

            Assert.True(testParams.SomeOption);
        }

        [Fact]
        public async Task CommandParameters_BindingCompleted_IsCalled()
        {
            var rootCommand = new RootCommand();

            var context = new InvocationContext(new Parser(rootCommand).Parse(string.Empty), Substitute.For<IConsole>());
            var testArgument = Substitute.For<ICommandArguments>();

            var sut = new PretzelCommandHandler(
              Substitute.For<IConfiguration>(),
              testArgument,
              new ExportFactory<Logic.Commands.ICommand, CommandInfoAttribute>(
                  () => Tuple.Create(
                          Substitute.For<Logic.Commands.ICommand>(),
                          new Action(() => { })
                      ), new CommandInfoAttribute()));

            await sut.InvokeAsync(context);

            testArgument.Received().BindingCompleted();
        }

        [Fact]
        public async Task Configuration_ReadFromFile_IsCalled_WithPathProvider()
        {
            var configuration = Substitute.For<IConfiguration>();

            var rootCommand = new RootCommand();
            var testArgument = Substitute.For<ICommandArguments, ISourcePathProvider>();
            ((ISourcePathProvider)testArgument).Source.Returns("foo");

            var context = new InvocationContext(new Parser(rootCommand).Parse(string.Empty), Substitute.For<IConsole>());

            var sut = new PretzelCommandHandler(
                configuration,
                testArgument,
                new ExportFactory<Logic.Commands.ICommand, CommandInfoAttribute>(
                    () => Tuple.Create(
                            Substitute.For<Logic.Commands.ICommand>(),
                            new Action(() => { })
                        ), new CommandInfoAttribute()));

            await sut.InvokeAsync(context);

            configuration.Received().ReadFromFile("foo");
        }

        [CommandArguments]
        public class TestArguments2 : ICommandArguments
        {
            public IList<Option> Options { get; } = new List<Option>();

            public void BindingCompleted()
            {
            }

            public class ExtendedArguments : ICommandArgumentsExtension
            {
                public bool BindingCompletedCalled { get; private set; }
                public void BindingCompleted()
                {
                    BindingCompletedCalled = true;
                }

                public IList<Option> Options { get; } = Array.Empty<Option>();

                public bool OtherOption { get; set; }
            }

            public readonly ExtendedArguments Args = new ExtendedArguments();

            public ExportFactory<ICommandArgumentsExtension, CommandArgumentsExtensionAttribute>[] ArgumentExtensions => new[]
            {
                new ExportFactory<ICommandArgumentsExtension, CommandArgumentsExtensionAttribute>(() => Tuple.Create<ICommandArgumentsExtension, Action>(Args, new Action(() => { })), new CommandArgumentsExtensionAttribute{ CommandNames = new []{ nameof(TestArguments2)} })
            };

            public IList<ICommandArgumentsExtension> Extensions { get; } = new List<ICommandArgumentsExtension>();
        }


        [Fact]
        public async Task CommandParametersExtentionsGetsBinded()
        {
            var rootCommand = new RootCommand();
            rootCommand.AddOption(new Option("--otheroption")
            {
                Argument = new Argument<bool>()
            });

            var context = new InvocationContext(new Parser(rootCommand).Parse("--otheroption"), Substitute.For<IConsole>());
            var testParams = new TestArguments2();

            testParams.Extensions.Add(testParams.Args);

            var sut = new PretzelCommandHandler(
                Substitute.For<IConfiguration>(),
                testParams,
                new ExportFactory<Logic.Commands.ICommand, CommandInfoAttribute>(
                    () => Tuple.Create(
                            Substitute.For<Logic.Commands.ICommand>(),
                            new Action(() => { })
                        ), new CommandInfoAttribute()));

            await sut.InvokeAsync(context);

            Assert.True(testParams.Args.OtherOption);
        }

        [Fact]
        public async Task CommandParametersExtention_BindingCompleted_IsCalled()
        {
            var rootCommand = new RootCommand();
            rootCommand.AddOption(new Option("--otheroption")
            {
                Argument = new Argument<bool>()
            });

            var context = new InvocationContext(new Parser(rootCommand).Parse("--otheroption"), Substitute.For<IConsole>());
            var testParams = new TestArguments2();
            testParams.Extensions.Add(testParams.Args);
            var sut = new PretzelCommandHandler(
                Substitute.For<IConfiguration>(),
                testParams,
                new ExportFactory<Logic.Commands.ICommand, CommandInfoAttribute>(
                    () => Tuple.Create(
                            Substitute.For<Logic.Commands.ICommand>(),
                            new Action(() => { })
                        ), new CommandInfoAttribute()));

            await sut.InvokeAsync(context);

            Assert.True(testParams.Args.BindingCompletedCalled);
        }

        [Fact]
        public async Task CommandGetsExecuted()
        {
            var rootCommand = new RootCommand();

            var context = new InvocationContext(new Parser(rootCommand).Parse(""), Substitute.For<IConsole>());
            var command = Substitute.For<Logic.Commands.ICommand>();
            var arguments = Substitute.For<ICommandArguments>();

            var sut = new PretzelCommandHandler(
                Substitute.For<IConfiguration>(),
                arguments,
                new ExportFactory<Logic.Commands.ICommand, CommandInfoAttribute>(
                    () => Tuple.Create(
                            command,
                            new Action(() => { })
                        ), new CommandInfoAttribute()));

            await sut.InvokeAsync(context);

            await command.Received(1).Execute(arguments);
        }

        [Fact]
        public async Task ExecutionOrder()
        {
            var rootCommand = new RootCommand();

            var context = new InvocationContext(new Parser(rootCommand).Parse(""), Substitute.For<IConsole>());
            var configuration = Substitute.For<IConfiguration>();
            var arguments = Substitute.For<ICommandArguments, ISourcePathProvider>();
            ((ISourcePathProvider)arguments).Source.Returns("bar");
            var command = Substitute.For<Logic.Commands.ICommand>();

            var sut = new PretzelCommandHandler(
                configuration,
                arguments,
                new ExportFactory<Logic.Commands.ICommand, CommandInfoAttribute>(
                    () => Tuple.Create(
                            command,
                            new Action(() => { })
                        ), new CommandInfoAttribute()));

            await sut.InvokeAsync(context);

            Received.InOrder(async () =>
            {
                arguments.BindingCompleted();
                configuration.ReadFromFile("bar");
                await command.Execute(arguments);
            });
        }
    }
}
