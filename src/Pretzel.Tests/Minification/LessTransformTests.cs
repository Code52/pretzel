using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using Pretzel.Logic.Minification;
using Pretzel.Logic.Templating.Context;
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
    }
}
