﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Import;
using Xunit;

namespace Pretzel.Tests.Import
{
    public class TumblrImportTests
    {
        private readonly MockFileSystem mockFileSystem;
        private readonly Func<string, string> mockWebClient;
        private readonly TumblrImport tumblrImport;

        private readonly Dictionary<string, string> remote = new Dictionary<string, string> {
            {"http://www.domain.com:80/api/read?start=0&num=10", 
                @"<?xml version=""1.0"" encoding=""UTF-8""?>
                <tumblr version=""1.0"">
	                <tumblelog name=""tonyedgecombe"" timezone=""Europe/Dublin"" cname=""www.domain.com"" title=""Blog Title"" />
	                <posts start=""0"" total=""1"">
		                <post id=""22834628296"" 
                                url=""http://www.domain.com/post/12345678"" 
                                url-with-slug=""http://www.domain.com/post/12345678/title"" 
                                type=""regular"" 
                                date-gmt=""2012-05-11 10:07:04 GMT"" 
                                date=""Fri, 11 May 2012 11:07:04"" 
                                unix-timestamp=""1336730824"" 
                                format=""markdown"" 
                                reblog-key=""D6faGdnt"" 
                                slug=""slug"">
			                <regular-title>First Title</regular-title>
			                <regular-body>First Body</regular-body>
		                </post>
	                </posts>
                </tumblr>"}
        }; 

        public TumblrImportTests()
        {
            mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
            mockWebClient = s => remote[s];

            tumblrImport = new TumblrImport(mockFileSystem, mockWebClient, "C:\\imported", "www.domain.com");
        }

        [Fact]
        public void Posts_Are_Imported()
        {
            string path = @"C:\imported\_posts\2012-05-11-First-Title.md";
            tumblrImport.Import();

            Assert.True(mockFileSystem.File.Exists(path));

            var result = mockFileSystem.File.ReadAllLines(path);
            Assert.Equal("---", result[0]);
            Assert.Equal("title: First Title", result[1]);
            Assert.Equal("date: 2012-05-11", result[2]);
            Assert.Equal("layout: post", result[3]);
            Assert.Equal("permalink: /post/12345678/title/index.html", result[4]);
            Assert.Equal("---", result[5]);
        }
    }
}
