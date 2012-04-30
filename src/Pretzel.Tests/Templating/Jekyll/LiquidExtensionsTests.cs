using Pretzel.Logic.Templating.Context;
using Xunit;

namespace Pretzel.Tests.Templating.Jekyll
{
    public class LiquidExtensionsTests
    {
        [Fact]
        public void IsMarkdownFile_ForMdExtension_ReturnsTrue()
        {
            Assert.True(".md".IsMarkdownFile());
        }

        [Fact]
        public void IsMarkdownFile_ForMarkdownExtension_ReturnsTrue()
        {
            Assert.True(".markdown".IsMarkdownFile());
        }

        [Fact]
        public void IsMarkdownFile_ForMdownExtension_ReturnsTrue()
        {
            Assert.True(".mdown".IsMarkdownFile());
        }

        [Fact]
        public void Permalink_WithNoSlash_DoesNotModify()
        {
            Assert.Equal("index.html", "index.html".ToRelativeFile());
        }

        [Fact]
        public void Permalink_WithLeadingSlash_SwitchestoBackslash()
        {
            Assert.Equal(@"index.html", "/index.html".ToRelativeFile());
        }

        [Fact]
        public void Permalink_WithInternalSlash_SwitchestoBackslash()
        {
            Assert.Equal(@"folder\index.html", "/folder/index.html".ToRelativeFile());
        }
    }
}
