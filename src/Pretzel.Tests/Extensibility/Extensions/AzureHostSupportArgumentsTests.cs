using Pretzel.Logic.Extensibility.Extensions;
using Xunit;

namespace Pretzel.Tests.Extensibility.Extensions
{
    public class AzureHostSupportArgumentsTests : CommandArgumentsExtensionTests<AzureHostSupportArguments>
    {
        protected override AzureHostSupportArguments CreateArguments() => new AzureHostSupportArguments();

        [Theory]
        [InlineData("", false)]
        [InlineData("--azure", true)]
        [InlineData("-azure", true)]
        public void Azure(string argument, bool expectedValue)
        {
            var arguments = BuildArguments(argument);

            Assert.Equal(expectedValue, arguments.Azure);
        }
    }
}
