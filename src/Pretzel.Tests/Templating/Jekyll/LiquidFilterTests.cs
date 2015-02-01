using Pretzel.Logic.Liquid;
using System;
using System.Xml;
using Xunit;

namespace Pretzel.Tests.Templating.Jekyll
{
    public class LiquidFilterTests
    {
        [Fact]
        public void DateToXmlSchema_ForExpectedDate_ReturnsCorrectString()
        {
            // "2014-01-01T01:00:00+01:00"
            var date = new DateTime(2014, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            Assert.Equal(XmlConvert.ToString(date, XmlDateTimeSerializationMode.Local), DateToXmlSchemaFilter.date_to_xmlschema(date));
        }

        [Fact]
        public void DateToRfc822_ForExpectedDate_ReturnsCorrectString()
        {
            Assert.Equal("Wed, 01 Jan 2014 00:00:00 +0000", DateToRfc822FormatFilter.date_to_rfc822(new DateTime(2014, 01, 01, 0, 0, 0, DateTimeKind.Utc)));
        }

        [Fact]
        public void DateToString_ForExpectedDate_ReturnsCorrectString()
        {
            var date = new DateTime(2008, 11, 07);
            Assert.Equal(date.ToString("dd MMM yyyy"), DateToStringFilter.date_to_string(date));
        }

        [Fact]
        public void DateToString_WithNoDate_ReturnsNothing()
        {
            Assert.Equal("", DateToStringFilter.date_to_string(""));
        }

        [Fact]
        public void DateToString_WithStringDate_ReturnsCorrectString()
        {
            var date = new DateTime(2008, 11, 07);
            Assert.Equal(date.ToString("dd MMM yyyy"), DateToStringFilter.date_to_string(date.ToString()));
        }

        [Fact]
        public void DateToLongString_ForExpectedDate_ReturnsCorrectString()
        {
            Assert.Equal("07 November 2008", DateToLongStringFilter.date_to_long_string(new DateTime(2008, 11, 07)));
        }

        [Fact]
        public void DateToLongString_ForExpectedStringDate_ReturnsCorrectString()
        {
            Assert.Equal("07 November 2008", DateToLongStringFilter.date_to_long_string(new DateTime(2008, 11, 07).ToString()));
        }

        [Fact]
        public void DateToLongString_ForUnexpectedStringDate_ReturnsStringEmpty()
        {
            Assert.Equal(string.Empty, DateToLongStringFilter.date_to_long_string("foo bar"));
        }

        [Fact]
        public void CgiEscape_ForGivenString_ReturnsEscapedString()
        {
            Assert.Equal("foo%2Cbar%3Bbaz%3F", CgiEscapeFilter.cgi_escape(@"foo,bar;baz?"));
        }

        [Fact]
        public void UriEscape_ForGivenString_ReturnsEscapedString()
        {
            Assert.Equal("foo,%20bar%20%5Cbaz?", UriEscapeFilter.uri_escape(@"foo, bar \baz?"));
        }

        [Fact]
        public void NumberOfWords_ForGiveString_ReturnsCorrectCount()
        {
            Assert.Equal(4.ToString(), NumberOfWordsFilter.number_of_words("This is a test"));
        }

        [Fact]
        public void XmlEscape_ForGiveString_ReturnsCorrectEscapedString()
        {
            Assert.Equal("&lt;test&gt;&quot;this is &amp; &apos;test&apos;&quot;&lt;/test&gt;", XmlEscapeFilter.xml_escape("<test>\"this is & 'test'\"</test>"));
        }
    }
}
