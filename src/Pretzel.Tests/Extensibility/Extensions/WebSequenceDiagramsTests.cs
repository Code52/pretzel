using Pretzel.Logic.Extensibility.Extensions;
using Pretzel.Tests.Templating.Jekyll;
using Xunit;

namespace Pretzel.Tests.Extensibility.Extensions
{
    public class WebSequenceDiagramsTests
    {
        readonly WebSequenceDiagrams transform = new WebSequenceDiagrams();

        [Fact]
        public void Single_block_is_converted_to_diagram()
        {
            const string input = @"<p>hello</p>
<pre><code>@@sequence
a->b: foo
b->a: bar
</code></pre>
<p>world</p>";

            const string expected = @"<p>hello</p>
<div class=""wsd"" wsd_style=""default""><pre>a->b: foo
b->a: bar
</pre></div>
<p>world</p>
<script type=""text/javascript"" src=""http://www.websequencediagrams.com/service.js""></script>";

            var markdown = transform.Transform(input);
            Assert.Equal(expected.RemoveWhiteSpace(), markdown.RemoveWhiteSpace());
        }

        [Fact]
        public void Multiple_blocks_are_converted_to_multiple_diagrams()
        {
            const string input = @"<p>hello</p>
<pre><code>@@sequence
a->b: foo
b->a: bar
</code></pre>
<p>world</p>
<pre><code>@@sequence
c->d: baz
d->c: qak
</code></pre>
<p>woo</p>";

            const string expected = @"<p>hello</p>
<div class=""wsd"" wsd_style=""default""><pre>a->b: foo
b->a: bar
</pre></div>
<p>world</p>
<div class=""wsd"" wsd_style=""default""><pre>c->d: baz
d->c: qak
</pre></div>
<p>woo</p>
<script type=""text/javascript"" src=""http://www.websequencediagrams.com/service.js""></script>";

            var markdown = transform.Transform(input);
            Assert.Equal(expected.RemoveWhiteSpace(), markdown.RemoveWhiteSpace());
        }

        [Fact]
        public void Single_block_is_converted_to_diagram_style_mscgen()
        {
            const string input = @"<p>hello</p>
<pre><code>@@sequence mscgen
a->b: foo
b->a: bar
</code></pre>
<p>world</p>";

            const string expected = @"<p>hello</p>
<div class=""wsd"" wsd_style=""mscgen""><pre>a->b: foo
b->a: bar
</pre></div>
<p>world</p>
<script type=""text/javascript"" src=""http://www.websequencediagrams.com/service.js""></script>";

            var markdown = transform.Transform(input);
            Assert.Equal(expected.RemoveWhiteSpace(), markdown.RemoveWhiteSpace());
        }
    }
}
