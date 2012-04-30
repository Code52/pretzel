using System.Collections.Generic;
using Pretzel.Logic.Templating.Context;
using Pretzel.Logic.Templating.Jekyll;
using Xunit;

namespace Pretzel.Tests.Templating.Jekyll
{
    public class PostCategoryTests
    {
        const string Category = "foo";
        const string OtherCategory = "bar";
        const string SourceFolder = @"C:\blog";
        const string Text = "This is a post. {% for category in page.categories %}{{ category }}{% endfor %}";

        public class ContainsCategory : BakingEnvironment<LiquidEngine>
        {
            readonly SiteContext context = new SiteContext();
            
            public override LiquidEngine Given()
            {
                var post = new Page
                               {
                                   Bag = new Dictionary<string, object> { { "categories", new[] { Category } } },
                                   File = "some-post.html", 
                                   Content = Text, Categories = new[] { Category }
                               };
                context.SourceFolder = SourceFolder;
                context.Pages.Add(post);

                return new LiquidEngine { FileSystem = FileSystem };
            }

            public override void When()
            {
                Subject.Process(context);
            }

            [Fact]
            public void The_Index_Renders_The_Category_Label()
            {
                var contents = FileSystem.File.ReadAllText(SourceFolder + @"\_site\some-post.html");
                Assert.True(contents.Contains(Category));
            }
        }

        public class ContainsMultipleCategory : BakingEnvironment<LiquidEngine>
        {
            readonly SiteContext context = new SiteContext();
            
            public override LiquidEngine Given()
            {
                var post = new Page
                {
                    Bag = new Dictionary<string, object> { { "categories", new[] { Category, OtherCategory } } },
                    File = "some-post.html",
                    Content = Text,
                    Categories = new[] { Category }
                };
                context.SourceFolder = SourceFolder;
                context.Pages.Add(post);

                return new LiquidEngine { FileSystem = FileSystem };
            }

            public override void When()
            {
                Subject.Process(context);
            }

            [Fact]
            public void The_Index_Renders_Both_Category_Labels()
            {
                var contents = FileSystem.File.ReadAllText(SourceFolder + @"\_site\some-post.html");
                Assert.True(contents.Contains(Category));
                Assert.True(contents.Contains(OtherCategory));
            }
        }
    }
}
