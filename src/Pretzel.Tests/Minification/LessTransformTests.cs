using Pretzel.Logic.Minification;
using Pretzel.Logic.Templating.Context;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using Xunit;

namespace Pretzel.Tests.Minification
{
    public class LessTransformTests
    {
        private const string HtmlFilePath = @"c:\index.html";
        private const string PageContent = @"<html><head><link rel='stylesheet' href='css\style.css' /></head><body></body></html>";

        [Fact]
        public void Should_Minify_Single_File()
        {
            var filepath = @"c:\css\style.less";
            var lessContent = "a { color: Red; }";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { HtmlFilePath, new MockFileData(PageContent)},
                { filepath, new MockFileData(lessContent) }
            });

            var minifier = new LessTransform(fileSystem);
            var context = new SiteContext { SourceFolder = @"C:\", OutputFolder = @"C:\_site" };
            context.Pages.Add(new NonProcessedPage { OutputFile = HtmlFilePath, Content = PageContent });
            context.Pages.Add(new NonProcessedPage { OutputFile = filepath, Content = lessContent, Filepath = filepath });
            minifier.Transform(context);

            var minifiedFile = fileSystem.File.ReadAllText(@"c:\css\style.css", Encoding.UTF8);

            Assert.Equal("a{color:Red}", minifiedFile);
        }

        [Fact]
        public void Should_Compile_Single_Less_File()
        {
            var lessContent = @"@brand_color: #4D926F;

                                    #header {
                                        color: @brand_color;
                                    }
 
                                    h2 {
                                        color: @brand_color;
                                    }";

            var lessOutput = @"#header{color:#4d926f}h2{color:#4d926f}";

