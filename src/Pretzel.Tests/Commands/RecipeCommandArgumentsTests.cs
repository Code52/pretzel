using System;
using System.IO.Abstractions;
using System.Linq;
using Pretzel.Commands;
using Xunit;

namespace Pretzel.Tests.Commands
{
    public class RecipeCommandArgumentsTests : PretzelBaseCommandArgumentsTests<RecipeCommandArguments>
    {
        protected override RecipeCommandArguments CreateArguments(IFileSystem fileSystem)
            => new RecipeCommandArguments(fileSystem);

        [Theory]
        [InlineData("--wiki", true)]
        [InlineData("", false)]
        public void WithWiki(string argument, bool expectedValue)
        {
            var sut = BuildArguments(argument);

            Assert.Equal(expectedValue, sut.Wiki);
        }

        [Theory]
        [InlineData("--withproject", true)]
        [InlineData("", false)]
        public void WithProject(string argument, bool expectedValue)
        {
            var sut = BuildArguments(argument);

            Assert.Equal(expectedValue, sut.WithProject);
        }
    }
}
