using System;
using System.IO.Abstractions;
using System.Linq;
using Pretzel.Commands;
using Xunit;

namespace Pretzel.Tests.Commands
{
    public class TasteCommandParametersTests : BakeBaseCommandParametersTests<TasteCommandArguments>
    {
        protected override TasteCommandArguments CreateParameters(IFileSystem fileSystem)
            => new TasteCommandArguments(fileSystem);

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
    }
}
