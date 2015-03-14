using DotLiquid;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Liquid;
using Pretzel.Logic.Templating.Context;
using Pretzel.Logic.Templating.Jekyll.Liquid;
using System;
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;

namespace Pretzel.Logic.Templating.Jekyll
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [SiteEngineInfo(Engine = "liquid")]
    public class LiquidEngine : JekyllEngineBase
    {
        private SiteContextDrop contextDrop;
        private readonly Regex emHtmlRegex = new Regex(@"(?<=\{[\{\%].*?)(</?em>)(?=.*?[\%\}]\})", RegexOptions.Compiled);

        public LiquidEngine()
        {
            DotLiquid.Liquid.UseRubyDateFormat = true;
        }

        protected override void PreProcess()
        {
            contextDrop = new SiteContextDrop(Context);

            Template.FileSystem = new Includes(Context.SourceFolder, FileSystem);

            if (Filters != null)
            {
                foreach (var filter in Filters)
                {
                    Template.RegisterFilter(filter.GetType());
                }
            }
            if (Tags != null)
            {
                var registerTagMethod = typeof(Template).GetMethod("RegisterTag", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                foreach (var tag in Tags)
                {
                    var registerTagGenericMethod = registerTagMethod.MakeGenericMethod(new[] { tag.GetType() });
                    registerTagGenericMethod.Invoke(null, new[] { tag.Name.ToUnderscoreCase() });
                }
            }
        }

        private Hash CreatePageData(PageContext pageContext)
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
                content = pageContext.Content,
                paginator = pageContext.Paginator,
            });

            return x;
        }

        protected override string RenderTemplate(string templateContents, PageContext pageData)
        {
            // Replace all em HTML tags in liquid tags ({{ or {%) by underscores
            templateContents = emHtmlRegex.Replace(templateContents, "_");

            var data = CreatePageData(pageData);
            var template = Template.Parse(templateContents);
            var output = template.Render(data);

            return output;
        }

        public override void Initialize()
        {
            Template.RegisterFilter(typeof(XmlEscapeFilter));
            Template.RegisterFilter(typeof(DateToXmlSchemaFilter));
            Template.RegisterFilter(typeof(DateToLongStringFilter));
            Template.RegisterFilter(typeof(DateToRfc822FormatFilter));
            Template.RegisterFilter(typeof(CgiEscapeFilter));
            Template.RegisterFilter(typeof(UriEscapeFilter));
            Template.RegisterFilter(typeof(NumberOfWordsFilter));
            Template.RegisterTag<HighlightBlock>("highlight");
        }
    }
}
