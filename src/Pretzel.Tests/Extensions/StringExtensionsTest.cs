using Pretzel.Logic.Extensions;
using Xunit;
using Xunit.Extensions;

namespace Pretzel.Tests.Extensions
{
    public class StringExtensionsTest
    {
        [Fact]
        public void MimeType_default_without_extension_returns_html()
        {
            Assert.Equal("text/html", "test".MimeType());
        }

        [Fact]
        public void MimeType_default_with_extension_returns_html()
        {
            Assert.Equal("application/octet-stream", "test.unknown".MimeType());
        }

        [Fact]
        public void MimeType_scala_returns_expected()
        {
            Assert.Equal("text/x-scala", "test.scala".MimeType());
        }

        [Fact]
        public void MimeType_silo_returns_expected()
        {
            Assert.Equal("model/mesh", "test.silo".MimeType());
        }

        [Fact]
        public void MimeType_pdb_returns_expected()
        {
            Assert.Equal("chemical/x-pdb", "test.pdb".MimeType());
        }

        [Fact]
        public void MimeType_amr_returns_expected()
        {
            Assert.Equal("audio/amr", "test.amr".MimeType());
        }

        [Fact]
        public void MimeType_php5_returns_expected()
        {
            Assert.Equal("application/x-httpd-php5", "test.php5".MimeType());
        }

        [Fact]
        public void MimeType_ott_returns_expected()
        {
            Assert.Equal("application/vnd.oasis.opendocument.text-template", "test.ott".MimeType());
        }

        [Fact]
        public void MimeType_bin_returns_expected()
        {
            Assert.Equal("application/octet-stream", "test.bin".MimeType());
        }

        [Fact]
        public void MimeType_anx_returns_expected()
        {
            Assert.Equal("application/annodex", "test.anx".MimeType());
        }

        [Fact]
        public void IsBinaryMime_ice_returns_false()
        {
            Assert.False("test.ice".MimeType().IsBinaryMime());
        }

        [Fact]
        public void IsBinaryMime_avi_returns_true()
        {
            Assert.True("test.avi".MimeType().IsBinaryMime());
        }

        [Fact]
        public void IsBinaryMime_css_returns_false()
        {
            Assert.False("test.css".MimeType().IsBinaryMime());
        }

        [Fact]
        public void IsBinaryMime_ico_returns_true()
        {
            Assert.True("test.ico".MimeType().IsBinaryMime());
        }

        [Fact]
        public void IsBinaryMime_gamin_returns_false()
        {
            Assert.False("test.gamin".MimeType().IsBinaryMime());
        }

        [Fact]
        public void IsBinaryMime_mp3_returns_true()
        {
            Assert.True("test.mp3".MimeType().IsBinaryMime());
        }

        [Fact]
        public void IsBinaryMime_pgp_returns_true()
        {
            Assert.True("test.pgp".MimeType().IsBinaryMime());
        }

        [InlineData("CamelCase", "camel_case")]
        [InlineData("Camel_Case", "camel__case")]
        [InlineData("camelcase", "camelcase")]
        [InlineData("pascalCase", "pascal_case")]
        [Theory]
        public void ToUnderscoreCase_should_convert(string input, string expectedResult)
        {
            Assert.Equal(expectedResult, input.ToUnderscoreCase());
        }
    }
}
