using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DotLiquid;
using Pretzel.Logic.Templating.Plugins;
using Xunit;

namespace Pretzel.Tests.Templating.Plugins
{
    public class TagCloudTests
    {
        [Fact]
        public void NoContext_Always_LeavesTextWriterAlone()
        {
            var tag = new TagCloud();
            Context context = new Context();
            var writer = new StringWriter();
            tag.Render(context, writer);

            Assert.True(string.IsNullOrWhiteSpace(writer.ToString()));
        }
    }
}
