using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Pretzel.Logic.Templating.Context;
using Xunit;

namespace Pretzel.Tests.Templating.Context
{
    public class SiteContextGeneratorTests
    {
        private readonly SiteContextGenerator generator;
        private readonly MockFileSystem fileSystem;

        public SiteContextGeneratorTests()
        {
            fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
            generator = new SiteContextGenerator(fileSystem);
        }

        [Fact]
        public void site_context_generator_finds_posts()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\_posts\2012-01-01-SomeFile.md", new MockFileData(ToPageContent("# Title")));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite");

            // assert
            Assert.Equal(1, siteContext.Posts.Count);
        }

        [Fact]
        public void site_context_generator_processes_page_markdown()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\_posts\2012-01-01-SomeFile.md", new MockFileData(ToPageContent("# Title")));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite");

            // assert
            Assert.Equal("<h1>Title</h1>", siteContext.Posts[0].Content.Trim());
        }

        [Fact]
        public void posts_do_not_include_pages()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\SubFolder\SomeFile.md", new MockFileData(ToPageContent("# Title")));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite");

            // assert
            Assert.Equal(0, siteContext.Posts.Count);
        }

        private static string ToPageContent(string content)
        {
            return @"---
title: Title
---" + content;
        }
    }
}