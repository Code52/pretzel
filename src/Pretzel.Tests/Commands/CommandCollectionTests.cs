using NSubstitute;
using Pretzel.Commands;
using Pretzel.Logic;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensibility;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Composition;
using System.Composition.Hosting;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Pretzel.Tests.Commands
{
    public class CommandCollectionTests
    {
        [Fact]
        public void RootCommandGetsCreatedOnImportSatisfied()
        {
            var collection = new CommandCollection();
            collection.OnImportsSatisfied();
            Assert.NotNull(collection.RootCommand);
        }

        [Fact]
        public void SubCommandAreAddedToRootCommand()
        {
            var arguments = Substitute.For<ICommandArguments>();
            var collection = new CommandCollection
            {
                Commands = new ExportFactory<Logic.Commands.ICommand, CommandInfoAttribute>[]
                {
                    new ExportFactory<Logic.Commands.ICommand, CommandInfoAttribute>(CreateCommand, new CommandInfoAttribute
                    {
                        Name = "test",
                        Description = "desc",
                        ArgumentsType = arguments.GetType()
                    })
                },
                CommandArguments = new[] { arguments }
            };
            collection.OnImportsSatisfied();

            Assert.Contains(collection.RootCommand.Children.OfType<Command>(), c => c.Name == "test");
            Assert.Contains(collection.RootCommand.Children.OfType<Command>(), c => c.Description == "desc");
        }

        [Fact]
        public void SubCommandsWithArgumentGetsHandler()
        {
            var parameters = Substitute.For<ICommandArguments>();
            parameters.Options.Returns(new List<Option>
            {
                new Option("-i")
            });

            var commandExportFactory = new ExportFactory<Logic.Commands.ICommand, CommandInfoAttribute>(
                CreateCommand,
                new CommandInfoAttribute
                {
                    Name = "test",
                    Description = "desc",
                    ArgumentsType = parameters.GetType()
                });

         
            var collection = new CommandCollection
            {
                Configuration = Substitute.For<IConfiguration>(),
                Commands = new ExportFactory<Logic.Commands.ICommand, CommandInfoAttribute>[]
                {
                    commandExportFactory
                },
                CommandArguments = new Logic.Commands.ICommandArguments[]
                {
                    parameters
                }
            };

            collection.OnImportsSatisfied();

            var command = collection.RootCommand.Children.OfType<Command>().First();

            Assert.Single(command.OfType<Option>());
            Assert.NotNull(command.Handler);
            Assert.IsType<PretzelCommandHandler>(command.Handler);
            Assert.NotNull(((PretzelCommandHandler)command.Handler).CommandArguments);
            Assert.Equal(parameters, ((PretzelCommandHandler)command.Handler).CommandArguments);
            Assert.NotNull(((PretzelCommandHandler)command.Handler).Configuration);
            Assert.Equal(collection.Configuration, ((PretzelCommandHandler)command.Handler).Configuration);
            Assert.NotNull(((PretzelCommandHandler)command.Handler).Command);
            Assert.Equal(commandExportFactory, ((PretzelCommandHandler)command.Handler).Command);
        }

        Tuple<Logic.Commands.ICommand, Action> CreateCommand()
            => Tuple.Create(Substitute.For<Logic.Commands.ICommand>(), new Action(() => { }));

        public class ICommandParametersExtentionsTests : IDisposable
        {
            CompositionHost Container;
            public ICommandParametersExtentionsTests()
            {
                var configuration = new ContainerConfiguration();
                configuration.WithPart<CommandCollection>();
                configuration.WithPart<TestCommand1>();
                configuration.WithPart<TestCommand2>();
                configuration.WithPart<TestCommand3>();
                configuration.WithPart<TestCommandArguments1>();
                configuration.WithPart<TestCommandArguments2>();
                configuration.WithPart<TestCommandArguments3>();
                configuration.WithPart<Extender1>();
                configuration.WithPart<Extender2>();
                configuration.WithPart<Extender3>();
                configuration.WithPart<Extender4>();
                configuration.WithPart<ConfigurationMock>();
                Container = configuration.CreateContainer();
            }

            [Fact]
            public void CollectsOnlyCommandsWhereNamesMatch1()
            {
                Container.GetExport<CommandCollection>();
                var target = Container.GetExport<TestCommandArguments1>();
                
                Assert.Equal(2, target.Extensions.Count);
                Assert.Contains(target.Extensions, e => e is Extender1);
                Assert.Contains(target.Extensions, e => e is Extender3);
            }

            [Fact]
            public void CollectsOnlyCommandsWhereNamesMatch2()
            {
                Container.GetExport<CommandCollection>();
                var target = Container.GetExport<TestCommandArguments2>();
                
                Assert.Equal(2, target.Extensions.Count);
                Assert.Contains(target.Extensions, e => e is Extender2);
                Assert.Contains(target.Extensions, e => e is Extender3);
            }

            [Fact]
            public void CollectsOnlyCommandsWhereInterfacesAreExported()
            {
                Container.GetExport<CommandCollection>();
                var target = Container.GetExport<ITestCommandArguments3>();

                Assert.Equal(1, target.Extensions.Count);
                Assert.Contains(target.Extensions, e => e is Extender4);
            }

            public void Dispose()
                => Container?.Dispose();

            [Export]
            [Shared]
            [CommandArguments]
            public class TestCommandArguments1 : ICommandArguments
            {
                [ImportMany]
                public ExportFactory<ICommandArgumentsExtension, CommandArgumentsExtensionAttribute>[] ArgumentExtensions { get; set; }

                public IList<Option> Options => new List<Option>();

                public IList<ICommandArgumentsExtension> Extensions { get; } = new List<ICommandArgumentsExtension>();

                public void BindingCompleted() { }
            }

            [Export]
            [Shared]
            [CommandInfo(Name = "test1", ArgumentsType = typeof(TestCommandArguments1))]
            public class TestCommand1 : Logic.Commands.ICommand
            {
                public Task<int> Execute(ICommandArguments arguments)
                {
                    return Task.FromResult(0);
                }
            }

            [Export]
            [Shared]
            [CommandArguments]
            public class TestCommandArguments2 : ICommandArguments
            {
                [ImportMany]
                public ExportFactory<ICommandArgumentsExtension, CommandArgumentsExtensionAttribute>[] ArgumentExtensions { get; set; }

                public IList<Option> Options => new List<Option>();

                public IList<ICommandArgumentsExtension> Extensions { get; } = new List<ICommandArgumentsExtension>();

                public void BindingCompleted() { }
            }


            [Export]
            [Shared]
            [CommandInfo(Name = "test2", ArgumentsType = typeof(TestCommandArguments2))]
            public class TestCommand2 : Logic.Commands.ICommand
            {
                public Task<int> Execute(ICommandArguments arguments)
                {
                    return Task.FromResult(0);
                }
            }

            public interface ITestCommandArguments3 : ICommandArguments { }

            [Export(typeof(ITestCommandArguments3))]
            [Shared]
            [CommandArguments]
            public class TestCommandArguments3 : ITestCommandArguments3
            {
                [ImportMany]
                public ExportFactory<ICommandArgumentsExtension, CommandArgumentsExtensionAttribute>[] ArgumentExtensions { get; set; }

                public IList<Option> Options => new List<Option>();

                public IList<ICommandArgumentsExtension> Extensions { get; } = new List<ICommandArgumentsExtension>();

                public void BindingCompleted() { }
            }

            [Export]
            [Shared]
            [CommandInfo(Name = "test3", ArgumentsType = typeof(ITestCommandArguments3))]
            public class TestCommand3 : Logic.Commands.ICommand
            {
                public Task<int> Execute(ICommandArguments arguments)
                {
                    return Task.FromResult(0);
                }
            }

            [CommandArgumentsExtension(CommandArgumentTypes = new[] { typeof(TestCommandArguments1) })]
            public class Extender1 : ICommandArgumentsExtension
            {
                public IList<Option> Options { get; } = Array.Empty<Option>();

                public void BindingCompleted()
                {
                }
            }

            [CommandArgumentsExtension(CommandArgumentTypes = new[] { typeof(TestCommandArguments2) })]
            public class Extender2 : ICommandArgumentsExtension
            {
                public IList<Option> Options { get; } = Array.Empty<Option>();

                public void BindingCompleted()
                {
                }
            }

            [CommandArgumentsExtension(CommandArgumentTypes = new[] { typeof(TestCommandArguments1), typeof(TestCommandArguments2) })]
            public class Extender3 : ICommandArgumentsExtension
            {
                public IList<Option> Options { get; } = Array.Empty<Option>();

                public void BindingCompleted()
                {
                }
            }

            [CommandArgumentsExtension(CommandArgumentTypes = new[] { typeof(ITestCommandArguments3) })]
            public class Extender4 : ICommandArgumentsExtension
            {
                public IList<Option> Options { get; } = Array.Empty<Option>();

                public void BindingCompleted()
                {
                }
            }

        }
    }
}
