using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.IO.Abstractions.TestingHelpers;
using Pretzel.Logic.Minification;
using System.IO;

namespace Pretzel.Tests.Minification
{
    public class CssMinificationTests
    {
        private const string _outputPath = @"c:\css\output.css";

        public CssMinificationTests()
        {

        }

        [Fact]
        public void Should_Minify_Single_File()
        {
            var filepath = @"c:\css\style.css";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { filepath, new MockFileData("a { color: Red; }") }
            });

            var files = new List<FileInfo>() { new FileInfo(filepath) };

            var minifier = new CssMinifier(fileSystem, files, _outputPath);

            var result = minifier.ProcessCss(new FileInfo(filepath));

            Assert.Equal("a{color:Red}", result);
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
                { filepath, new MockFileData(lessContent) }
            });

            var files = new List<FileInfo>() { new FileInfo(filepath) };

            var minifier = new CssMinifier(fileSystem, files, _outputPath);
            var minified = minifier.ProcessCss(new FileInfo(filepath));

            Assert.Equal(lessOutput, minified);
        }

        [Fact]
        public void Should_Write_Single_File_To_Output_Path()
        {
            var filepath = @"c:\css\style.css";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { filepath, new MockFileData("a { color: Red; }") }
            });

            var files = new List<FileInfo>() { new FileInfo(filepath) };

            var minifier = new CssMinifier(fileSystem, files, _outputPath);
            minifier.Minify();

            var minifiedFile = fileSystem.File.ReadAllText(_outputPath, Encoding.UTF8);

            Assert.False(string.IsNullOrWhiteSpace(minifiedFile));
        }
        
        [Fact]
        public void Should_Combine_Files_To_Output_Path()
        {
            var filepath1 = @"c:\css\style.css";
            var filepath2 = @"c:\css\style2.css";

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { filepath1, new MockFileData("a { color: Red; }") },
                { filepath2, new MockFileData("a { color: Blue; }") }
            });

            var files = new List<FileInfo>() { new FileInfo(filepath1), new FileInfo(filepath2) };

            var minifier = new CssMinifier(fileSystem, files, _outputPath);
            minifier.Minify();

            var expectedOutput = @"a{color:Red}a{color:Blue}";

            var minifiedFile = fileSystem.File.ReadAllText(_outputPath, Encoding.UTF8);

            Assert.Equal(expectedOutput, minifiedFile);
        }

        [Fact]
        public void Should_Compile_Less_To_Css_To_Output_Path()
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
                { filepath, new MockFileData(lessContent) }
            });

            var files = new List<FileInfo>() { new FileInfo(filepath) };

            var minifier = new CssMinifier(fileSystem, files, _outputPath);
            minifier.Minify();

            var minifiedFile = fileSystem.File.ReadAllText(_outputPath, Encoding.UTF8);

            Assert.Equal(lessOutput, minifiedFile);
        }

        [Fact]
        public void Should_Process_Less_Imports()
        {
            var filepath1 = @"c:\css\style.less";
            var filepath2 = @"c:\css\style2.less";

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { filepath1, new MockFileData("@brand_color: #4D926F;") },
                { filepath2, new MockFileData("@import \"style.less\"; a { color: @brand_color; }") }
            });

            var files = new List<FileInfo>() { new FileInfo(filepath1), new FileInfo(filepath2) };

            var minifier = new CssMinifier(fileSystem, files, _outputPath);

            var expectedOutput = @"a{color:#4D926F}";

            var minifiedFile = minifier.ProcessCss(new FileInfo(filepath2));

            Assert.Equal(expectedOutput, minifiedFile);
        }
    }
}
