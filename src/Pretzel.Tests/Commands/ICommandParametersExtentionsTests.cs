using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensibility;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Composition.Hosting;

namespace Pretzel.Tests.Commands
{
    public class ICommandParametersExtentionsTests : IDisposable
    {
        CompositionHost Container;
        public ICommandParametersExtentionsTests()
        {
            var configuration = new ContainerConfiguration();
            configuration.WithPart<TestArguments1Class>();
            configuration.WithPart<TestArguments2Class>();
            configuration.WithPart<Extender1>();
            configuration.WithPart<Extender2>();
            configuration.WithPart<Extender3>();
            Container = configuration.CreateContainer();
        }

        [Fact]
        public void CollectsOnlyCommandsWhereNamesMatch1()
        {
            var target1 = Container.GetExport<TestArguments1Class>();

            var extentions = target1.GetCommandExtentions().ToList();

            Assert.Equal(2, extentions.Count);
            Assert.Contains(extentions, e => e.CreateExport().Value is Extender1);
            Assert.Contains(extentions, e => e.CreateExport().Value is Extender3);

        }

        [Fact]
        public void CollectsOnlyCommandsWhereNamesMatch2()
        {
            var target1 = Container.GetExport<TestArguments2Class>();

            var extentions = target1.GetCommandExtentions().ToList();

            Assert.Equal(2, extentions.Count);
            Assert.Contains(extentions, e => e.CreateExport().Value is Extender2);
            Assert.Contains(extentions, e => e.CreateExport().Value is Extender3);

        }

        public void Dispose()
            => Container?.Dispose();

        [Export]
        [Shared]
        [CommandArguments(CommandName = "test1")]
        public class TestArguments1Class : ICommandParameters, ICommandParametersExtendable
        {
            [ImportMany]
            public ExportFactory<IHaveCommandLineArgs, CommandArgumentsExtentionAttribute>[] ArgumentExtenders { get; set; }

            public IList<Option> Options => new List<Option>();

            public void BindingCompleted() { }
        }

        [Export]
        [Shared]
        [CommandArguments(CommandName = "test2")]
        public class TestArguments2Class : ICommandParameters, ICommandParametersExtendable
        {
            [ImportMany]
            public ExportFactory<IHaveCommandLineArgs, CommandArgumentsExtentionAttribute>[] ArgumentExtenders { get; set; }

            public IList<Option> Options => new List<Option>();

            public void BindingCompleted() { }
        }

        [CommandArgumentsExtention(CommandNames = new[] { "test1" })]
        public class Extender1 : IHaveCommandLineArgs
        {
            public void UpdateOptions(IList<Option> options)
            {
            }

            public void BindingCompleted()
            {
            }
        }

        [CommandArgumentsExtention(CommandNames = new[] { "test2" })]
        public class Extender2 : IHaveCommandLineArgs
        {
            public void UpdateOptions(IList<Option> options)
            {
            }

            public void BindingCompleted()
            {
            }
        }

        [CommandArgumentsExtention(CommandNames = new[] { "test1", "test2" })]
        public class Extender3 : IHaveCommandLineArgs
        {
            public void UpdateOptions(IList<Option> options)
            {
            }

            public void BindingCompleted()
            {
            }
        }

    }
}
