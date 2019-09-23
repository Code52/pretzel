using System;
using System.IO.Abstractions;
using System.Linq;
using Pretzel.Commands;
using Xunit;

namespace Pretzel.Tests.Commands
{
    public class TasteCommandParametersTests : ParametersTests<TasteCommandParameters>
    {
        protected override TasteCommandParameters CreateParameters(IFileSystem fileSystem)
            => new TasteCommandParameters(fileSystem);

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
        [InlineData("--nobrowser", true)]
        public void NoBrowser(string argument, bool expectedValue)
        {
            var sut = BuildParameters(argument, expectedValue.ToString());

            Assert.Equal(expectedValue, sut.NoBrowser);
            Assert.Equal(!expectedValue, sut.LaunchBrowser);
        }

        [Fact]
        public void NoBrowserDefaultValue()
        {
            var sut = BuildParameters();

            Assert.False(sut.NoBrowser);
            Assert.True(sut.LaunchBrowser);
        }

        [Theory]
        [InlineData("--port", 9000)]
        [InlineData("-p", 9090)]
        public void Port(string argument, int expectedValue)
        {
            var sut = BuildParameters(argument, expectedValue.ToString());

            Assert.Equal(expectedValue, sut.Port);
        }

        [Fact]
        public void PortDefaultValue()
        {
            var sut = BuildParameters();

            Assert.Equal(8080, sut.Port);
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
