using System;
using System.ComponentModel.Composition;
using DotLiquid;
using Pretzel.Logic.Templating.Context;
using Pretzel.Logic.Templating.Jekyll.Liquid;

namespace Pretzel.Logic.Templating.Jekyll
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [SiteEngineInfo(Engine = "jekyll")]
    public class JekyllEngine : SiteEngineBase
    {
        SiteContextDrop contextDrop;

        public JekyllEngine()
        {
            DotLiquid.Liquid.UseRubyDateFormat = true;
        }

        protected override void PreProcess()
        {
            contextDrop = new SiteContextDrop(Context);
        }

        Hash CreatePageData(PageContext pageContext)
        {
            var y = Hash.FromDictionary(pageContext.Bag);

            if (y.ContainsKey("title"))
            {
                if (string.IsNullOrWhiteSpace(y["title"].ToString()))
                {
                    y["title"] = Context.Title;
                }
            }
            else
            {
                y.Add("title", Context.Title);
            }

            var x = Hash.FromAnonymousObject(new
            {
                site = contextDrop.ToHash(),
                wtftime = Hash.FromAnonymousObject(new { date = DateTime.Now }),
                page = y,
                content = pageContext.Content,//)//Markdown.Transform(contents.ExcludeHeader()),
            });

            if (Context.Config.ContainsKey("paginate") && pageContext.OutputPath.EndsWith("index.html"))
            {
                x.Add("paginator", new Paginator(Context));
            }
            return x;
        }

        protected override string RenderTemplate(string templateContents, PageContext pageData)
        {
            var data = CreatePageData(pageData);
            var template = Template.Parse(templateContents);
            Template.FileSystem = new Includes(Context.SourceFolder);
            return template.Render(data);
        }

        public override void Initialize()
        {
            //Template.RegisterTag<RenderTime>("render_time");
            //Template.RegisterTag<TagCloud>("tag_cloud");
        }
    }
}
