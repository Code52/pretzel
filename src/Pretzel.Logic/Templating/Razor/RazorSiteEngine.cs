using System.ComponentModel.Composition;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Logic.Templating.Razor
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [SiteEngineInfo(Engine = "razor")]
    public class RazorSiteEngine : JekyllEngineBase
    {
        public override void Initialize()
        {
        }

        protected override void PreProcess()
        {
        }

        protected override string LayoutExtension
        {
            get { return ".cshtml"; }
        }

        protected override string RenderTemplate(string content, PageContext pageData)
        {
            return RazorEngine.Razor.Parse(content, pageData);
        }
    }
}
