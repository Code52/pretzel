using System.IO.Abstractions.TestingHelpers;
using Pretzel.Logic.Templating.Jekyll;
using Xunit;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Tests.Templating.Jekyll
{
    public class JekyllEngineTests
    {
        public class When_Recieving_A_Folder_Containing_One_File : BakingEnvironment<JekyllEngine>
        {
            const string FileContents = "<html><head></head><body></body></html>";

            public override JekyllEngine Given()
            {
                return new JekyllEngine();
            }

            public override void When()
            {
                var context = new SiteContext {SourceFolder = @"C:\website\"};
                FileSystem.AddFile(@"C:\website\index.html", new MockFileData(FileContents));
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
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

        public class When_Recieving_A_Folder_Without_A_Trailing_Slash : BakingEnvironment<JekyllEngine>
        {
            const string FileContents = "<html><head></head><body></body></html>";

            public override JekyllEngine Given()
            {
                return new JekyllEngine();
            }

            public override void When()
            {
                var context = new SiteContext { SourceFolder = @"C:\website" };
                FileSystem.AddFile(@"C:\website\index.html", new MockFileData(FileContents));
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
            }

            [Fact]
            public void The_File_Is_Added_At_The_Root()
            {
                Assert.True(FileSystem.File.Exists(@"C:\website\_site\index.html"));
            }
        }

        public class When_Recieving_A_Folder_Containing_One_File_In_A_Subfolder : BakingEnvironment<JekyllEngine>
        {
            const string FileContents = "<html><head></head><body></body></html>";

            public override JekyllEngine Given()
            {
                return new JekyllEngine();
            }

            public override void When()
            {
                var context = new SiteContext { SourceFolder = @"C:\website\" };
                FileSystem.AddFile(@"C:\website\content\index.html", new MockFileData(FileContents));
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
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

        public class When_Recieving_A_File_Without_Metadata : BakingEnvironment<JekyllEngine>
        {
            const string FileContents = "<html><head><title>{{ page.title }}</title></head><body></body></html>";

            public override JekyllEngine Given()
            {
                return new JekyllEngine();
            }

            public override void When()
            {
                var context = new SiteContext { SourceFolder = @"C:\website\", Title = "My Web Site" };
                FileSystem.AddFile(@"C:\website\index.html", new MockFileData(FileContents));
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
            }

            [Fact]
            public void The_File_Is_Unaltered()
            {
                Assert.Equal(FileContents, FileSystem.File.ReadAllText(@"C:\website\_site\index.html"));
            }
        }

        public class When_Recieving_A_Markdown_File : BakingEnvironment<JekyllEngine>
        {
            const string TemplateContents = "<html><head><title>{{ page.title }}</title></head><body>{{ content }}</body></html>";
            const string PageContents = "---\r\n layout: default\r\n---\r\n\r\n# Hello World!";
            const string ExpectedfileContents = "<html><head><title>My Web Site</title></head><body><h1>Hello World!</h1></body></html>";

            public override JekyllEngine Given()
            {
                return new JekyllEngine();
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\_layouts\default.html", new MockFileData(TemplateContents));
                FileSystem.AddFile(@"C:\website\index.md", new MockFileData(PageContents));
                var context = new SiteContext { SourceFolder = @"C:\website\", Title = "My Web Site" };
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
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

        public class When_A_Template_Also_Has_A_Layout_Value : BakingEnvironment<JekyllEngine>
        {
            const string ParentTemplateContents = "<html><head><title>{{ page.title }}</title></head><body>{{ content }}</body></html>";
            const string InnerTemplateContents = "---\r\n layout: parent\r\n---\r\n\r\n<h1>Title</h1>{{content}}";
            const string PageContents = "---\r\n layout: inner\r\n---\r\n\r\n## Hello World!";
            const string ExpectedfileContents = "<html><head><title>My Web Site</title></head><body><h1>Title</h1><h2>Hello World!</h2></body></html>";

            public override JekyllEngine Given()
            {
                return new JekyllEngine();
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\_layouts\parent.html", new MockFileData(ParentTemplateContents));
                FileSystem.AddFile(@"C:\website\_layouts\inner.html", new MockFileData(InnerTemplateContents));
                FileSystem.AddFile(@"C:\website\index.md", new MockFileData(PageContents));

                var context = new SiteContext { SourceFolder = @"C:\website\", Title = "My Web Site" };
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
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

        public class Given_Markdown_Page_Has_A_Permalink : BakingEnvironment<JekyllEngine>
        {
            const string PageContents = "---\r\npermalink: /somepage.html\r\n---\r\n\r\n# Hello World!";

            public override JekyllEngine Given()
            {
                return new JekyllEngine();
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\index.md", new MockFileData(PageContents));
                var context = new SiteContext { SourceFolder = @"C:\website\", Title = "My Web Site" };
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
            }

            [Fact]
            public void The_File_Should_Be_At_A_Different_Path()
            {
                Assert.True(FileSystem.File.Exists(@"C:\website\_site\somepage.html"));
            }
        }

        public class Given_Markdown_Page_Has_A_Title : BakingEnvironment<JekyllEngine>
        {
            const string TemplateContents = "<html><head><title>{{ page.title }}</title></head><body>{{ content }}</body></html>";
            const string PageContents = "---\r\n layout: default \r\n title: 'A different title'\r\n---\r\n\r\n## Hello World!";
            const string ExpectedfileContents = "<html><head><title>A different title</title></head><body><h2>Hello World!</h2></body></html>";


            public override JekyllEngine Given()
            {
                return new JekyllEngine();
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\_layouts\default.html", new MockFileData(TemplateContents));
                FileSystem.AddFile(@"C:\website\index.md", new MockFileData(PageContents));
                var context = new SiteContext { SourceFolder = @"C:\website\", Title = "My Web Site" };
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
            }

            [Fact]
            public void The_Output_Should_Override_The_Site_Title()
            {
                Assert.Equal(ExpectedfileContents, FileSystem.File.ReadAllText(@"C:\website\_site\index.html").RemoveWhiteSpace());
            }
        }

        public class Given_A_Folder_Starting_With_Dot : BakingEnvironment<JekyllEngine>
        {
            const string PageContents = "---\r\n layout: default \r\n title: 'A different title'\r\n---\r\n\r\n## Hello World!";

            public override JekyllEngine Given()
            {
                return new JekyllEngine();
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\.gems\file.txt", new MockFileData(PageContents));
                var context = new SiteContext { SourceFolder = @"C:\website\", Title = "My Web Site" };
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
            }

            [Fact]
            public void The_Folder_Should_Be_Ignored()
            {
                Assert.False(FileSystem.File.Exists(@"C:\website\_site\.gems\file.txt"));
            }
        }

        public class When_Aeoth_Tests_The_Edge_Cases_Of_Handling_YAML_Front_Matter : BakingEnvironment<JekyllEngine>
        {
            const string PageContents = "---\n---";

            public override JekyllEngine Given()
            {
                return new JekyllEngine();
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\file.txt", new MockFileData(PageContents));
                var context = new SiteContext { SourceFolder = @"C:\website\", Title = "My Web Site" };
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
            }

            [Fact]
            public void The_Yaml_Matter_Should_Be_Cleared()
            {
                Assert.Equal("", FileSystem.File.ReadAllText(@"C:\website\_site\file.txt"));
            }
        }
    }
}