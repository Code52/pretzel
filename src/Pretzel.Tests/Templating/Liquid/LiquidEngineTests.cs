using System.IO.Abstractions.TestingHelpers;
using Pretzel.Logic.Templating.Liquid;
using Xunit;

namespace Pretzel.Tests.Templating.Liquid
{
    public static class TestExtensions
    {
        public static string RemoveWhiteSpace(this string s)
        {
            return s.Replace("\r\n", "").Replace("\n", "");
        }
    }

    public class LiquidEngineTests
    {
        public class When_Recieving_A_Folder_Containing_One_File : BakingEnvironment<LiquidEngine>
        {
            const string FileContents = "<html><head></head><body></body></html>";

            public override LiquidEngine Given()
            {
                return new LiquidEngine();
            }

            public override void When()
            {
                var context = new SiteContext {Folder = @"C:\website\"};
                FileSystem.AddFile(@"C:\website\index.html", new MockFileData(FileContents));
                Subject.Process(FileSystem, context);
            }

            [Fact]
            public void That_Site_Folder_Is_Created()
            {
                Assert.True(FileSystem.Directory.Exists(@"C:\website\_site"));
            }

            [Fact]
            public void The_File_Is_Added_At_The_Root()
            {
                Assert.True(FileSystem.File.Exists(@"C:\website\_site\index.html"));
            }

            [Fact]
            public void The_File_Is_Identical()
            {
                Assert.Equal(FileContents, FileSystem.File.ReadAllText(@"C:\website\_site\index.html"));
            }
        }

        public class When_Recieving_A_Folder_Containing_One_File_In_A_Subfolder : BakingEnvironment<LiquidEngine>
        {
            const string FileContents = "<html><head></head><body></body></html>";

            public override LiquidEngine Given()
            {
                return new LiquidEngine();
            }

            public override void When()
            {
                var context = new SiteContext { Folder = @"C:\website\" };
                FileSystem.AddFile(@"C:\website\content\index.html", new MockFileData(FileContents));
                Subject.Process(FileSystem,context);
            }

            [Fact]
            public void That_Child_Folder_Is_Created()
            {
                Assert.True(FileSystem.Directory.Exists(@"C:\website\_site\content"));
            }

            [Fact]
            public void The_File_Is_Added_At_The_Root()
            {
                Assert.True(FileSystem.File.Exists(@"C:\website\_site\content\index.html"));
            }

            [Fact]
            public void The_File_Is_Identical()
            {
                Assert.Equal(FileContents, FileSystem.File.ReadAllText(@"C:\website\_site\content\index.html"));
            }
        }

        public class When_Recieving_A_File_With_A_Template : BakingEnvironment<LiquidEngine>
        {
            const string FileContents = "<html><head><title>{{ page.title }}</title></head><body></body></html>";
            const string ExpectedfileContents = "<html><head><title>My Web Site</title></head><body></body></html>";

            public override LiquidEngine Given()
            {
                return new LiquidEngine();
            }

            public override void When()
            {
                var context = new SiteContext { Folder = @"C:\website\", Title = "My Web Site" };
                FileSystem.AddFile(@"C:\website\index.html", new MockFileData(FileContents));
                Subject.Process(FileSystem, context);
            }

            [Fact]
            public void The_File_Is_Applies_Data_To_The_Template()
            {
                Assert.Equal(ExpectedfileContents, FileSystem.File.ReadAllText(@"C:\website\_site\index.html"));
            }
        }

        public class When_Recieving_A_Markdown_File : BakingEnvironment<LiquidEngine>
        {
            const string TemplateContents = "<html><head><title>{{ page.title }}</title></head><body>{{ content }}</body></html>";
            const string PageContents = "---\r\n layout: default\r\n---\r\n\r\n# Hello World!";
            const string ExpectedfileContents = "<html><head><title>My Web Site</title></head><body><h1>Hello World!</h1></body></html>";

