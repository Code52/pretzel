using NSubstitute;
using Pretzel.Commands;
using Pretzel.Logic.Commands;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Pretzel.Tests.Commands
{
    public abstract class ParametersTests<T> where T : BaseParameters
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

            new ModelBinder(typeof(BakeCommandParameters))
                .UpdateInstance(parameters, context.BindingContext);

            parameters.BindingCompleted();

            return parameters;
        }
    }

    public class BakeCommandParametersTests : ParametersTests<BakeCommandParameters>
    {
        protected override BakeCommandParameters CreateParameters(IFileSystem fileSystem)
            => new BakeCommandParameters(fileSystem);

        [Theory]
        [InlineData("-c")]
        [InlineData("--cleantarget")]
        public void CleanTarget(string argument)
        {
            var sut = BuildParameters(argument);

            Assert.True(sut.CleanTarget);
        }

        [Theory]
        [InlineData("--drafts")]
        public void Drafts(string argument)
        {
            var sut = BuildParameters(argument);

            Assert.True(sut.Drafts);
        }

        [Theory]
        [InlineData("-t", "liquid")]
        [InlineData("--template", "razor")]
        public void Template(string argument, string value)
        {
            var sut = BuildParameters(argument, value);

            Assert.Equal(value, sut.Template);
        }

        [Theory]
        [InlineData("-d", "foo/_site")]
        [InlineData("--destination", "bar/mySite")]
        public void Destination(string argument, string value)
        {
            var sut = BuildParameters(argument, value);

            Assert.Equal(fileSystem.Path.Combine(sut.Source, value), sut.Destination);
        }

        [Theory]
        [InlineData("-d", @"c:\tmp\_site")]
        public void DestinationRooted(string argument, string value)
        {
            var sut = BuildParameters(argument, value);

            Assert.Equal(value, sut.Destination);
        }

        [Fact]
        public void DestinationEmpty()
        {
            var sut = BuildParameters("-d");

            Assert.Equal(fileSystem.Path.Combine(sut.Source, "_site"), sut.Destination);
        }

        [Fact]
        public void DestinationDefaultValue()
        {
            var sut = BuildParameters();

            Assert.Equal(fileSystem.Path.Combine(sut.Source, "_site"), sut.Destination);
        }
    }
}
