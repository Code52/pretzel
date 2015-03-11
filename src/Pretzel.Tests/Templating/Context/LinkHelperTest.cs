using Pretzel.Logic.Templating.Context;
using System;
using System.Linq;
using Xunit;
using Xunit.Extensions;

namespace Pretzel.Tests.Templating.Context
{
    public class LinkHelperTest
    {
        public LinkHelper LinkHelper { get; private set; }

        public LinkHelperTest()
        {
            LinkHelper = new LinkHelper();
        }

        [Fact]
        public void GetTitle_returns_original_value_when_no_timestamp()
        {
            var result = LinkHelper.GetTitle(@"C:\temp\foobar_baz.md");
            Assert.Equal("foobar_baz", result);
        }

        [Fact]
        public void GetTitle_returns_strips_timestamp()
        {
            var result = LinkHelper.GetTitle(@"C:\temp\2012-01-03-foobar_baz.md");
            Assert.Equal("foobar_baz", result);
        }

        [Fact]
        public void GetTitle_preserves_dash_separated_values_that_arent_timestamps()
        {
            var result = LinkHelper.GetTitle(@"C:\temp\foo-bar-baz-qak-foobar_baz.md");
            Assert.Equal("foo-bar-baz-qak-foobar_baz", result);
        }

        [InlineData(@"C:\TestSite\_site\about.md", "/about.html")]
        [InlineData(@"C:\TestSite\_site\about.mkd", "/about.html")]
        [InlineData(@"C:\TestSite\_site\about.mkdn", "/about.html")]
        [InlineData(@"C:\TestSite\_site\about.mdown", "/about.html")]
        [InlineData(@"C:\TestSite\_site\about.markdown", "/about.html")]
        [InlineData(@"C:\TestSite\_site\about.textile", "/about.html")]
        [InlineData(@"C:\TestSite\_site\rss.xml", "/rss.xml")]
        [InlineData(@"C:\TestSite\_site\relativepath\about.md", "/relativepath/about.html")]
        [Theory]
        public void EvaluateLink_url_is_well_formatted(string filePath, string expectedUrl)
        {
            var siteContext = new SiteContext { OutputFolder = @"C:\TestSite\_site" };
            var page = new Page { Filepath = filePath };

            Assert.Equal(expectedUrl, LinkHelper.EvaluateLink(siteContext, page));
        }

        [InlineData("date", "/cat1/cat2/2015/03/09/foobar-baz.html", "cat1,cat2")]
        [InlineData("date", "/2015/03/09/foobar-baz.html", "")]
        [InlineData("/:dashcategories/:year/:month/:day/:title.html", "/cat1-cat2/2015/03/09/foobar-baz.html", "cat1,cat2")]
        [InlineData("/:dashcategories/:year/:month/:day/:title.html", "/2015/03/09/foobar-baz.html", "")]
        [InlineData("pretty", "/cat1/cat2/2015/03/09/foobar-baz/", "cat1,cat2")]
        [InlineData("ordinal", "/cat1/cat2/2015/068/foobar-baz.html", "cat1,cat2")]
        [InlineData("none", "/cat1/cat2/foobar-baz.html", "cat1,cat2")]
        [InlineData("/:categories/:short_year/:i_month/:i_day/:title.html", "/cat1/cat2/15/3/9/foobar-baz.html", "cat1,cat2")]
        [InlineData("/:category/:title.html", "/cat1/foobar-baz.html", "cat1,cat2")]
        [InlineData("/:category/:title.html", "/foobar-baz.html", "")]
        [InlineData("/:category1/:title.html", "/cat1/foobar-baz.html", "cat1,cat2")]
        [InlineData("/:category2/:title.html", "/cat2/foobar-baz.html", "cat1,cat2")]
        [InlineData("/:category3/:title.html", "/foobar-baz.html", "cat1,cat2")]
        [Theory]
        public void EvaluatePermalink_url_is_well_formatted(string permalink, string expectedUrl, string categories)
        {
            var page = new Page
            {
                Categories = categories == null ? Enumerable.Empty<string>() : categories.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries),
                Date = new DateTime(2015, 03, 09),
                File = @"C:\temp\2015-03-09-foobar-baz.md"
            };

            Assert.Equal(expectedUrl, LinkHelper.EvaluatePermalink(permalink, page));
        }
    }
}
