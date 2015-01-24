using Pretzel.Logic.Exceptions;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensibility.Extensions;
using Pretzel.Logic.Templating.Context;
using Pretzel.Logic.Templating.Jekyll;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Xunit;

namespace Pretzel.Tests.Templating.Jekyll
{
    public class LiquidEngineTests
    {
        public class When_Recieving_A_Folder_Containing_One_File : BakingEnvironment<LiquidEngine>
        {
            const string FileContents = "<html><head></head><body></body></html>";
            const string Folder = @"C:\website\";
            readonly SiteContext context = new SiteContext { SourceFolder = Folder };

            public override LiquidEngine Given()
            {
                context.Pages.Add(new Page
                {
                    Content = FileContents,
                    File = "index.html",
                    Filepath = @"C:\website\index.html"
                });

                return new LiquidEngine();
            }

            public override void When()
            {
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

        public class When_Recieving_A_Folder_Without_A_Trailing_Slash : BakingEnvironment<LiquidEngine>
        {
            const string FileContents = "<html><head></head><body></body></html>";
            const string Folder = @"C:\website";
            readonly SiteContext context = new SiteContext { SourceFolder = Folder };

            public override LiquidEngine Given()
            {
                context.Pages.Add(new Page
                {
                    Content = FileContents,
                    File = "index.html",
                    Filepath = @"C:\website\index.html"
                });
                return new LiquidEngine();
            }

            public override void When()
            {
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
            }

            [Fact]
            public void The_File_Is_Added_At_The_Root()
            {
                Assert.True(FileSystem.File.Exists(@"C:\website\_site\index.html"));
            }
        }

        public class When_Recieving_A_Folder_Containing_One_File_In_A_Subfolder : BakingEnvironment<LiquidEngine>
        {
            const string FileContents = "<html><head></head><body></body></html>";
            const string Folder = @"C:\website";
            readonly SiteContext context = new SiteContext { SourceFolder = Folder };

            public override LiquidEngine Given()
            {
                context.Pages.Add(new Page
                {
                    Content = FileContents,
                    File = @"content\index.html",
                    Filepath = @"C:\website\content\index.html"
                });
                return new LiquidEngine();
            }

            public override void When()
            {
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

        public class When_Recieving_A_File_With_Metadata : BakingEnvironment<LiquidEngine>
        {
            const string FileContents = "<html><head><title>{{ page.title }}</title></head><body></body></html>";
            const string OutputContents = "<html><head><title></title></head><body></body></html>";
            const string Folder = @"C:\website";
            readonly SiteContext context = new SiteContext { SourceFolder = Folder };

            public override LiquidEngine Given()
            {
                context.Pages.Add(new Page
                {
                    Content = FileContents,
                    File = @"index.html",
                    Filepath = @"C:\website\index.html"
                });

                return new LiquidEngine();
            }

            public override void When()
            {
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
            }

            [Fact]
            public void The_File_Is_Altered()
            {
                Assert.Equal(OutputContents, FileSystem.File.ReadAllText(@"C:\website\_site\index.html"));
            }
        }

        public class Given_A_Folder_Starting_With_Dot : BakingEnvironment<LiquidEngine>
        {
            const string PageContents = "---\r\n layout: default \r\n title: 'A different title'\r\n---\r\n\r\n## Hello World!";

            public override LiquidEngine Given()
            {
                return new LiquidEngine();
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

        public class When_Paginate_With_No_Posts : BakingEnvironment<LiquidEngine>
        {
            const string TemplateContents = "<html><head><title>{{ page.title }}</title></head><body>{{ content }}</body></html>";
            const string IndexContents = "---\r\n layout: default \r\n paginate: 5 \r\n paginate_link: /blog/page:page/index.html \r\n title: 'A different title'\r\n---\r\n\r\n## Hello World!";
            const string ExpectedfileContents = "<html><head><title>A different title</title></head><body><h2>Hello World!</h2></body></html>";

            public override LiquidEngine Given()
            {
                return new LiquidEngine();
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\_layouts\default.html", new MockFileData(TemplateContents));
                FileSystem.AddFile(@"C:\website\index.md", new MockFileData(IndexContents));

                var generator = new SiteContextGenerator(FileSystem, Enumerable.Empty<IContentTransform>());
                var context = generator.BuildContext(@"C:\website\", false);
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
            }

            [Fact]
            public void Index_Is_Generated()
            {
                const string fileName = @"C:\website\_site\index.html";
                Assert.Equal(ExpectedfileContents, FileSystem.File.ReadAllText(fileName).RemoveWhiteSpace());
            }

            [Fact]
            public void No_Pages_Should_Be_Generated()
            {
                Assert.False(FileSystem.File.Exists(@"C:\website\_site\page1\index.html"));
                Assert.False(FileSystem.File.Exists(@"C:\website\_site\page2\index.html"));
            }
        }

        public class When_Paginate_With_Default_Pagelink : BakingEnvironment<LiquidEngine>
        {
            const string TemplateContents = "<html><head><title>{{ page.title }}</title></head><body>{{ content }}</body></html>";
            const string PostContents = "---\r\n layout: default \r\n title: 'Post'\r\n---\r\n\r\n## Hello World!";
            const string IndexContents = "---\r\n layout: default \r\n paginate: 1 \r\n title: 'A different title'\r\n---\r\n\r\n## Hello World!";
            const string ExpectedfileContents = "<html><head><title>A different title</title></head><body><h2>Hello World!</h2></body></html>";

            public override LiquidEngine Given()
            {
                return new LiquidEngine();
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\_layouts\default.html", new MockFileData(TemplateContents));
                FileSystem.AddFile(@"C:\website\index.md", new MockFileData(IndexContents));

                for (var i = 1; i <= 5; i++)
                {
                    FileSystem.AddFile(String.Format(@"C:\website\_posts\2012-02-0{0}-p{0}.md", i), new MockFileData(PostContents));
                }

                var generator = new SiteContextGenerator(FileSystem, Enumerable.Empty<IContentTransform>());
                var context = generator.BuildContext(@"C:\website\", false);
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
            }

            [Fact]
            public void Four_Pages_Should_Be_Generated()
            {
                for (var i = 2; i <= 5; i++)
                {
                    var fileName = String.Format(@"C:\website\_site\page\{0}\index.html", i);
                    Assert.Equal(ExpectedfileContents, FileSystem.File.ReadAllText(fileName).RemoveWhiteSpace());
                }
            }

            [Fact]
            public void Page1_Is_Not_Generated()
            {
                Assert.False(FileSystem.File.Exists(@"C:\website\_site\page\1\index.html"));
            }
        }

        public class When_Paginate_With_Custom_Pagelink : BakingEnvironment<LiquidEngine>
        {
            const string TemplateContents = "<html><head><title>{{ page.title }}</title></head><body>{{ content }}</body></html>";
            const string PostContents = "---\r\n layout: default \r\n title: 'Post'\r\n---\r\n\r\n## Hello World!";
            const string IndexContents = "---\r\n layout: default \r\n paginate: 2 \r\n paginate_link: /blog/page:page/index.html \r\n title: 'A different title'\r\n---\r\n\r\n## Hello World!";
            const string ExpectedfileContents = "<html><head><title>A different title</title></head><body><h2>Hello World!</h2></body></html>";

            public override LiquidEngine Given()
            {
                return new LiquidEngine();
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\_layouts\default.html", new MockFileData(TemplateContents));
                FileSystem.AddFile(@"C:\website\index.md", new MockFileData(IndexContents));

                for (var i = 1; i <= 7; i++)
                {
                    FileSystem.AddFile(String.Format(@"C:\website\_posts\2012-02-0{0}-p{0}.md", i), new MockFileData(PostContents));
                }

                var generator = new SiteContextGenerator(FileSystem, Enumerable.Empty<IContentTransform>());
                var context = generator.BuildContext(@"C:\website\", false);
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
            }

            [Fact]
            public void Four_Pages_Should_Be_Generated()
            {
                for (var i = 2; i <= 4; i++)
                {
                    var fileName = String.Format(@"C:\website\_site\blog\page{0}\index.html", i);
                    Assert.Equal(ExpectedfileContents, FileSystem.File.ReadAllText(fileName).RemoveWhiteSpace());
                }
            }

            [Fact]
            public void Page1_Is_Not_Generated()
            {
                Assert.False(FileSystem.File.Exists(@"C:\website\_site\blog\page1\index.html"));
            }
        }

        public class When_SiteContext_Specifies_Title : BakingEnvironment<LiquidEngine>
        {
            const string PageContents = "<html><head><title>{{ page.title }}</title></head><body><h1>Hello World!</h1></body></html>";
            readonly SiteContext context = new SiteContext { SourceFolder = Folder, Title = "My Web Site" };
            readonly string expectedfileContents = PageContents.Replace(@"{{ page.title }}", "My Web Site");
            const string Folder = @"C:\website";

            public override LiquidEngine Given()
            {
                context.Pages.Add(new Page
                {
                    Content = PageContents,
                    File = @"index.md",
                    Filepath = @"C:\website\index.html"
                });
                return new LiquidEngine();
            }

            public override void When()
            {
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
            }

            [Fact]
            public void The_Page_Includes_The_Value()
            {
                Assert.Equal(expectedfileContents, FileSystem.File.ReadAllText(@"C:\website\_site\index.html"));
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

                var generator = new SiteContextGenerator(FileSystem, Enumerable.Empty<IContentTransform>());
                var context = generator.BuildContext(@"C:\website\", false);
                context.Title = "My Web Site";
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
                Assert.False(FileSystem.File.Exists(@"C:\website\_site\_layouts\parent.html"));
                Assert.False(FileSystem.File.Exists(@"C:\website\_site\_layouts\inner.html"));
            }
        }

        public class When_Aeoth_Tests_The_Edge_Cases_Of_Handling_YAML_Front_Matter : BakingEnvironment<LiquidEngine>
        {
            const string PageContents = "---\n---";

            public override LiquidEngine Given()
            {
                return new LiquidEngine();
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\file.txt", new MockFileData(PageContents));
                var generator = new SiteContextGenerator(FileSystem, Enumerable.Empty<IContentTransform>());
                var context = generator.BuildContext(@"C:\website\", false);
                context.Title = "My Web Site";
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
            }

            [Fact]
            public void The_Yaml_Matter_Should_Be_Cleared()
            {
                Assert.Equal("", FileSystem.File.ReadAllText(@"C:\website\_site\file.txt"));
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
                var generator = new SiteContextGenerator(FileSystem, Enumerable.Empty<IContentTransform>());
                var context = generator.BuildContext(@"C:\website\", false);
                context.Title = "My Web Site";
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
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
                var generator = new SiteContextGenerator(FileSystem, Enumerable.Empty<IContentTransform>());
                var context = generator.BuildContext(@"C:\website\", false);
                context.Title = "My Web Site";
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
            }

            [Fact]
            public void The_Output_Should_Override_The_Site_Title()
            {
                Assert.Equal(ExpectedfileContents, FileSystem.File.ReadAllText(@"C:\website\_site\index.html").RemoveWhiteSpace());
            }
        }

        public class Given_Page_Use_Filter : BakingEnvironment<LiquidEngine>
        {
            const string TemplateContents = "<html><body>{{ '.NET C#' | slugify}}</body></html>";
            const string PageContents = "---\r\n layout: default \r\n title: 'A different title'\r\n---\r\n\r\n## Hello World!";
            const string ExpectedfileContents = "<html><body>net-csharp</body></html>";

            public override LiquidEngine Given()
            {
                return new LiquidEngine { Filters = new IFilter[] { new SlugifyFilter() } };
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\_layouts\default.html", new MockFileData(TemplateContents));
                FileSystem.AddFile(@"C:\website\index.md", new MockFileData(PageContents));

                var generator = new SiteContextGenerator(FileSystem, Enumerable.Empty<IContentTransform>());
                var context = generator.BuildContext(@"C:\website\", false);
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
            }

            [Fact]
            public void The_Output_Should_Be_Slugified()
            {
                Assert.Equal(ExpectedfileContents, FileSystem.File.ReadAllText(@"C:\website\_site\index.html").RemoveWhiteSpace());
            }
        }

        public class Given_Config_File_Has_A_Title : BakingEnvironment<LiquidEngine>
        {
            const string TemplateContents_1 = "<html><head><title>{{ page.title }}</title></head><body>{{ content }}</body></html>";
            const string TemplateContents_2 = "<html><head><title>{{ site.title }}</title></head><body>{{ content }}</body></html>";
            const string PageContents_1 = "---\r\n layout: default \r\n---\r\n\r\n## Hello World!";
            const string PageContents_2 = "---\r\n layout: default2 \r\n---\r\n\r\n## Hello World!";
            const string ConfigContents = "---\r\n title: A different title\r\n---";
            const string ExpectedfileContents = "<html><head><title>A different title</title></head><body><h2>Hello World!</h2></body></html>";

            public override LiquidEngine Given()
            {
                return new LiquidEngine();
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\_config.yml", new MockFileData(ConfigContents));
                FileSystem.AddFile(@"C:\website\_layouts\default.html", new MockFileData(TemplateContents_1));
                FileSystem.AddFile(@"C:\website\_layouts\default2.html", new MockFileData(TemplateContents_2));
                FileSystem.AddFile(@"C:\website\index.md", new MockFileData(PageContents_1));
                FileSystem.AddFile(@"C:\website\index2.md", new MockFileData(PageContents_2));
                var generator = new SiteContextGenerator(FileSystem, Enumerable.Empty<IContentTransform>());
                var context = generator.BuildContext(@"C:\website\", false);
                context.Title = "My Web Site";
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
            }

            [Fact]
            public void The_Output_Should_Override_The_Page_Title()
            {
                Assert.Equal(ExpectedfileContents, FileSystem.File.ReadAllText(@"C:\website\_site\index.html").RemoveWhiteSpace());
            }

            [Fact]
            public void The_Output_Should_Override_The_Site_Title()
            {
                Assert.Equal(ExpectedfileContents, FileSystem.File.ReadAllText(@"C:\website\_site\index2.html").RemoveWhiteSpace());
            }
        }

        public class Given_Config_File_Has_A_Title_And_Context_Has_No_Title : BakingEnvironment<LiquidEngine>
        {
            const string TemplateContents = "<html><head><title>{{ site.title }}</title></head><body>{{ content }}</body></html>";
            const string PageContents = "---\r\n layout: default \r\n---\r\n\r\n## Hello World!";
            const string ConfigContents = "---\r\n title: A different title\r\n---";
            const string ExpectedfileContents = "<html><head><title>A different title</title></head><body><h2>Hello World!</h2></body></html>";

            public override LiquidEngine Given()
            {
                return new LiquidEngine();
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\_config.yml", new MockFileData(ConfigContents));
                FileSystem.AddFile(@"C:\website\_layouts\default.html", new MockFileData(TemplateContents));
                FileSystem.AddFile(@"C:\website\index.md", new MockFileData(PageContents));
                var generator = new SiteContextGenerator(FileSystem, Enumerable.Empty<IContentTransform>());
                var context = generator.BuildContext(@"C:\website\", false);
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
            }

            [Fact]
            public void The_Output_Should_Use_The_Site_Title_From_Config()
            {
                Assert.Equal(ExpectedfileContents, FileSystem.File.ReadAllText(@"C:\website\_site\index.html").RemoveWhiteSpace());
            }
        }

        public class Given_Site_Comport_Non_Standard_Files : BakingEnvironment<LiquidEngine>
        {
            const string TemplateContents = "<html><head><title>{{ site.title }}</title></head><body>{{ content }}</body></html>";
            const string NonProcessedPageContents = "## Hello World!";
            const string LayoutNilPageContents = "---\r\n layout: nil \r\n---\r\n\r\n## Hello World!";
            const string NoLayoutPageContents = "---\r\n  \r\n---\r\n\r\n## Hello World!";
            const string NonExistingLayoutPageContents = "----\r\n layout: inexistant \r\n---\r\n\r\n## Hello World!";

            const string ExpectedfileContents = "<h2>Hello World!</h2>";

            public override LiquidEngine Given()
            {
                return new LiquidEngine();
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\_layouts\default.html", new MockFileData(TemplateContents));
                FileSystem.AddFile(@"C:\website\NoProcessed.md", new MockFileData(NonProcessedPageContents));
                FileSystem.AddFile(@"C:\website\LayoutNil.md", new MockFileData(LayoutNilPageContents));
                FileSystem.AddFile(@"C:\website\NoLayout.md", new MockFileData(NoLayoutPageContents));
                FileSystem.AddFile(@"C:\website\NonExistingLayout.md", new MockFileData(NonExistingLayoutPageContents));
                FileSystem.AddFile(@"C:\website\image.jpg", new MockFileData("jpg image"));
                FileSystem.AddFile(@"C:\website\image.png", new MockFileData("png image"));
                FileSystem.AddFile(@"C:\website\image.gif", new MockFileData("gif image"));
                var generator = new SiteContextGenerator(FileSystem, Enumerable.Empty<IContentTransform>());
                var context = generator.BuildContext(@"C:\website\", false);
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
            }

            [Fact]
            public void Non_Processed_Page_Should_Not_Be_Transformed()
            {
                Assert.Equal("## Hello World!", FileSystem.File.ReadAllText(@"C:\website\_site\NoProcessed.md").RemoveWhiteSpace());
            }

            [Fact]
            public void Page_With_Layout_Nil_Should_Not_Have_Any_Layout()
            {
                Assert.Equal(ExpectedfileContents, FileSystem.File.ReadAllText(@"C:\website\_site\LayoutNil.html").RemoveWhiteSpace());
            }

            [Fact]
            public void Page_With_No_Layout_Should_Not_Have_Any_Layout()
            {
                Assert.Equal(ExpectedfileContents, FileSystem.File.ReadAllText(@"C:\website\_site\NoLayout.html").RemoveWhiteSpace());
            }

            [Fact]
            public void Page_With_Non_Existing_Layout_Should_Not_Have_Any_Layout()
            {
                Assert.Equal(ExpectedfileContents, FileSystem.File.ReadAllText(@"C:\website\_site\NonExistingLayout.html").RemoveWhiteSpace());
            }

            [Fact]
            public void Images_Should_Be_Copied_In_Output_Directory()
            {
                Assert.Equal("jpg image", FileSystem.File.ReadAllText(@"C:\website\_site\image.jpg").RemoveWhiteSpace());
                Assert.Equal("png image", FileSystem.File.ReadAllText(@"C:\website\_site\image.png").RemoveWhiteSpace());
                Assert.Equal("gif image", FileSystem.File.ReadAllText(@"C:\website\_site\image.gif").RemoveWhiteSpace());
            }
        }

        public class Given_Site_Comport_Bad_Formated_Layout : BakingEnvironment<LiquidEngine>
        {
            private SiteContext Context;
            const string TemplateContents = "----\r\n layout: default \r\n-----<html><head><title>{{ site.title }}</title>{{}</head><body>{{ content }}</body></html>";
            const string PageContents = "---\r\n layout: default \r\n---\r\n\r\n## Hello World!";

            public override LiquidEngine Given()
            {
                return new LiquidEngine();
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\_layouts\default.html", new MockFileData(TemplateContents));
                FileSystem.AddFile(@"C:\website\index.md", new MockFileData(PageContents));
                var generator = new SiteContextGenerator(FileSystem, Enumerable.Empty<IContentTransform>());
                Context = generator.BuildContext(@"C:\website\", false);
                Subject.FileSystem = FileSystem;
            }

            [Fact]
            public void Layout_With_Bad_Header_Should_Throw_Exception()
            {
                var exception = Record.Exception(() => Subject.Process(Context));
                Assert.IsType(typeof(PageProcessingException), exception);
                Assert.Equal(@"Failed to process layout default for C:\website\_site\index.html, see inner exception for more details", exception.Message);
                Assert.False(FileSystem.AllFiles.Contains(@"C:\website\_site\index.html"));
            }
        }

        public class Given_Site_Comport_Bad_Formated_Layout_But_Skip_File_Error : BakingEnvironment<LiquidEngine>
        {
            private SiteContext Context;
            const string TemplateContents = "----\r\n layout: default \r\n-----<html><head><title>{{ site.title }}</title>{{}</head><body>{{ content }}</body></html>";
            const string PageContents = "---\r\n layout: default \r\n---\r\n\r\n## Hello World!";

            public override LiquidEngine Given()
            {
                return new LiquidEngine();
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\_layouts\default.html", new MockFileData(TemplateContents));
                FileSystem.AddFile(@"C:\website\index.md", new MockFileData(PageContents));
                var generator = new SiteContextGenerator(FileSystem, Enumerable.Empty<IContentTransform>());
                Context = generator.BuildContext(@"C:\website\", false);
                Subject.FileSystem = FileSystem;
            }

            [Fact]
            public void Layout_With_Bad_Header_Should_Not_Throw_Exception()
            {
                using (StringWriter sw = new StringWriter())
                {
                    Console.SetOut(sw);

                    Subject.Process(Context, true);

                    Assert.Equal(@"Failed to process layout default for C:\website\_site\index.html because 'Variable '{{}' was not properly terminated with regexp: (?-mix:\}\})'. Skipping file" + Environment.NewLine, sw.ToString());
                }
                Assert.False(FileSystem.AllFiles.Contains(@"C:\website\_site\index.html"));
            }
        }

        public class Given_Page_Is_Bad_Formated : BakingEnvironment<LiquidEngine>
        {
            private SiteContext Context;
            const string BadFormatPageContents = "---\r\n layout: default \r\n---\r\n\r\n## Hello World! {{}";

            public override LiquidEngine Given()
            {
                return new LiquidEngine();
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\BadFormat.md", new MockFileData(BadFormatPageContents));
                var generator = new SiteContextGenerator(FileSystem, Enumerable.Empty<IContentTransform>());
                Context = generator.BuildContext(@"C:\website\", false);
                Subject.FileSystem = FileSystem;
            }

            [Fact]
            public void Layout_With_Bad_Header_Should_Throw_Exception()
            {
                var exception = Record.Exception(() => Subject.Process(Context));
                Assert.IsType(typeof(PageProcessingException), exception);
                Assert.Equal(@"Failed to process C:\website\_site\BadFormat.html, see inner exception for more details", exception.Message);
                Assert.False(FileSystem.AllFiles.Contains(@"C:\website\_site\BadFormat.html"));
            }
        }

        public class Given_Page_Is_Bad_Formated_But_Skip_File_Error : BakingEnvironment<LiquidEngine>
        {
            private SiteContext Context;
            const string BadFormatPageContents = "---\r\n layout: default \r\n---\r\n\r\n## Hello World! {{}";

            public override LiquidEngine Given()
            {
                return new LiquidEngine();
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\BadFormat.md", new MockFileData(BadFormatPageContents));
                var generator = new SiteContextGenerator(FileSystem, Enumerable.Empty<IContentTransform>());
                Context = generator.BuildContext(@"C:\website\", false);
                Subject.FileSystem = FileSystem;
            }

            [Fact]
            public void Layout_With_Bad_Header_Should_Not_Throw_Exception()
            {
                using (StringWriter sw = new StringWriter())
                {
                    Console.SetOut(sw);

                    Subject.Process(Context, true);

                    Assert.Equal(@"Failed to process C:\website\_site\BadFormat.html, see inner exception for more details" + Environment.NewLine, sw.ToString());
                }
                Assert.False(FileSystem.AllFiles.Contains(@"C:\website\_site\BadFormat.html"));
            }
        }

        public class Given_Older_Non_Processed_Page_Already_Exists_In_Output_Directory : BakingEnvironment<LiquidEngine>
        {
            const string OriginalPageContents = "## Hello Earth!";
            const string PageContents = "## Hello World!";

            public override LiquidEngine Given()
            {
                return new LiquidEngine();
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\_site\BadFormat.md", new MockFileData(OriginalPageContents) { LastWriteTime = new DateTime(2010, 01, 2) });
                FileSystem.AddFile(@"C:\website\BadFormat.md", new MockFileData(PageContents) { LastWriteTime = new DateTime(2010, 01, 3) });

                var generator = new SiteContextGenerator(FileSystem, Enumerable.Empty<IContentTransform>());
                var context = generator.BuildContext(@"C:\website\", false);
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
            }

            [Fact]
            public void Existing_File_Should_Be_Replaced()
            {
                Assert.Equal(PageContents, FileSystem.File.ReadAllText(@"C:\website\_site\BadFormat.md").RemoveWhiteSpace());
            }
        }

        public class Given_Newer_Non_Processed_Page_Already_Exists_In_Output_Directory : BakingEnvironment<LiquidEngine>
        {
            const string OriginalPageContents = "## Hello Earth!";
            const string PageContents = "## Hello World!";

            public override LiquidEngine Given()
            {
                return new LiquidEngine();
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\_site\BadFormat.md", new MockFileData(OriginalPageContents) { LastWriteTime = new DateTime(2010, 01, 5) });
                FileSystem.AddFile(@"C:\website\BadFormat.md", new MockFileData(PageContents) { LastWriteTime = new DateTime(2010, 01, 3) });

                var generator = new SiteContextGenerator(FileSystem, Enumerable.Empty<IContentTransform>());
                var context = generator.BuildContext(@"C:\website\", false);
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
            }

            [Fact]
            public void Existing_File_Should_Be_Not_Replaced()
            {
                Assert.Equal(OriginalPageContents, FileSystem.File.ReadAllText(@"C:\website\_site\BadFormat.md").RemoveWhiteSpace());
            }
        }

        public class Given_GetOutputDirectory_Method : BakingEnvironment<LiquidEngine>
        {
            public override LiquidEngine Given()
            {
                return new LiquidEngine();
            }

            public override void When()
            {
                FileSystem.AddDirectory(@"C:\website\");
                var generator = new SiteContextGenerator(FileSystem, Enumerable.Empty<IContentTransform>());
                var context = generator.BuildContext(@"C:\website\", false);
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
            }

            [Fact]
            public void Output_Directory_Should_Comport_site()
            {
                Assert.Equal(@"C:\mysite\_site", Subject.GetOutputDirectory(@"C:\mysite"));
            }
        }

        public class Given_Site_Engine_Is_Liquid : BakingEnvironment<LiquidEngine>
        {
            private SiteContext context;

            public override LiquidEngine Given()
            {
                return new LiquidEngine();
            }

            public override void When()
            {
                context = new SiteContext();
                context.Config.Add("pretzel", new Dictionary<string, object> { { "engine", "liquid" } });
            }

            [Fact]
            public void CanProcess_Should_Return_True()
            {
                Assert.True(Subject.CanProcess(context));
            }
        }

        public class Given_Site_Engine_Is_Not_Liquid : BakingEnvironment<LiquidEngine>
        {
            private SiteContext context;

            public override LiquidEngine Given()
            {
                return new LiquidEngine();
            }

            public override void When()
            {
                context = new SiteContext();
                context.Config.Add("pretzel", new Dictionary<string, object> { { "engine", "myengine" } });
            }

            [Fact]
            public void CanProcess_Should_Return_False()
            {
                Assert.False(Subject.CanProcess(context));
            }
        }
    }
}