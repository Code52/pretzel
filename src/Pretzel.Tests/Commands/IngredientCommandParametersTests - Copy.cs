using System;
using System.IO.Abstractions;
using System.Linq;
using Pretzel.Commands;
using Xunit;

namespace Pretzel.Tests.Commands
{
    public class RecipeCommandParametersTests : PretzelBaseCommandParametersTests<RecipeCommandParameters>
    {
        protected override RecipeCommandParameters CreateParameters(IFileSystem fileSystem)
            => new RecipeCommandParameters(fileSystem);

        [Theory]
        [InlineData("--wiki", true)]
        [InlineData("", false)]
        public void WithWiki(string argument, bool expectedValue)
        {
            var sut = BuildParameters(argument);

            Assert.Equal(expectedValue, sut.Wiki);
        }

        [Theory]
        [InlineData("--withproject", true)]
        [InlineData("", false)]
        public void WithProject(string argument, bool expectedValue)
        {
            var sut = BuildParameters(argument);

            Assert.Equal(expectedValue, sut.WithProject);
        }
    }
}
