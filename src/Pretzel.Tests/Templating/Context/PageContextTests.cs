using Pretzel.Logic;
using Pretzel.Logic.Templating.Context;
using System.Collections.Generic;
using Xunit;

namespace Pretzel.Tests.Templating.Context
{
    public class PageContextTests
    {
        [Fact]
        public void config_permalink_sets_relative_file_output_path()
        {
            var context = new SiteContext();
            var dict = new Dictionary<string, object>();
            context.Config = new ConfigurationMock(dict);
            dict.Add("permalink", "/blog/:year/:month/:day/:title.html");

            var page = new Page()
            {
                Url = "/blog/2010/08/21/title-of-my-post.html"
            };

            var outputPath = "c:\\temp";
            var defaultOutputPath = "c:\\default";

            var pageContext = PageContext.FromPage(context, page, outputPath, defaultOutputPath);

            Assert.Equal("c:\\temp\\blog\\2010\\08\\21\\title-of-my-post.html", pageContext.OutputPath);
        }

        [Fact]
        public void page_permalink_sets_relative_file_output_path()
        {
            var context = new SiteContext();
            context.Config = new Configuration();

            var page = new Page()
            {
                Url = "/blog/2010/08/21/title-of-my-post.html"
            };

            page.Bag = new Dictionary<string, object>();
            page.Bag.Add("permalink", "/blog/:year/:month/:day/:title.html");

            var outputPath = "c:\\temp";
            var defaultOutputPath = "c:\\default";

            var pageContext = PageContext.FromPage(context, page, outputPath, defaultOutputPath);

            Assert.Equal("c:\\temp\\blog\\2010\\08\\21\\title-of-my-post.html", pageContext.OutputPath);
        }

        [Fact]
        public void no_permalink_sets_default_output_path_and_page_bag_permalink()
        {
            var context = new SiteContext();
            context.Config = new Configuration();

            var file = "title-of-my-post.html";
            var page = new Page()
            {
                File = file
            };

            page.Bag = new Dictionary<string, object>();

            var outputPath = "c:\\temp";
            var defaultOutputPath = "c:\\default\\title-of-my-post.html";

            var pageContext = PageContext.FromPage(context, page, outputPath, defaultOutputPath);

            Assert.Equal("c:\\default\\title-of-my-post.html", pageContext.OutputPath);
            Assert.Equal(file, page.Bag["permalink"]);
        }
    }
}
