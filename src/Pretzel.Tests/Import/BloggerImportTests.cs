using System;
using System.Collections.Generic;
using Xunit;
using System.IO.Abstractions.TestingHelpers;
using Pretzel.Logic.Import;
using Pretzel.Logic.Extensions;

namespace Pretzel.Tests.Import
{
    public class BloggerImportTests
    {
        const string BaseSite = @"c:\site\";
        const string ImportFile = @"c:\import.xml";
        string ImportContent;

        public BloggerImportTests()
        {
            ImportContent = System.IO.File.ReadAllText(@"C:\Users\mheath\Downloads\blog-02-14-2012.xml");
        }

        [Fact]
        public void Posts_Are_Imported()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { ImportFile, new MockFileData(ImportContent) }
            });

            var bloggerImporter = new BloggerImport(fileSystem, BaseSite, ImportFile);
            bloggerImporter.Import();

            string expectedPost = @"_posts\2012-01-27-Handling-multi-channel-audio-in-NAudio.md";
            Assert.True(fileSystem.File.Exists(BaseSite + expectedPost));

            var postContent = fileSystem.File.ReadAllText(BaseSite + expectedPost);
            var header = postContent.YamlHeader();

            Assert.Equal("Handling multi-channel audio in NAudio", header["title"].ToString());
        }
    }
}
