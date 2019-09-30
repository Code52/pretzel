using System;
using System.IO.Abstractions;
using System.Linq;
using Pretzel.Commands;
using Xunit;

namespace Pretzel.Tests.Commands
{
    public class IngredientCommandParametersTests : PretzelBaseCommandParametersTests<IngredientCommandArguments>
    {
        protected override IngredientCommandArguments CreateParameters(IFileSystem fileSystem)
            => new IngredientCommandArguments(fileSystem);

        [Theory]
        [InlineData("--newposttitle", "My AwesomeBlog")]
        [InlineData("-n", "This is the second blog")]
        public void NewPostTitle(string argument, string value)
        {
            var sut = BuildParameters(argument, value);

            Assert.Equal(value, sut.NewPostTitle);
        }

        [Fact]
        public void NewPostTitleDefaultValue()
        {
            var sut = BuildParameters();

            Assert.Equal("New post", sut.NewPostTitle);
        }
    }
}
