using Pretzel.Logic.Templating;
using Pretzel.Logic.Templating.Context;
using System;
using Xunit;

namespace Pretzel.Tests.Templating
{
    public class JekyllEngineBaseTest
    {
        [Fact]
        public void CanProcess_Without_SiteEnginInfoAttribute_Should_Return_False()
        {
            var engine = new DummyEngine();
            var context = new SiteContext();
            Assert.False(engine.CanProcess(context));
        }

        private class DummyEngine : JekyllEngineBase
        {
            public override void Initialize()
            {
                throw new NotImplementedException();
            }

            protected override void PreProcess()
            {
                throw new NotImplementedException();
            }

            protected override string RenderTemplate(string content, PageContext pageData)
            {
                throw new NotImplementedException();
            }
        }
    }
}