            public override LiquidEngine Given()
            {
                return new LiquidEngine();
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\_layouts\default.html", new MockFileData(TemplateContents));
                FileSystem.AddFile(@"C:\website\index.md", new MockFileData(PageContents));
                var context = new SiteContext { Folder = @"C:\website\", Title = "My Web Site" };
                Subject.Process(FileSystem, context);
            }

            [Fact]
            public void The_File_Is_Applies_Data_To_The_Template()
            {
                Assert.Equal(ExpectedfileContents, FileSystem.File.ReadAllText(@"C:\website\_site\index.html").RemoveWhiteSpace());
            }

            [Fact]
            public void Does_Not_Copy_Template_To_Output()
            {
                Assert.False(FileSystem.File.Exists(@"C:\website\_site\_layouts\default.html"));
            }
        }

        public class When_A_Template_Also_Has_A_Layout_Value : BakingEnvironment<LiquidEngine>
        {
            const string ParentTemplateContents = "<html><head><title>{{ page.title }}</title></head><body>{{ content }}</body></html>";
            const string InnerTemplateContents = "---\r\n layout: parent\r\n---\r\n\r\n<h1>Title</h1>{{content}}";
            const string PageContents = "---\r\n layout: inner\r\n---\r\n\r\n## Hello World!";
            const string ExpectedfileContents = "<html><head><title>My Web Site</title></head><body><h1>Title</h1><h2>Hello World!</h2></body></html>";

            public override LiquidEngine Given()
            {
                return new LiquidEngine();
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\_layouts\parent.html", new MockFileData(ParentTemplateContents));
                FileSystem.AddFile(@"C:\website\_layouts\inner.html", new MockFileData(InnerTemplateContents));
                FileSystem.AddFile(@"C:\website\index.md", new MockFileData(PageContents));

                var context = new SiteContext { Folder = @"C:\website\", Title = "My Web Site" };
                Subject.Process(FileSystem, context);
            }

            [Fact]
            public void The_File_Is_Applies_Data_To_The_Template()
            {
                Assert.Equal(ExpectedfileContents, FileSystem.File.ReadAllText(@"C:\website\_site\index.html").RemoveWhiteSpace());
            }

            [Fact]
            public void Does_Not_Copy_Template_To_Output()
            {
                Assert.False(FileSystem.File.Exists(@"C:\website\_site\_layouts\default.html"));
            }
        }

        public class Given_Markdown_Page_Has_A_Permalink : BakingEnvironment<LiquidEngine>
        {
            const string PageContents = "---\r\npermalink: /somepage.html\r\n---\r\n\r\n# Hello World!";

            public override LiquidEngine Given()
            {
                return new LiquidEngine();
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\index.md", new MockFileData(PageContents));
                var context = new SiteContext { Folder = @"C:\website\", Title = "My Web Site" };
                Subject.Process(FileSystem, context);
            }

            [Fact]
            public void The_File_Should_Be_At_A_Different_Path()
            {
                Assert.True(FileSystem.File.Exists(@"C:\website\_site\somepage.html"));
            }
        }

        public class Given_Markdown_Page_Has_A_Title : BakingEnvironment<LiquidEngine>
        {
            const string TemplateContents = "<html><head><title>{{ page.title }}</title></head><body>{{ content }}</body></html>";
            const string PageContents = "---\r\n layout: default \r\n title: 'A different title'\r\n---\r\n\r\n## Hello World!";
            const string ExpectedfileContents = "<html><head><title>A different title</title></head><body><h2>Hello World!</h2></body></html>";


            public override LiquidEngine Given()
            {
                return new LiquidEngine();
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\_layouts\default.html", new MockFileData(TemplateContents));
                FileSystem.AddFile(@"C:\website\index.md", new MockFileData(PageContents));
                var context = new SiteContext { Folder = @"C:\website\", Title = "My Web Site" };
                Subject.Process(FileSystem, context);
            }

            [Fact]
            public void The_Output_Should_Override_The_Site_Title()
            {
                Assert.Equal(ExpectedfileContents, FileSystem.File.ReadAllText(@"C:\website\_site\index.html").RemoveWhiteSpace());
            }
        }
    }
}