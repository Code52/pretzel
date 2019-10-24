using System;
using System.IO.Abstractions;
using System.Linq;
using Pretzel.Logic.Commands;
using Xunit;

namespace Pretzel.Tests.Commands
{
    public class IngredientCommandArgumentsTests : PretzelBaseCommandArgumentsTests<IngredientCommandArguments>
    {
        protected override IngredientCommandArguments CreateArguments(IFileSystem fileSystem)
            => new IngredientCommandArguments(fileSystem);

        [Theory]
        [InlineData("--newposttitle", "My AwesomeBlog")]
        [InlineData("-n", "This is the second blog")]
        public void NewPostTitle(string argument, string value)
        {
            var sut = BuildArguments(argument, value);

            Assert.Equal(value, sut.NewPostTitle);
        }

        [Fact]
        public void NewPostTitleDefaultValue()
        {
            var sut = BuildArguments();

            Assert.Equal("New post", sut.NewPostTitle);
        }
    }
}
