using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Pretzel.Logic.Import;

namespace Pretzel.Tests.Import
{
    public class XhtmlToMarkdownConverterTests
    {
        [Fact]
        public void P_elements_are_converted()
        {
            string md = XhtmlToMarkdownConverter.Convert("<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\"><head><title>blah</title></head><body><h1>Hello world</h1><p>Something</p>Paragraph 2</body></html>");
            Assert.Equal("Hello world", md);
        }
    }
}