            var filepath = @"c:\css\style.less";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { HtmlFilePath, new MockFileData(PageContent)},
                { filepath, new MockFileData(lessContent) }
            });

            var minifier = new LessTransform(fileSystem);
            var context = new SiteContext { SourceFolder = @"C:\", OutputFolder = @"C:\_site" };
            context.Pages.Add(new NonProcessedPage { OutputFile = HtmlFilePath, Content = PageContent });
            context.Pages.Add(new NonProcessedPage { OutputFile = filepath, Content = lessContent, Filepath = filepath });
            minifier.Transform(context);

            var minifiedFile = fileSystem.File.ReadAllText(@"c:\css\style.css", Encoding.UTF8);
            Assert.Equal(lessOutput, minifiedFile);
        }

        [Fact]
        public void Should_Compile_Less_To_Css_To_Output_Path()
        {
            var filepath = @"c:\css\style.less";
            var lessContent = @"@brand_color: #4D926F;

                                    #header {
                                        color: @brand_color;
                                    }
 
                                    h2 {
                                        color: @brand_color;
                                    }";

            var lessOutput = @"#header{color:#4d926f}h2{color:#4d926f}";

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { filepath, new MockFileData(lessContent) },
                { HtmlFilePath , new MockFileData(PageContent) }
            });

            var minifier = new LessTransform(fileSystem);
            var context = new SiteContext { SourceFolder = @"C:\", OutputFolder = @"C:\_site" };
            context.Pages.Add(new NonProcessedPage { OutputFile = HtmlFilePath, Content = PageContent });
            context.Pages.Add(new NonProcessedPage { OutputFile = filepath, Content = lessContent, Filepath = filepath });
            minifier.Transform(context);

            var minifiedFile = fileSystem.File.ReadAllText(@"c:\css\style.css", Encoding.UTF8);

            Assert.Equal(lessOutput, minifiedFile);
        }

        [Fact]
        public void Should_Process_Less_Imports()
        {
            var filepath1 = @"c:\css\style-dependency.less";
            var fileContent1 = "@brand_color: #4D926F;";
            var filepath2 = @"c:\css\style.less";
            var fileContent2 = "@import \"style-dependency.less\"; a { color: @brand_color; }";

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { HtmlFilePath, new MockFileData(PageContent)},
                { filepath1, new MockFileData(fileContent1) },
                { filepath2, new MockFileData(fileContent2) }
            });

            var expectedOutput = @"a{color:#4d926f}";

            var minifier = new LessTransform(fileSystem);
            var context = new SiteContext { SourceFolder = @"C:\", OutputFolder = @"C:\_site" };
            context.Pages.Add(new NonProcessedPage { OutputFile = HtmlFilePath, Content = PageContent });
            context.Pages.Add(new NonProcessedPage { OutputFile = filepath1, Content = fileContent1, Filepath = filepath1 });
            context.Pages.Add(new NonProcessedPage { OutputFile = filepath2, Content = fileContent2, Filepath = filepath2 });
            minifier.Transform(context);

            var minifiedFile = fileSystem.File.ReadAllText(@"c:\css\style.css", Encoding.UTF8);

            Assert.Equal(expectedOutput, minifiedFile);
        }

        [Fact]
        public void Should_Process_Less_Imports_With_Cleanup()
        {
            // arrange
            var filepath1 = @"c:\css\subfolder\style-dependency.less";
            var fileContent1 = "@brand_color: #4D926F;";
            var filepath2 = @"c:\css\style.less";
            var fileContent2 = "@import \"subfolder/style-dependency.less\"; @import \"foldertodelete/style-dependency2.less\"; @import \"foldertodelete/style-dependency3.less\"; @import \"../_site/style-dependency4.less\"; a { color: @brand_color; width: @width; height: @height; foo: @bar; }";
            var filepath3 = @"c:\css\foldertodelete\style-dependency2.less";
            var fileContent3 = "@width: 24px;";
            var filepath4 = @"c:\css\foldertodelete\style-dependency3.less";
            var fileContent4 = "@height: 24px;";
            var filepath5 = @"c:\_site\style-dependency4.less";
            var fileContent5 = "@bar: bold;";

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { HtmlFilePath, new MockFileData(PageContent)},
                { filepath1, new MockFileData(fileContent1) },
                { filepath2, new MockFileData(fileContent2) },
                { filepath3, new MockFileData(fileContent3) },
                { filepath4, new MockFileData(fileContent4) },
                { filepath5, new MockFileData(fileContent5) },
                { @"c:\css\subfolder\anothersubfolder\anything.less", "@size: 12px;" }
            });
            fileSystem.AddDirectory(@"c:\css\emptysubfolder");

            var expectedOutput = @"a{color:#4d926f;width:24px;height:24px;foo:bold}";

            var minifier = new LessTransform(fileSystem);
            var context = new SiteContext { SourceFolder = @"C:\", OutputFolder = @"C:\_site" };
            context.Pages.Add(new NonProcessedPage { OutputFile = HtmlFilePath, Content = PageContent });
            context.Pages.Add(new NonProcessedPage { OutputFile = filepath1, Content = fileContent1, Filepath = filepath1 });
            context.Pages.Add(new NonProcessedPage { OutputFile = filepath2, Content = fileContent2, Filepath = filepath2 });
            context.Pages.Add(new NonProcessedPage { OutputFile = filepath3, Content = fileContent3, Filepath = filepath3 });
            context.Pages.Add(new NonProcessedPage { OutputFile = filepath4, Content = fileContent4, Filepath = filepath4 });
            context.Pages.Add(new NonProcessedPage { OutputFile = filepath5, Content = fileContent5, Filepath = filepath5 });

            // act
            minifier.Transform(context);

            // assert
            var minifiedFile = fileSystem.File.ReadAllText(@"c:\css\style.css", Encoding.UTF8);
            Assert.Equal(expectedOutput, minifiedFile);
            Assert.True(fileSystem.Directory.Exists(@"c:\css\emptysubfolder"));
            Assert.False(fileSystem.Directory.Exists(@"c:\css\foldertodelete"));
            Assert.False(fileSystem.File.Exists(@"c:\css\style.less"));
            Assert.True(fileSystem.Directory.Exists(@"c:\css\subfolder"));
            Assert.True(fileSystem.File.Exists(@"c:\css\subfolder\anothersubfolder\anything.less"));
            Assert.True(fileSystem.Directory.Exists(@"c:\_site"));
        }

        [Fact]
        public void Should_Not_Process_Exernal_Css_File()
        {
            // arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { HtmlFilePath, new MockFileData(@"<html><head><link rel='stylesheet' href='http://foor.bar/style.css' /><link rel='stylesheet' href='https://foor.bar/style.css' /><link rel='stylesheet' href='//foor.bar/style.css' /></head><body></body></html>")}
            });

            var minifier = new LessTransform(fileSystem);
            var context = new SiteContext { SourceFolder = @"C:\", OutputFolder = @"C:\_site" };
            context.Pages.Add(new NonProcessedPage { OutputFile = HtmlFilePath, Content = PageContent });

            // act
            minifier.Transform(context);

            // assert
            // No css file have been generated
            Assert.Equal(2, fileSystem.AllPaths.Count());
            Assert.False(fileSystem.AllPaths.Any(p => p.EndsWith(".css")));
        }

        [Fact]
        public void Should_Not_Process_Already_Existing_Css_File()
        {
            // arrange
            var cssFilePath = @"c:\css\style.css";
            var lessFilepath = @"c:\css\style.less";
            var lessContent = "a { color: Red; }";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { HtmlFilePath, new MockFileData(PageContent)},
                { cssFilePath, MockFileData.NullObject },
                { lessFilepath, new MockFileData(lessContent) }
            });

            var minifier = new LessTransform(fileSystem);
            var context = new SiteContext { SourceFolder = @"C:\", OutputFolder = @"C:\_site" };
            context.Pages.Add(new NonProcessedPage { OutputFile = HtmlFilePath, Content = PageContent });
            context.Pages.Add(new NonProcessedPage { OutputFile = cssFilePath, Content = string.Empty });
            context.Pages.Add(new NonProcessedPage { OutputFile = lessFilepath, Content = lessContent });

            // act
            minifier.Transform(context);

            // assert
            // The existing css file is still empty
            Assert.Equal(string.Empty, fileSystem.File.ReadAllText(cssFilePath));
        }

        [Fact]
        public void If_no_corresponding_less_file_nothing_is_done()
        {
            // arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { HtmlFilePath, new MockFileData(PageContent)}
            });

            var minifier = new LessTransform(fileSystem);
            var context = new SiteContext { SourceFolder = @"C:\", OutputFolder = @"C:\_site" };
            context.Pages.Add(new NonProcessedPage { OutputFile = HtmlFilePath, Content = PageContent });

            // act
            minifier.Transform(context);

            // assert
            // No css file have been generated
            Assert.Equal(2, fileSystem.AllPaths.Count());
            Assert.False(fileSystem.AllPaths.Any(p => p.EndsWith(".css")));
        }

        [Fact]
        public void Multiple_css_file_references_should_generate_one_file()
        {
            // arrange
            var lessFilepath = @"c:\css\style.less";
            var lessContent = "a { color: Red; }";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { HtmlFilePath, new MockFileData(PageContent)},
                { @"c:\about.html", new MockFileData(PageContent)},
                { lessFilepath, new MockFileData(lessContent) }
            });

            var minifier = new LessTransform(fileSystem);
            var context = new SiteContext { SourceFolder = @"C:\", OutputFolder = @"C:\_site" };
            context.Pages.Add(new NonProcessedPage { OutputFile = HtmlFilePath, Content = PageContent });
            context.Pages.Add(new NonProcessedPage { OutputFile = @"c:\about.html", Content = PageContent });
            context.Pages.Add(new NonProcessedPage { OutputFile = lessFilepath, Content = lessContent, Filepath = lessFilepath });

            // act
            minifier.Transform(context);

            // assert
            Assert.Equal(5, fileSystem.AllPaths.Count());
            Assert.Equal(1, fileSystem.AllPaths.Count(p => p.EndsWith(".css")));
        }


        [Fact]
        public void Should_Not_Process_Already_Existing_Css_File_In_Output_Folder()
        {
            // arrange
            var cssFilePath = @"c:\_site\css\style.css";
            var lessFilepath = @"c:\css\style.less";
            var lessContent = "a { color: Red; }";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { HtmlFilePath, new MockFileData(PageContent)},
                { cssFilePath, MockFileData.NullObject },
                { lessFilepath, new MockFileData(lessContent) }
            });

            var minifier = new LessTransform(fileSystem);
            var context = new SiteContext { SourceFolder = @"C:\", OutputFolder = @"C:\_site" };
            context.Pages.Add(new NonProcessedPage { OutputFile = HtmlFilePath, Content = PageContent });
            context.Pages.Add(new NonProcessedPage { OutputFile = cssFilePath, Content = string.Empty });
            context.Pages.Add(new NonProcessedPage { OutputFile = lessFilepath, Content = lessContent });

            // act
            minifier.Transform(context);

            // assert
            // The existing css file is still empty
            Assert.Equal(string.Empty, fileSystem.File.ReadAllText(cssFilePath));
        }
    }
}
