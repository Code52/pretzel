using Pretzel.Logic.Templating.Context;
using Xunit;

namespace Pretzel.Tests.Templating.Razor
{
    public class RazorExtensionsTests
    {
        [Fact]
        public void IsMarkdownFile_ForExpectedExtensions_ReturnsTrue()
        {
            Assert.True(".cshtml".IsRazorFile());
        }
    }
}
