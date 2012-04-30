using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Templating.Context;
using Xunit;

namespace Pretzel.Tests.Templating.Jekyll
{
    public class SiteContextGeneratorTests
    {
        public class Given_Markdown_Page_Has_A_Permalink : BakingEnvironment<SiteContextGenerator>
        {
            SiteContext context;
            const string PageContents = "---\r\npermalink: /somepage.html\r\n---\r\n\r\n# Hello World!";

            public override SiteContextGenerator Given()
            {
                return new SiteContextGenerator(FileSystem, Enumerable.Empty<IContentTransform>());
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\index.md", new MockFileData(PageContents));
                context = Subject.BuildContext(@"C:\website");
            }

            [Fact]
            public void The_File_Should_Be_At_A_Different_Path()
            {
                Assert.True(context.Pages[0].Url.EndsWith("somepage.html"));
            }
        }

        public class When_A_Template_Also_Has_A_Layout_Value : BakingEnvironment<SiteContextGenerator>
        {

            const string ParentTemplateContents = "<html><head><title>{{ page.title }}</title></head><body>{{ content }}</body></html>";
            const string InnerTemplateContents = "---\r\n layout: parent\r\n---\r\n\r\n<h1>Title</h1>{{content}}";
            const string PageContents = "---\r\n layout: inner\r\n---\r\n\r\n## Hello World!";

            public override SiteContextGenerator Given()
            {
                return new SiteContextGenerator(FileSystem, Enumerable.Empty<IContentTransform>());
            }

            private SiteContext context;

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\_layouts\parent.html", new MockFileData(ParentTemplateContents));
                FileSystem.AddFile(@"C:\website\_layouts\inner.html", new MockFileData(InnerTemplateContents));
                FileSystem.AddFile(@"C:\website\index.md", new MockFileData(PageContents));

                context = Subject.BuildContext(@"C:\website\");
            }

            [Fact]
            public void The_File_Is_Appended_To_The_Site_Context()
            {
                Assert.Equal(1, context.Pages.Count);
                Assert.Equal("inner", context.Pages[0].Layout);
            }

            [Fact]
            public void Does_Not_Copy_Template_To_Output()
            {
                Assert.False(FileSystem.File.Exists(@"C:\website\_site\_layouts\parent.html"));
                Assert.False(FileSystem.File.Exists(@"C:\website\_site\_layouts\inner.html"));
            }
        }

        public class Given_Markdown_Page_Has_No_Title : BakingEnvironment<SiteContextGenerator>
        {
            SiteContext context;
            const string TemplateContents = "<html><head><title>{{ page.title }}</title></head><body>{{ content }}</body></html>";
            const string PageContents = "---\r\n layout: default \r\n---\r\n\r\n## Hello World!";
            const string ConfigFile = "title: My Title";

            public override SiteContextGenerator Given()
            {
                return new SiteContextGenerator(FileSystem, Enumerable.Empty<IContentTransform>());
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\_layouts\default.html", new MockFileData(TemplateContents));
                FileSystem.AddFile(@"C:\website\_config.yml", new MockFileData(ConfigFile));
                FileSystem.AddFile(@"C:\website\index.md", new MockFileData(PageContents));

                context = Subject.BuildContext(@"C:\website");
            }

            [Fact]
            public void The_Output_Should_Use_The_Site_Title()
            {
                Assert.Equal("My Title", context.Pages[0].Title);
            }
        }

        public class Given_Markdown_And_Site_Do_Not_Have_Title : BakingEnvironment<SiteContextGenerator>
        {
            SiteContext context;
            const string TemplateContents = "<html><head><title>{{ page.title }}</title></head><body>{{ content }}</body></html>";
            const string PageContents = "---\r\n layout: default \r\n---\r\n\r\n## Hello World!";
            const string ConfigFile = "";

            public override SiteContextGenerator Given()
            {
                return new SiteContextGenerator(FileSystem, Enumerable.Empty<IContentTransform>());
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\_layouts\default.html", new MockFileData(TemplateContents));
                FileSystem.AddFile(@"C:\website\_config.yml", new MockFileData(ConfigFile));
                FileSystem.AddFile(@"C:\website\index.md", new MockFileData(PageContents));

                context = Subject.BuildContext(@"C:\website");
            }

            [Fact]
            public void The_Output_Should_Have_An_Empty_Title()
            {
                Assert.Equal("", context.Pages[0].Title);
            }
        }

        public class Given_Markdown_Page_Has_A_Title : BakingEnvironment<SiteContextGenerator>
        {
            SiteContext context;
            const string TemplateContents = "<html><head><title>{{ page.title }}</title></head><body>{{ content }}</body></html>";
            const string PageContents = "---\r\n layout: default \r\n title: 'A different title'\r\n---\r\n\r\n## Hello World!";

            public override SiteContextGenerator Given()
            {
                return new SiteContextGenerator(FileSystem, Enumerable.Empty<IContentTransform>());
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\_layouts\default.html", new MockFileData(TemplateContents));
                FileSystem.AddFile(@"C:\website\index.md", new MockFileData(PageContents));

                context = Subject.BuildContext(@"C:\website");
            }

            [Fact]
            public void The_Output_Should_Override_The_Site_Title()
            {
                Assert.Equal("A different title", context.Pages[0].Title);
            }
        }

    }
}