using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Pretzel.Logic.Import;

namespace Pretzel.Tests.Import
{
    public class HtmlToMarkdownConverterTests
    {
        private HtmlToMarkdownConverter converter = new HtmlToMarkdownConverter();

        [Fact]
        public void Plain_text_is_copied_straight_through()
        {
            string markdown = converter.Convert("hello world");
            Assert.Equal("hello world", markdown);
        }

        [Fact]
        public void H1_headings_are_converted()
        {
            string markdown = converter.Convert("<h1>hello world</h1>");
            Assert.Equal(Environment.NewLine + "# hello world" + Environment.NewLine, markdown);
        }
        
        [Fact]
        public void H2_headings_are_converted()
        {
            string markdown = converter.Convert("<h2>heading 2</h2>");
            Assert.Equal(Environment.NewLine + "## heading 2" + Environment.NewLine, markdown);
        }

        [Fact]
        public void Paragraphs_have_a_new_line_before_and_after()
        {
            string markdown = converter.Convert("<p>Paragraph 1</p>");
            Assert.Equal(Environment.NewLine + "Paragraph 1" + Environment.NewLine, markdown);
        }

        [Fact]
        public void Links_are_converted()
        {
            string markdown = converter.Convert("<a href=\"http://foo.com/bar\">Link text</a>");
            Assert.Equal("[Link text](http://foo.com/bar)", markdown);
        }

        [Fact]
        public void Links_without_href_are_left_unconverted()
        {
            string markdown = converter.Convert("<a name=\"OLE_LINK1\"><em>This</em></a>");
            Assert.Equal("<a name=\"OLE_LINK1\"><em>This</em></a>", markdown);
        }

        [Fact]
        public void Images_are_left_unconverted()
        {
            string markdown = converter.Convert("<img src=\"http://foo.com/bar\">");
            Assert.Equal("<img src=\"http://foo.com/bar\">", markdown);
        }

        [Fact]
        public void Code_blocks_are_converted()
        {
            string markdown = converter.Convert("<pre>hello</pre>");
            Assert.Equal(Environment.NewLine + "    hello", markdown);
        }

        [Fact]
        public void Unordered_lists_are_converted()
        {
            string markdown = converter.Convert("<ul><li>first</li><li>second</li></ul>");
            Assert.Equal(Environment.NewLine  + Environment.NewLine + "* first" + Environment.NewLine + "* second" + Environment.NewLine, markdown);
        }
        
        [Fact]
        public void Unordered_lists_can_be_nested()
        {
            string markdown = converter.Convert("<ul><li>first</li><li>second</li><ul><li>second nested</li></ul></ul>");
            Assert.Contains(Environment.NewLine + "    * second nested" + Environment.NewLine, markdown);
        }

        [Fact]
        public void Unordered_lists_can_be_nested_inside_li()
        {
            string markdown = converter.Convert("<ul><li>first</li><li>second<li>second nested</li></li></ul>");
            Assert.Contains(Environment.NewLine + "    * second nested" + Environment.NewLine, markdown);
        }

        [Fact]
        public void Code_blocks_with_new_lines_are_converted()
        {
            string markdown = converter.Convert("<pre>hello" + Environment.NewLine + "world</pre>");
            Assert.Equal(Environment.NewLine + "    hello" + Environment.NewLine + "    world", markdown);
        }
    }
}
