using NSubstitute;
using Pretzel.Commands;
using Pretzel.Logic;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensibility;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Pretzel.Tests.Commands
{
    public class PretzelCommandHandlerTests
    {
        public class TestParams : ICommandParameters
        {
            public IList<Option> Options => throw new NotImplementedException();

            public void BindingCompleted()
            {
            }

            public bool SomeOption { get; set; }
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
            var testParams = new TestParams();
            var sut = new PretzelCommandHandler(
                Substitute.For<IConfiguration>(),
                testParams,
                new ExportFactory<IPretzelCommand, CommandInfoAttribute>(
                    () => Tuple.Create(
                            Substitute.For<IPretzelCommand>(),
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
            var testParams = Substitute.For<ICommandParameters>();

            var sut = new PretzelCommandHandler(
              Substitute.For<IConfiguration>(),
              testParams,
              new ExportFactory<IPretzelCommand, CommandInfoAttribute>(
                  () => Tuple.Create(
                          Substitute.For<IPretzelCommand>(),
                          new Action(() => { })
                      ), new CommandInfoAttribute()));

            await sut.InvokeAsync(context);

            testParams.Received().BindingCompleted();
        }

        [Fact]
        public async Task Configuration_ReadFromFile_IsCalled_WithPathProvider()
        {
            var configuration = Substitute.For<IConfiguration>();

            var rootCommand = new RootCommand();
            var testParams = Substitute.For<ICommandParameters, ISourcePathProvider>();
            ((ISourcePathProvider)testParams).Source.Returns("foo");

            var context = new InvocationContext(new Parser(rootCommand).Parse(string.Empty), Substitute.For<IConsole>());

            var sut = new PretzelCommandHandler(
                configuration,
                testParams,
                new ExportFactory<IPretzelCommand, CommandInfoAttribute>(
                    () => Tuple.Create(
                            Substitute.For<IPretzelCommand>(),
                            new Action(() => { })
                        ), new CommandInfoAttribute()));

            await sut.InvokeAsync(context);

            configuration.Received().ReadFromFile("foo");
        }

        [CommandArguments(CommandName = nameof(TestParams2))]
        public class TestParams2 : ICommandParameters, ICommandParametersExtendable
        {
            public IList<Option> Options => throw new NotImplementedException();

            public void BindingCompleted()
            {
            }

            public class ExtendedArguments : IHaveCommandLineArgs
            {
                public bool BindingCompletedCalled { get; private set; }
                public void BindingCompleted()
                {
                    BindingCompletedCalled = true;
                }

                public void UpdateOptions(IList<Option> options)
                {
                    throw new NotImplementedException();
                }

                public bool OtherOption { get; set; }
            }

            public readonly ExtendedArguments Args = new ExtendedArguments();

            public ExportFactory<IHaveCommandLineArgs, CommandArgumentsExtentionAttribute>[] ArgumentExtenders => new[]
            {
                new ExportFactory<IHaveCommandLineArgs, CommandArgumentsExtentionAttribute>(() => Tuple.Create<IHaveCommandLineArgs, Action>(Args, new Action(() => { })), new CommandArgumentsExtentionAttribute{ CommandNames = new []{ nameof(TestParams2)} })
            };
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
            var testParams = new TestParams2();
            var sut = new PretzelCommandHandler(
                Substitute.For<IConfiguration>(),
                testParams,
                new ExportFactory<IPretzelCommand, CommandInfoAttribute>(
                    () => Tuple.Create(
                            Substitute.For<IPretzelCommand>(),
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
            var testParams = new TestParams2();
            var sut = new PretzelCommandHandler(
                Substitute.For<IConfiguration>(),
                testParams,
                new ExportFactory<IPretzelCommand, CommandInfoAttribute>(
                    () => Tuple.Create(
                            Substitute.For<IPretzelCommand>(),
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
            var command = Substitute.For<IPretzelCommand>();

            var sut = new PretzelCommandHandler(
                Substitute.For<IConfiguration>(),
                Substitute.For<ICommandParameters>(),
                new ExportFactory<IPretzelCommand, CommandInfoAttribute>(
                    () => Tuple.Create(
                            command,
                            new Action(() => { })
                        ), new CommandInfoAttribute()));

            await sut.InvokeAsync(context);

            await command.Received(1).Execute();
        }

        [Fact]
        public async Task ExecutionOrder()
        {
            var rootCommand = new RootCommand();

            var context = new InvocationContext(new Parser(rootCommand).Parse(""), Substitute.For<IConsole>());
            var configuration = Substitute.For<IConfiguration>();
            var @params = Substitute.For<ICommandParameters, ISourcePathProvider>();
            ((ISourcePathProvider)@params).Source.Returns("bar");
            var command = Substitute.For<IPretzelCommand>();

            var sut = new PretzelCommandHandler(
                configuration,
                @params,
                new ExportFactory<IPretzelCommand, CommandInfoAttribute>(
                    () => Tuple.Create(
                            command,
                            new Action(() => { })
                        ), new CommandInfoAttribute()));

            await sut.InvokeAsync(context);

            Received.InOrder(async () =>
            {
                @params.BindingCompleted();
                configuration.ReadFromFile("bar");
                await command.Execute();
            });
        }
    }
}
