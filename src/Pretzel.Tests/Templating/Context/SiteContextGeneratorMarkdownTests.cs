using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Templating.Context;
using Xunit;

namespace Pretzel.Tests.Templating.Context
{
    public class SiteContextGeneratorMarkdownTests
    {
        private readonly SiteContextGenerator generator;
        private readonly MockFileSystem fileSystem;

        private const string CodeBlock = "```js\r\nfunction() test{\r\n    Console.log(\"test\");\r\n}\r\n```";
        private const string ColorizedCodeBlock = "<div class=\"highlight\"><pre><span class=\"kd\">function</span><span class=\"p\">()</span> <span class=\"nx\">test</span><span class=\"p\">{</span>\n    <span class=\"nx\">Console</span><span class=\"p\">.</span><span class=\"nx\">log</span><span class=\"p\">(</span><span class=\"s2\">&quot;test&quot;</span><span class=\"p\">);</span>\n<span class=\"p\">}</span>\n</pre></div>";
         
        public SiteContextGeneratorMarkdownTests()
        {
            fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
            generator = new SiteContextGenerator(fileSystem, Enumerable.Empty<IContentTransform>());
        }

        [Fact]
        public void Single_block_is_converted_to_code()
        {
            const string input = "hello\r\n" + CodeBlock;
            const string expected = "<p>hello</p>\n" + ColorizedCodeBlock;

            fileSystem.AddFile(@"C:\TestSite\_posts\2012-01-01-SomeFile.md", new MockFileData(input));

            var siteContext = generator.BuildContext(@"C:\TestSite");

            Assert.Equal(expected, siteContext.Posts[0].Content.Trim());
        }

        [Fact]
        public void Multiple_blocks_are_converted_to_code()
        {
            const string input = "hello\r\n" + CodeBlock + "\r\nis it me you're looking for?\r\n" + CodeBlock +"\r\n";
            const string expected = "<p>hello</p>\n" + ColorizedCodeBlock + "\n<p>is it me you're looking for?</p>\n" + ColorizedCodeBlock;

            fileSystem.AddFile(@"C:\TestSite\_posts\2012-01-01-SomeFile.md", new MockFileData(input));

            var siteContext = generator.BuildContext(@"C:\TestSite");

            Assert.Equal(expected, siteContext.Posts[0].Content.Trim());
        }       
    }
}