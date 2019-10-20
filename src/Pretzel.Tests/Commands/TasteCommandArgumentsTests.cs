using System;
using System.IO.Abstractions;
using System.Linq;
using Pretzel.Logic.Commands;
using Xunit;

namespace Pretzel.Tests.Commands
{
    public class TasteCommandArgumentsTests : BakeBaseCommandArgumentsTests<TasteCommandArguments>
    {
        protected override TasteCommandArguments CreateArguments(IFileSystem fileSystem)
            => new TasteCommandArguments(fileSystem);

        [Theory]
        [InlineData("--nobrowser", true)]
        public void NoBrowser(string argument, bool expectedValue)
        {
            var sut = BuildArguments(argument, expectedValue.ToString());

            Assert.Equal(expectedValue, sut.NoBrowser);
            Assert.Equal(!expectedValue, sut.LaunchBrowser);
        }

        [Fact]
        public void NoBrowserDefaultValue()
        {
            var sut = BuildArguments();

            Assert.False(sut.NoBrowser);
            Assert.True(sut.LaunchBrowser);
        }

        [Theory]
        [InlineData("--port", 9000)]
        [InlineData("-p", 9090)]
        public void Port(string argument, int expectedValue)
        {
            var sut = BuildArguments(argument, expectedValue.ToString());

            Assert.Equal(expectedValue, sut.Port);
        }

        [Fact]
        public void PortDefaultValue()
        {
            var sut = BuildArguments();

            Assert.Equal(8080, sut.Port);
        }
    }
}
