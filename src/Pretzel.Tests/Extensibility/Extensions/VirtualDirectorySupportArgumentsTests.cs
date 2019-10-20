using Pretzel.Logic.Extensibility.Extensions;
using System;
using System.Linq;
using Xunit;

namespace Pretzel.Tests.Extensibility.Extensions
{
    public class VirtualDirectorySupportArgumentsTests : CommandArgumentsExtensionTests<VirtualDirectorySupportArguments>
    {
        protected override VirtualDirectorySupportArguments CreateArguments() => new VirtualDirectorySupportArguments();

        [Theory]
        [InlineData("--virtualdirectory", "bar")]
        [InlineData("-vDir", "foo")]
        public void VirtualDirectory(string argument, string expectedValue)
        {
            var arguments = BuildArguments(argument, expectedValue);

            Assert.Equal(expectedValue, arguments.VirtualDirectory);
        }
    }
}
