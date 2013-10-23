using System;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensibility.Extensions;
using Pretzel.Logic.Templating.Jekyll;
using Xunit;
using Pretzel.Logic.Templating.Context;

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
                var context = generator.BuildContext(@"C:\website\");
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
                var context = generator.BuildContext(@"C:\website\");
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
                var context = generator.BuildContext(@"C:\website\");
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

        // TODO: tests should live in SiteContextGenerator

        public class When_A_Template_Also_Has_A_Layout_Value : BakingEnvironment<SiteContextGenerator>
        {
            const string ParentTemplateContents = "<html><head><title>{{ page.title }}</title></head><body>{{ content }}</body></html>";
            const string InnerTemplateContents = "---\r\n layout: parent\r\n---\r\n\r\n<h1>Title</h1>{{content}}";
            const string PageContents = "---\r\n layout: inner\r\n---\r\n\r\n## Hello World!";
            const string ExpectedfileContents = "<html><head><title>My Web Site</title></head><body><h1>Title</h1><h2>Hello World!</h2></body></html>";

            public override SiteContextGenerator Given()
            {
                return new SiteContextGenerator(FileSystem, Enumerable.Empty<IContentTransform>());
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\_layouts\parent.html", new MockFileData(ParentTemplateContents));
                FileSystem.AddFile(@"C:\website\_layouts\inner.html", new MockFileData(InnerTemplateContents));
                FileSystem.AddFile(@"C:\website\index.md", new MockFileData(PageContents));

                Subject.BuildContext(@"C:\website\");
            }

            // [Fact]
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
                var context = new SiteContext { SourceFolder = @"C:\website\", Title = "My Web Site" };
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
            }

            // [Fact]
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
                var context = new SiteContext { SourceFolder = @"C:\website\", Title = "My Web Site" };
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
            }

            // [Fact]
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
                var context = new SiteContext { SourceFolder = @"C:\website\", Title = "My Web Site" };
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
            }

            // [Fact]
            public void The_Output_Should_Override_The_Site_Title()
            {
                Assert.Equal(ExpectedfileContents, FileSystem.File.ReadAllText(@"C:\website\_site\index.html").RemoveWhiteSpace());
            }
        }

       public class Givet_Page_Use_Filter : BakingEnvironment<LiquidEngine>
       {
          const string TemplateContents = "<html><body>{{ '.NET C#' | slugify}}</body></html>";
          const string PageContents = "---\r\n layout: default \r\n title: 'A different title'\r\n---\r\n\r\n## Hello World!";
          const string ExpectedfileContents = "<html><body>net-csharp</body></html>";

          public override LiquidEngine Given()
          {
             return new LiquidEngine {Filters = new IFilter[] {new SlugifyFilter()}};
          }

          public override void When()
          {
             FileSystem.AddFile(@"C:\website\_layouts\default.html", new MockFileData(TemplateContents));
             FileSystem.AddFile(@"C:\website\index.md", new MockFileData(PageContents));

             var generator = new SiteContextGenerator(FileSystem, Enumerable.Empty<IContentTransform>());
             var context = generator.BuildContext(@"C:\website\");
             Subject.FileSystem = FileSystem;
             Subject.Process(context);
          }

          [Fact]
          public void The_Output_Should_Be_Slugified()
          {
             Assert.Equal(ExpectedfileContents, FileSystem.File.ReadAllText(@"C:\website\_site\index.html").RemoveWhiteSpace());
          }
       }

        public class Given_Page_With_Code_And_Pygments_Enabled : BakingEnvironment<LiquidEngine>
        {
            const string TemplateContents = "<html><head><title>{{ page.title }}</title></head><body>{{ content }}</body></html>";
            const string PageContents = "---\r\n layout: default \r\n title: 'A different title'\r\n---\r\n\r\n{% highlight c# %}\r\nvar test = \"test\";\r\n{% endhighlight %}";
            const string ExpectedfileContents = "<html><head><title>A different title</title></head><body><p><div class=\"highlight\"><pre><span class=\"kt\">var</span> <span class=\"n\">test</span> <span class=\"p\">=</span> <span class=\"s\">&quot;test&quot;</span><span class=\"p\">;</span></pre></div></p></body></html>";

            public override LiquidEngine Given()
            {
                var engine = new LiquidEngine();
                engine.Initialize();
                return engine;
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\_layouts\default.html", new MockFileData(TemplateContents));
                FileSystem.AddFile(@"C:\website\index.md", new MockFileData(PageContents));

                var generator = new SiteContextGenerator(FileSystem, Enumerable.Empty<IContentTransform>());
                var context = generator.BuildContext(@"C:\website\");
                context.Config.Add("pygments", "true");
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
            }

            [Fact]
            public void The_Output_Should_Have_HighLighted_Code()
            {
                Assert.Equal(ExpectedfileContents, FileSystem.File.ReadAllText(@"C:\website\_site\index.html").RemoveWhiteSpace());
            }
        }

        public class Given_Page_With_Two_Code_And_Pygments_Enabled : BakingEnvironment<LiquidEngine>
        {
            const string TemplateContents = "<html><head><title>{{ page.title }}</title></head><body>{{ content }}</body></html>";
            const string PageContents = "---\r\n layout: default \r\n title: 'A different title'\r\n---\r\n\r\n{% highlight c# %}\r\nvar test = \"test\";\r\n{% endhighlight %}\r\n{% highlight c# %}\r\nvar test = \"test\";\r\n{% endhighlight %}";
            const string ExpectedfileContents = "<html><head><title>A different title</title></head><body><p><div class=\"highlight\"><pre><span class=\"kt\">var</span> <span class=\"n\">test</span> <span class=\"p\">=</span> <span class=\"s\">&quot;test&quot;</span><span class=\"p\">;</span></pre></div><div class=\"highlight\"><pre><span class=\"kt\">var</span> <span class=\"n\">test</span> <span class=\"p\">=</span> <span class=\"s\">&quot;test&quot;</span><span class=\"p\">;</span></pre></div></p></body></html>";

            public override LiquidEngine Given()
            {
                var engine = new LiquidEngine();
                engine.Initialize();
                return engine;
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\_layouts\default.html", new MockFileData(TemplateContents));
                FileSystem.AddFile(@"C:\website\index.md", new MockFileData(PageContents));

                var generator = new SiteContextGenerator(FileSystem, Enumerable.Empty<IContentTransform>());
                var context = generator.BuildContext(@"C:\website\");
                context.Config.Add("pygments", "true");
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
            }

            [Fact]
            public void The_Output_Should_Have_HighLighted_Code()
            {
                Assert.Equal(ExpectedfileContents, FileSystem.File.ReadAllText(@"C:\website\_site\index.html").RemoveWhiteSpace());
            }
        }

        public class Given_Page_With_Code_And_Pygments_Disabled : BakingEnvironment<LiquidEngine>
        {
            const string TemplateContents = "<html><head><title>{{ page.title }}</title></head><body>{{ content }}</body></html>";
            const string PageContents = "---\r\n layout: default \r\n title: 'A different title'\r\n---\r\n\r\n{% highlight c# %}\r\nvar test = \"test\";\r\n{% endhighlight %}";
            const string ExpectedfileContents = "<html><head><title>A different title</title></head><body><p><pre><code>var test = &quot;test&quot;;</code></pre></p></body></html>";

            public override LiquidEngine Given()
            {
                var engine = new LiquidEngine();
                engine.Initialize();
                return engine;
            }

            public override void When()
            {
                FileSystem.AddFile(@"C:\website\_layouts\default.html", new MockFileData(TemplateContents));
                FileSystem.AddFile(@"C:\website\index.md", new MockFileData(PageContents));

                var generator = new SiteContextGenerator(FileSystem, Enumerable.Empty<IContentTransform>());
                var context = generator.BuildContext(@"C:\website\");
                Subject.FileSystem = FileSystem;
                Subject.Process(context);
            }

            [Fact]
            public void The_Output_Should_Have_HighLighted_Code()
            {
                Assert.Equal(ExpectedfileContents, FileSystem.File.ReadAllText(@"C:\website\_site\index.html").RemoveWhiteSpace());
            }
        }
    }
}