using System.Collections.Generic;
using Pretzel.Logic.Templating.Context;
using Pretzel.Logic.Templating.Jekyll;
using Xunit;

namespace Pretzel.Tests.Templating.Jekyll
{
    public class SiteCategoryTests
    {
        const string category = "foo";
        const string template = "Hello world \r\n {% for post in site.categories.foo %}{{post.content}}{% endfor %}";
        const string sourceFolder = @"C:\blog";

        public class ContainsCategoryAndOnePost : BakingEnvironment<LiquidEngine>
        {
            SiteContext context;

            string text = "This is a post";

            public override LiquidEngine Given()
            {
                context = new SiteContext();

                var post = new Page { File = "some-post.html", Content = text };
                var index = new Page { File = "index.html", Content = template };
                context.SourceFolder = sourceFolder;
                context.Pages.Add(post);
                context.Pages.Add(index);
                context.Categories = new List<Category>
                                         {
                                             new Category { Name = category, Posts = new[] { post } }
                                         };

                return new LiquidEngine { FileSystem = FileSystem };
            }

            public override void When()
            {
                Subject.Process(context);
            }

            [Fact]
            public void The_Index_Renders_The_Post_Contents()
            {
                var contents = FileSystem.File.ReadAllText(sourceFolder + @"\_site\index.html");
                Assert.True(contents.Contains(text));
            }
        }

        public class ContainsCategoryAndMultiplePosts : BakingEnvironment<LiquidEngine>
        {
            SiteContext context;

            string firstText = "This is a post";
            string secondText = "This is another post";

            public override LiquidEngine Given()
            {
                context = new SiteContext();

                var secondPost = new Page { File = "another-post.html", Content = firstText };
                var firstPost = new Page { File = "some-post.html", Content = secondText };
                var index = new Page { File = "index.html", Content = template };

                context.SourceFolder = sourceFolder;
                context.Pages.Add(firstPost);
                context.Pages.Add(secondPost);
                context.Pages.Add(index);
                context.Categories = new List<Category> { new Category { Name = category, Posts = new[] { firstPost, secondPost } } };

                return new LiquidEngine { FileSystem = FileSystem };
            }

            public override void When()
            {
                Subject.Process(context);
            }

            [Fact]
            public void The_Index_Renders_Both_Post_Contents()
            {
                var contents = FileSystem.File.ReadAllText(sourceFolder + @"\_site\index.html");
                Assert.True(contents.Contains(firstText));
                Assert.True(contents.Contains(secondText));
            }
        }

        public class ContainsNoCategories : BakingEnvironment<LiquidEngine>
        {
            SiteContext context;

            string firstText = "This is a post";
            string secondText = "This is another post";

            public override LiquidEngine Given()
            {
                context = new SiteContext();

                var firstPost = new Page { File = "some-post.html", Content = secondText };
                var index = new Page { File = "index.html", Content = template };

                context.SourceFolder = sourceFolder;
                context.Pages.Add(firstPost);
                context.Pages.Add(index);
                
                return new LiquidEngine { FileSystem = FileSystem };
            }

            public override void When()
            {
                Subject.Process(context);
            }

            [Fact]
            public void The_Index_Renders_The_Post_Contents()
            {
                var contents = FileSystem.File.ReadAllText(sourceFolder + @"\_site\index.html");
                Assert.False(contents.Contains(firstText));
            }
        }
    }
}