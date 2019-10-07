using System;
using System.IO.Abstractions;
using System.Linq;
using Pretzel.Logic.Commands;
using Xunit;

namespace Pretzel.Tests.Commands
{
    public class ImportCommandArgumentsTests : PretzelBaseCommandArgumentsTests<ImportCommandArguments>
    {
        protected override ImportCommandArguments CreateArguments(IFileSystem fileSystem)
            => new ImportCommandArguments(fileSystem);

        [Theory]
        [InlineData("--importtype", "foo")]
        [InlineData("-i", "bar")]
        public void ImportType(string argument, string expectedValue)
        {
            var sut = BuildArguments(argument, expectedValue);

            Assert.Equal(expectedValue, sut.ImportType);
        }

        [Theory]
        [InlineData("--importfile", "baz")]
        [InlineData("-f", "buzz")]
        public void ImportFile(string argument, string expectedValue)
        {
            var sut = BuildArguments(argument, expectedValue);

            Assert.Equal(expectedValue, sut.ImportFile);
        }
    }
}
