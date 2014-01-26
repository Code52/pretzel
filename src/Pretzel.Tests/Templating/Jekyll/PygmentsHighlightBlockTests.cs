using System.Collections.Generic;
using Pretzel.Logic.Liquid;
using Pygments;
using Xunit;

namespace Pretzel.Tests.Templating.Jekyll
{
    public class When_PygmentsHighlighting_Is_Used : BakingEnvironment<PygmentsHighlightBlock>
    {
        public override PygmentsHighlightBlock Given()
        {
            return new PygmentsHighlightBlock();
        }

        public override void When()
        {
            Subject.Initialize("highlight", "", new List<string> { "{% endhighlight %}" });
        }

        [Fact]
        public void Lexer_Defaults_To_Null()
        {
            Assert.Null(Subject.LexerName);
        }

        [Fact]
        public void LineNumbers_Default_To_None()
        {
            Assert.Equal(Subject.LineNumberStyle, LineNumberStyle.none);
        }
    }

    public class When_PygmentsHighlighting_Is_Used_With_Lexer : BakingEnvironment<PygmentsHighlightBlock>
    {
        public override PygmentsHighlightBlock Given()
        {
            return new PygmentsHighlightBlock();
        }

        public override void When()
        {
            Subject.Initialize("highlight", "c#", new List<string> { "{% endhighlight %}" });
        }

        [Fact]
        public void Lexer_Is_Set_To_CSharp()
        {
            Assert.Equal(Subject.LexerName, "c#");
        }

        [Fact]
        public void LineNumbers_Default_To_None()
        {
            Assert.Equal(Subject.LineNumberStyle, LineNumberStyle.none);
        }
    }

    public class When_PygmentsHighlighting_Is_Used_With_Lexer_And_Linenos : BakingEnvironment<PygmentsHighlightBlock>
    {
        public override PygmentsHighlightBlock Given()
        {
            return new PygmentsHighlightBlock();
        }

        public override void When()
        {
            Subject.Initialize("highlight", "c# linenos", new List<string> { "{% endhighlight %}" });
        }

        [Fact]
        public void Lexer_Is_Set_To_CSharp()
        {
            Assert.Equal(Subject.LexerName, "c#");
        }

        [Fact]
        public void LineNumbers_Is_Set_To_Inline()
        {
            Assert.Equal(Subject.LineNumberStyle, LineNumberStyle.inline);
        }
    }
}