using System;
using System.IO.Abstractions;
using System.Linq;
using Pretzel.Commands;
using Xunit;

namespace Pretzel.Tests.Commands
{
    public class ImportCommandParametersTests : PretzelBaseCommandParametersTests<ImportCommandParameters>
    {
        protected override ImportCommandParameters CreateParameters(IFileSystem fileSystem)
            => new ImportCommandParameters(fileSystem);

        [Theory]
        [InlineData("--importtype", "foo")]
        [InlineData("-i", "bar")]
        public void ImportType(string argument, string expectedValue)
        {
            var sut = BuildParameters(argument, expectedValue);

            Assert.Equal(expectedValue, sut.ImportType);
        }

        [Theory]
        [InlineData("--importfile", "baz")]
        [InlineData("-f", "buzz")]
        public void ImportFile(string argument, string expectedValue)
        {
            var sut = BuildParameters(argument, expectedValue);

            Assert.Equal(expectedValue, sut.ImportFile);
        }
    }
}
