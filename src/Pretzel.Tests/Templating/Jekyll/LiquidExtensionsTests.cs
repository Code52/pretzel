using Pretzel.Logic.Templating;
using Pretzel.Logic.Templating.Context;
using Xunit;

namespace Pretzel.Tests.Templating.Jekyll
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

        [Fact]
        public void Permalink_WithLeadingSlash_SwitchestoBackslash()
        {
            Assert.Equal("index.html", "index.html".ToRelativeFile());
            Assert.Equal(@"index.html", "/index.html".ToRelativeFile());
            Assert.Equal(@"folder\index.html", "/folder/index.html".ToRelativeFile());
        }
    }
}
