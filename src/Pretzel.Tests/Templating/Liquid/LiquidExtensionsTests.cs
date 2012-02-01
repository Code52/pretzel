using Pretzel.Logic.Templating.Liquid;
using Xunit;

namespace Pretzel.Tests.Templating.Liquid
{
    public class LiquidExtensionsTests
    {
        [Fact]
        public void IsMarkdownFile_ForExpectedExtensions_ReturnsTrue()
        {
            Assert.True(".md".IsMarkdownFile());
            Assert.True(".markdown".IsMarkdownFile());
            Assert.True(".mdown".IsMarkdownFile());
        }
    }
}
