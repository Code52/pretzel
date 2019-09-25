using NSubstitute;
using Pretzel.Commands;
using Pretzel.Logic;
using Pretzel.Logic.Commands;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Composition;
using System.Linq;
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
            var collection = new CommandCollection
            {
                Commands = new ExportFactory<Logic.Commands.IPretzelCommand, CommandInfoAttribute>[]
                {
                    new ExportFactory<Logic.Commands.IPretzelCommand, CommandInfoAttribute>(CreateCommand, new CommandInfoAttribute{ CommandName = "test", CommandDescription = "desc" })
                }
            };
            collection.OnImportsSatisfied();

            Assert.Contains(collection.RootCommand.Children.OfType<Command>(), c => c.Name == "test");
            Assert.Contains(collection.RootCommand.Children.OfType<Command>(), c => c.Description == "desc");
        }

        [Fact]
        public void SubCommandsWithoutArgumentGetsHandler()
        {
            var exportFactory = new ExportFactory<Logic.Commands.IPretzelCommand, CommandInfoAttribute>(
                CreateCommand,
                new CommandInfoAttribute
                {
                    CommandName = "test",
                    CommandDescription = "desc"
                });

            var collection = new CommandCollection
            {
                Configuration = Substitute.For<IConfiguration>(),
                Commands = new ExportFactory<Logic.Commands.IPretzelCommand, CommandInfoAttribute>[]
                {
                    exportFactory
                }
            };

            collection.OnImportsSatisfied();

            var command = collection.RootCommand.Children.OfType<Command>().First();
            Assert.NotNull(command.Handler);
            Assert.IsType<PretzelCommandHandler>(command.Handler);
            Assert.Null(((PretzelCommandHandler)command.Handler).CommandParameters);
            Assert.NotNull(((PretzelCommandHandler)command.Handler).Configuration);
            Assert.Equal(collection.Configuration, ((PretzelCommandHandler)command.Handler).Configuration);
            Assert.NotNull(((PretzelCommandHandler)command.Handler).Command);
            Assert.Equal(exportFactory, ((PretzelCommandHandler)command.Handler).Command);
        }

        [Fact]
        public void SubCommandsWithArgumentGetsHandler()
        {
            var parameters = Substitute.For<ICommandParameters>();
            parameters.Options.Returns(new List<Option>
            {
                new Option("-i")
            });

            var commandExportFactory = new ExportFactory<Logic.Commands.IPretzelCommand, CommandInfoAttribute>(
                CreateCommand,
                new CommandInfoAttribute
                {
                    CommandName = "test",
                    CommandDescription = "desc"
                });

            var argumentExportFactory = new ExportFactory<Logic.Commands.ICommandParameters, Logic.Commands.CommandArgumentsAttribute>(
                () => CreateArgument(parameters),
                new Logic.Commands.CommandArgumentsAttribute
                {
                    CommandName = "test"
                });

            var collection = new CommandCollection
            {
                Configuration = Substitute.For<IConfiguration>(),
                Commands = new ExportFactory<Logic.Commands.IPretzelCommand, CommandInfoAttribute>[]
                {
                    commandExportFactory
                },
                CommandArguments = new ExportFactory<Logic.Commands.ICommandParameters, Logic.Commands.CommandArgumentsAttribute>[]
                {
                    argumentExportFactory,
                    new ExportFactory<Logic.Commands.ICommandParameters, Logic.Commands.CommandArgumentsAttribute>(
                    () => CreateArgument(Substitute.For<ICommandParameters>()),
                    new Logic.Commands.CommandArgumentsAttribute
                    {
                        CommandName = "othercommand"
                    })
                }
            };

            collection.OnImportsSatisfied();

            var command = collection.RootCommand.Children.OfType<Command>().First();

            Assert.Single(command.OfType<Option>());
            Assert.NotNull(command.Handler);
            Assert.IsType<PretzelCommandHandler>(command.Handler);
            Assert.NotNull(((PretzelCommandHandler)command.Handler).CommandParameters);
            Assert.Equal(parameters, ((PretzelCommandHandler)command.Handler).CommandParameters);
            Assert.NotNull(((PretzelCommandHandler)command.Handler).Configuration);
            Assert.Equal(collection.Configuration, ((PretzelCommandHandler)command.Handler).Configuration);
            Assert.NotNull(((PretzelCommandHandler)command.Handler).Command);
            Assert.Equal(commandExportFactory, ((PretzelCommandHandler)command.Handler).Command);
        }

        Tuple<Logic.Commands.IPretzelCommand, Action> CreateCommand()
            => Tuple.Create(Substitute.For<Logic.Commands.IPretzelCommand>(), new Action(() => { }));
        Tuple<ICommandParameters, Action> CreateArgument(ICommandParameters parameters)
            => Tuple.Create(parameters, new Action(() => { }));
    }
}
