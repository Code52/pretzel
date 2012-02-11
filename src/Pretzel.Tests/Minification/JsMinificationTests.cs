using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Abstractions.TestingHelpers;
using System.IO;
using Pretzel.Logic.Minification;
using Xunit;

namespace Pretzel.Tests.Minification
{
    public class JsMinificationTests
    {
        private const string _outputPath = @"c:\css\output.js";

        [Fact]
        public void Should_Minify_Single_File()
        {
            var filepath = @"c:\css\script.js";
            var script = "function test() { alert(\"hello\"); }";

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { filepath, new MockFileData(script) }
            });

            var files = new List<FileInfo> { new FileInfo(filepath) };

            var minifier = new JsMinifier(fileSystem, files, _outputPath);
            minifier.Minify();

            var minifiedFile = fileSystem.File.ReadAllText(_outputPath, Encoding.UTF8);

            Assert.Equal("function test(){alert(\"hello\")}", minifiedFile);
        }

        [Fact]
        public void Should_Combine_Files_To_Output_Path()
        {
            var filepath1 = @"c:\css\script1.js";
            var script1 = @"function test() { alert('hello'); }";

            var filepath2 = @"c:\css\script2.css";
            var script2 = @"document.write('<h1>This is a heading</h1>');
document.write('<p>This is a paragraph.</p>');
document.write('<p>This is another paragraph.</p>');";

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { filepath1, new MockFileData(script1) },
                { filepath2, new MockFileData(script2) }
            });

            var files = new List<FileInfo> { new FileInfo(filepath1), new FileInfo(filepath2) };

            var minifier = new JsMinifier(fileSystem, files, _outputPath);
            minifier.Minify();

            var expectedOutput = "function test(){alert(\"hello\")}document.write(\"<h1>This is a heading</h1>\"),document.write(\"<p>This is a paragraph.</p>\"),document.write(\"<p>This is another paragraph.</p>\")";

            var minifiedFile = fileSystem.File.ReadAllText(_outputPath, Encoding.UTF8);

            Assert.Equal(expectedOutput, minifiedFile);
        }
    }
}
