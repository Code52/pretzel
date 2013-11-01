using Pretzel.Logic.Extensibility.Extensions;
using Pretzel.Tests.Templating.Jekyll;
using Xunit;

namespace Pretzel.Tests.Extensibility.Extensions
{
    public class FencedCodeBlocksTests
    {
        private readonly FencedCodeBlocks transform = new FencedCodeBlocks();
        private const string CodeBlock = "```js\r\nfunction() test{\r\n    Console.log(\"test\");\r\n}\r\n```";
        private const string ColorizedCodeBlock = "<div class=\"highlight\"><pre><span class=\"kd\">function</span><span class=\"p\">()</span> <span class=\"nx\">test</span><span class=\"p\">{</span>    <span class=\"nx\">Console</span><span class=\"p\">.</span><span class=\"nx\">log</span><span class=\"p\">(</span><span class=\"s2\">&quot;test&quot;</span><span class=\"p\">);</span><span class=\"p\">}</span></pre></div>";

        [Fact]
        public void Single_block_is_converted_to_code()
        {
            const string input = "<p>hello</p>\r\n" + CodeBlock;
            const string expected = "<p>hello</p>\r\n" + ColorizedCodeBlock;

            var markdown = transform.Transform(input);
            Assert.Equal(expected.RemoveWhiteSpace(), markdown.RemoveWhiteSpace());
        }

        [Fact]
        public void Multiple_blocks_are_converted_to_code()
        {
            const string input = "<p>hello</p>\r\n" + CodeBlock + "\r\n<p>is it me you're looking for?</p>\r\n" + CodeBlock +"\r\n";

            const string expected = "<p>hello</p>\r\n" + ColorizedCodeBlock + "\r\n<p>is it me you're looking for?</p>\r\n" + ColorizedCodeBlock + "\r\n";

            var markdown = transform.Transform(input);
            Assert.Equal(expected.RemoveWhiteSpace(), markdown.RemoveWhiteSpace());
        }
    }
}