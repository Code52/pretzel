using System;
using Pretzel.Logic.Extensions;
using Xunit;

namespace Pretzel.Tests.Extensions
{
    public class StringExtensionTests
    {
        [Fact]
        public void MimeType_ForImageFile_ReturnsImageJpeg()
        {
            const string fileName = "index.jpg";
            var mimeType = fileName.MimeType();
            Assert.Equal("image/jpeg", mimeType);
        }

        [Fact]
        public void MimeType_ForFileWithoutExtension_ReturnsTextHtml()
        {
            const string fileName = "index";
            var mimeType = fileName.MimeType();
            Assert.Equal("text/html", mimeType);
        }

        [Fact]
        public void MimeType_ForUnknownType_ReturnsTextHtml()
        {
            const string fileName = "index.jgaojgaogjapojgsa";
            var mimeType = fileName.MimeType();
            Assert.Equal("application/octet-stream", mimeType);
        }

        [Fact]
        public void MimeType_ForSpeicificCases_ReturnsTrue()
        {
            Assert.True("image/jpeg".IsBinaryMime());
            Assert.True("video/x-ms-wmv".IsBinaryMime());
            Assert.True("application/x-shockwave-flash".IsBinaryMime());
            Assert.True("audio/mpeg".IsBinaryMime());

            Assert.False("text/html".IsBinaryMime());
        }

        [Fact]
        public void Datestamp_WhenFullPathIncluded_ReturnsExpectedValues()
        {
            var output = "C:\\SomeWebsite\\2012-01-02-hello-world.md".Datestamp();
            Assert.Equal(2012, output.Year);
            Assert.Equal(1, output.Month);
            Assert.Equal(2, output.Day);
        }

        [Fact]
        public void Datestamp_WhenRelativePathIncluded_ReturnsExpectedValues()
        {
            var output = "SomeWebsite\\2012-01-02-hello-world.md".Datestamp();
            Assert.Equal(2012, output.Year);
            Assert.Equal(1, output.Month);
            Assert.Equal(2, output.Day);
        }

        [Fact]
        public void Datestamp_WhenNoDateTimeSet_ReturnsDateTimeNow()
        {
            var now = DateTime.Now;
            var output = "SomeWebsite\\hello-world.md".Datestamp();
            Assert.Equal(now.Year, output.Year);
            Assert.Equal(now.Month, output.Month);
            Assert.Equal(now.Day, output.Day);
        }

        [Fact]
        public void Datestamp_WithIncorrectFormat_ThrowsException()
        {
            Assert.Throws<FormatException>(() => "C:\\SomeWebsite\\2012-01-hello.md".Datestamp());
        }

        [Fact]
        public void Datestamp_WithoutDirectorySet_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => "2012-01-02-hello-world.md".Datestamp());
        }
    }
}