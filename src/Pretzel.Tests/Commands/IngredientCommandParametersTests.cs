using System;
using System.IO.Abstractions;
using System.Linq;
using Pretzel.Commands;
using Xunit;

namespace Pretzel.Tests.Commands
{
    public class IngredientCommandParametersTests : PretzelBaseCommandParametersTests<IngredientCommandParameters>
    {
        protected override IngredientCommandParameters CreateParameters(IFileSystem fileSystem)
            => new IngredientCommandParameters(fileSystem);

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
