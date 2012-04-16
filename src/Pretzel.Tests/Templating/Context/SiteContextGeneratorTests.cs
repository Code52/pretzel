using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
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

        [Fact]
        public void posts_without_front_matter_get_processed()
        {

            // arrange
            fileSystem.AddFile(@"C:\TestSite\_posts\SomeFile.md", new MockFileData("# Title"));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite");

            // assert
            Assert.Equal(1, siteContext.Posts.Count);
        }

        [Fact]
        public void posts_without_front_matter_uses_convention_to_render_folder()
        {
            fileSystem.AddFile(@"C:\TestSite\_posts\SomeFile.md", new MockFileData("# Title"));

            var outputPath = string.Format("/{0}/{1}", DateTime.Now.ToString("yyyy'/'MM'/'dd"), "SomeFile.html");

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite");

            var firstPost = siteContext.Posts.First();

            Assert.Equal(outputPath, firstPost.Url);
        }

        [Fact]
        public void posts_without_front_matter_and_override_config_renders_folder()
        {
            fileSystem.AddFile(@"C:\TestSite\_posts\SomeFile.md", new MockFileData("# Title"));
            fileSystem.AddFile(@"C:\TestSite\_config.yml", new MockFileData("permalink: /blog/:year/:month/:day/:title.html"));

            var outputPath = string.Format("/blog/{0}/{1}", DateTime.Now.ToString("yyyy'/'MM'/'dd"), "SomeFile.html");

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite");

            var firstPost = siteContext.Posts.First();

            Assert.Equal(outputPath, firstPost.Url);
        }

        [Fact]
        public void pages_without_front_matter_do_not_get_processed()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\SubFolder\SomeFile.md", new MockFileData("# Title"));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite");

            // assert
            Assert.Equal(1, siteContext.Pages.Count);
            Assert.IsType<NonProcessedPage>(siteContext.Pages[0]);
        }

        [Fact]
        public void pages_with_front_matter_get_processed()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\SubFolder\SomeFile.md", new MockFileData(ToPageContent("# Title")));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite");

            // assert
            Assert.Equal(1, siteContext.Pages.Count);
            Assert.IsType<Page>(siteContext.Pages[0]);
        }

        [Fact]
        public void site_context_includes_pages_in_same_folder()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\SubFolder\SomeFile.md", new MockFileData(ToPageContent("# Title")));
            fileSystem.AddFile(@"C:\TestSite\SubFolder\SomeFile2.md", new MockFileData(ToPageContent("# Title")));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite");

            // assert
            Assert.Equal(2, siteContext.Pages[0].DirectoryPages.ToArray().Length);
        }

        private static string ToPageContent(string content)
        {
            return @"---
title: Title
---" + content;
        }

        [Fact]
        public void Generator_Extracts_Tags_From_Posts()
        {
            fileSystem.AddFile(@"C:\TestSite\_posts\2012-01-01-File1.md", new MockFileData("---\n\r tags: [\"tag1\",\"tag2\"]\n\r---\n\r Test"));
            fileSystem.AddFile(@"C:\TestSite\_posts\2012-01-02-File2.md", new MockFileData("---\n\r tags: [\"tag2\",\"tag3\"]\n\r---\n\r Test"));
            fileSystem.AddFile(@"C:\TestSite\_posts\2012-01-03-File3.md", new MockFileData("---\n\r tags: [\"tag4\"]\n\r categories: [\"cat1\"]\n\r---\n\r Test"));
            fileSystem.AddFile(@"C:\TestSite\page.md", new MockFileData("---\n\r tags: [\"tag2\",\"tag3\"]\n\r categories: [\"cat1\"]\n\r---\n\r Test"));

            var siteContext = generator.BuildContext(@"C:\TestSite");

            Assert.Equal(4, siteContext.Tags.Count());
            Assert.Equal(1, siteContext.Categories.Count());
            
            var tag1 = siteContext.Tags.First(x => x.Name == "tag1");
            Assert.Equal(1, tag1.Posts.Count());
            Assert.Equal(2, tag1.Posts.First().Tags.Count());
            Assert.True(tag1.Posts.First().File.Contains("File1"));

            var tag2 = siteContext.Tags.First(x => x.Name == "tag2");
            Assert.Equal(2, tag2.Posts.Count());
            Assert.NotNull(tag2.Posts.FirstOrDefault(x=>x.File.Contains("File1")));
            Assert.NotNull(tag2.Posts.FirstOrDefault(x=>x.File.Contains("File2")));
            
            var tag3 = siteContext.Tags.First(x => x.Name == "tag3");
            Assert.Equal(1, tag3.Posts.Count());
            Assert.True(tag3.Posts.First().File.Contains("File2"));
            
            var tag4 = siteContext.Tags.First(x => x.Name == "tag4");
            Assert.Equal(1, tag4.Posts.Count());
            Assert.Equal(1, tag4.Posts.First().Tags.Count());
            Assert.True(tag4.Posts.First().File.Contains("File3"));
        }

        [Fact]
        public void Generator_Extracts_Categories_From_Posts()
        {
            fileSystem.AddFile(@"C:\TestSite\_posts\2012-01-01-File1.md", new MockFileData("---\n\r categories: [\"cat1\",\"cat2\"]\n\r---\n\r Test"));
            fileSystem.AddFile(@"C:\TestSite\_posts\2012-01-02-File2.md", new MockFileData("---\n\r categories: [\"cat2\",\"cat3\"]\n\r---\n\r Test"));
            fileSystem.AddFile(@"C:\TestSite\_posts\2012-01-03-File3.md", new MockFileData("---\n\r categories: [\"cat4\"]\n\r tags: [\"tag1\"]\n\r---\n\r Test"));
            fileSystem.AddFile(@"C:\TestSite\page.md", new MockFileData("---\n\r categories: [\"cat2\",\"cat3\"]\n\r tags: [\"tag1\"]\n\r---\n\r Test"));

            var siteContext = generator.BuildContext(@"C:\TestSite");

            Assert.Equal(4, siteContext.Categories.Count());
            Assert.Equal(1, siteContext.Tags.Count());
            
            var cat1 = siteContext.Categories.First(x => x.Name == "cat1");
            Assert.Equal(1, cat1.Posts.Count());
            Assert.Equal(2, cat1.Posts.First().Categories.Count());
            Assert.True(cat1.Posts.First().File.Contains("File1"));

            var cat2 = siteContext.Categories.First(x => x.Name == "cat2");
            Assert.Equal(2, cat2.Posts.Count());
            Assert.NotNull(cat2.Posts.FirstOrDefault(x=>x.File.Contains("File1")));
            Assert.NotNull(cat2.Posts.FirstOrDefault(x=>x.File.Contains("File2")));

            var cat3 = siteContext.Categories.First(x => x.Name == "cat3");
            Assert.Equal(1, cat3.Posts.Count());
            Assert.True(cat3.Posts.First().File.Contains("File2"));

            var cat4 = siteContext.Categories.First(x => x.Name == "cat4");
            Assert.Equal(1, cat4.Posts.Count());
            Assert.Equal(1, cat4.Posts.First().Categories.Count());
            Assert.True(cat4.Posts.First().File.Contains("File3"));
        }
    }
}