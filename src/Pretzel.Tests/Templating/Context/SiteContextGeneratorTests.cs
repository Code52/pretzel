using NSubstitute;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating.Context;
using Pretzel.Tests.Templating.Jekyll;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Extensions;

namespace Pretzel.Tests.Templating.Context
{
    public class SiteContextGeneratorTests
    {
        private readonly SiteContextGenerator generator;
        private readonly MockFileSystem fileSystem;

        public SiteContextGeneratorTests()
        {
            fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
            generator = new SiteContextGenerator(fileSystem, Enumerable.Empty<IContentTransform>(), new LinkHelper());
        }

        [Fact]
        public void site_context_generator_finds_posts()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\_posts\2012-01-01-SomeFile.md", new MockFileData(ToPageContent("# Title")));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            // assert
            Assert.Equal(1, siteContext.Posts.Count);
        }

        [Fact]
        public void site_context_generator_processes_page_markdown()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\_posts\2012-01-01-SomeFile.md", new MockFileData(ToPageContent("# Title")));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            // assert
            Assert.Equal("<h1>Title</h1>", siteContext.Posts[0].Content.Trim());
        }

        [Fact]
        public void posts_do_not_include_pages()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\SubFolder\SomeFile.md", new MockFileData(ToPageContent("# Title")));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            // assert
            Assert.Equal(0, siteContext.Posts.Count);
        }

        [Fact]
        public void posts_without_front_matter_get_processed()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\_posts\SomeFile.md", new MockFileData("# Title"));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            // assert
            Assert.Equal(1, siteContext.Posts.Count);
        }

        [Fact]
        public void posts_without_front_matter_uses_convention_to_render_folder()
        {
            fileSystem.AddFile(@"C:\TestSite\_posts\SomeFile.md", new MockFileData("# Title"));

            var outputPath = string.Format("/{0}/{1}", DateTime.Now.ToString("yyyy'/'MM'/'dd"), "SomeFile.html");

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

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
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            var firstPost = siteContext.Posts.First();

            Assert.Equal(outputPath, firstPost.Url);
        }

        [Fact]
        public void pages_without_front_matter_do_not_get_processed()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\SubFolder\SomeFile.md", new MockFileData("# Title"));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

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
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

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
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            // assert
            Assert.Equal(2, siteContext.Pages[0].DirectoryPages.ToArray().Length);
        }

        [Fact]
        public void site_context_pages_have_correct_url()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\Index.md", new MockFileData(ToPageContent("# Title")));
            fileSystem.AddFile(@"C:\TestSite\SubFolder\SomeFile.md", new MockFileData(ToPageContent("# Title")));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            // assert
            Assert.Equal("/Index.html", siteContext.Pages[0].Url);
            Assert.Equal("/SubFolder/SomeFile.html", siteContext.Pages[1].Url);
        }

        [Fact]
        public void site_context_does_not_cache_page()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\SubFolder\SomeFile.md", new MockFileData(ToPageContent("# Title")));
            generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);
            fileSystem.RemoveFile(@"C:\TestSite\SubFolder\SomeFile.md");
            fileSystem.AddFile(@"C:\TestSite\SubFolder\SomeFile.md", new MockFileData(ToPageContent("# AnotherTitle")));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            // assert
            Assert.True(siteContext.Pages[0].Content.Contains("AnotherTitle"), "Site context should not cache output");
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

            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            Assert.Equal(4, siteContext.Tags.Count());
            Assert.Equal(1, siteContext.Categories.Count());

            var tag1 = siteContext.Tags.First(x => x.Name == "tag1");
            Assert.Equal(1, tag1.Posts.Count());
            Assert.Equal(2, tag1.Posts.First().Tags.Count());
            Assert.True(tag1.Posts.First().File.Contains("File1"));

            var tag2 = siteContext.Tags.First(x => x.Name == "tag2");
            Assert.Equal(2, tag2.Posts.Count());
            Assert.NotNull(tag2.Posts.FirstOrDefault(x => x.File.Contains("File1")));
            Assert.NotNull(tag2.Posts.FirstOrDefault(x => x.File.Contains("File2")));

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

            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            Assert.Equal(4, siteContext.Categories.Count());
            Assert.Equal(1, siteContext.Tags.Count());

            var cat1 = siteContext.Categories.First(x => x.Name == "cat1");
            Assert.Equal(1, cat1.Posts.Count());
            Assert.Equal(2, cat1.Posts.First().Categories.Count());
            Assert.True(cat1.Posts.First().File.Contains("File1"));

            var cat2 = siteContext.Categories.First(x => x.Name == "cat2");
            Assert.Equal(2, cat2.Posts.Count());
            Assert.NotNull(cat2.Posts.FirstOrDefault(x => x.File.Contains("File1")));
            Assert.NotNull(cat2.Posts.FirstOrDefault(x => x.File.Contains("File2")));

            var cat3 = siteContext.Categories.First(x => x.Name == "cat3");
            Assert.Equal(1, cat3.Posts.Count());
            Assert.True(cat3.Posts.First().File.Contains("File2"));

            var cat4 = siteContext.Categories.First(x => x.Name == "cat4");
            Assert.Equal(1, cat4.Posts.Count());
            Assert.Equal(1, cat4.Posts.First().Categories.Count());
            Assert.True(cat4.Posts.First().File.Contains("File3"));
        }

        [Fact]
        public void IsSpecialPath_Scenarios_AreWorking()
        {
            Func<string, bool> function = SiteContextGenerator.IsSpecialPath;

            // underscores are ignored
            Assert.False(function("folder"));
            Assert.True(function("_folder"));

            // .htaccess is included
            Assert.False(function(".htaccess"));
            Assert.True(function(".something-else"));

            // temp files are ignored
            Assert.True(function("some-file.tmp"));
            Assert.True(function("some-file.TMP"));
            Assert.False(function("another-file.bar"));

            // and these ones are causing me headaches in Sublime
            Assert.True(function(@"docs\pages\features\.a4agat3qqt3.tmp"));
        }

        [Fact]
        public void site_context_generator_finds_posts_and_drafts()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\_posts\2012-01-01-SomeFile.md", new MockFileData(ToPageContent("# Title")));
            fileSystem.AddFile(@"C:\TestSite\_drafts\SomeFile.md", new MockFileData(ToPageContent("# Draft")));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", true);

            // assert
            Assert.Equal(2, siteContext.Posts.Count);
        }

        [Theory]
        [InlineData(@"C:\TestSite\2014-01-01-ByFilename.md", false)]
        [InlineData(@"C:\TestSite\UsingDefault.md", true)]
        public void site_context_pages_have_date_in_bag(string fileName, bool useDefault)
        {
            // note - this test does not include the time component.

            // arrange
            var expectedDate = useDefault
                ? DateTime.Now.ToString("yyyy-MM-dd")
                : "2014-01-01";

            fileSystem.AddFile(fileName, new MockFileData(ToPageContent("# Title")));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            // assert
            Assert.True(siteContext.Pages[0].Bag.ContainsKey("date"));
            Assert.IsType<DateTime>(siteContext.Pages[0].Bag["date"]);

            var actualDate = ((DateTime)siteContext.Pages[0].Bag["date"]).ToString("yyyy-MM-dd");

            Assert.Equal(expectedDate, actualDate);
        }

        [Fact]
        public void site_context_generator_processes_page_markdown_mkd()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\_posts\2012-01-01-SomeFile.mkd", new MockFileData(ToPageContent("# Title")));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            // assert
            Assert.Equal("<h1>Title</h1>", siteContext.Posts[0].Content.Trim());
        }

        [Fact]
        public void site_context_generator_processes_page_markdown_mkdn()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\_posts\2012-01-01-SomeFile.mkdn", new MockFileData(ToPageContent("# Title")));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            // assert
            Assert.Equal("<h1>Title</h1>", siteContext.Posts[0].Content.Trim());
        }

        [Fact]
        public void site_context_generator_processes_page_markdown_mdown()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\_posts\2012-01-01-SomeFile.mdown", new MockFileData(ToPageContent("# Title")));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            // assert
            Assert.Equal("<h1>Title</h1>", siteContext.Posts[0].Content.Trim());
        }

        [Fact]
        public void site_context_generator_processes_page_markdown_markdown()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\_posts\2012-01-01-SomeFile.markdown", new MockFileData(ToPageContent("# Title")));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            // assert
            Assert.Equal("<h1>Title</h1>", siteContext.Posts[0].Content.Trim());
        }

        [Fact]
        public void CanBeIncluded_Scenarios_AreWorking()
        {
            Func<string, bool> function = generator.CanBeIncluded;

            // underscores are ignored
            Assert.True(function("folder"));
            Assert.False(function("_folder"));

            // .htaccess is included
            Assert.True(function(".htaccess"));
            Assert.False(function(".something-else"));

            // temp files are ignored
            Assert.False(function("some-file.tmp"));
            Assert.False(function("some-file.TMP"));
            Assert.True(function("another-file.bar"));

            // and these ones are causing me headaches in Sublime
            Assert.False(function(@"docs\pages\features\.a4agat3qqt3.tmp"));
        }

        [Fact]
        public void CanBeIncluded_Scenarios_Include()
        {
            // arrange
            Func<string, bool> function = generator.CanBeIncluded;
            fileSystem.AddFile(@"C:\TestSite\_config.yml", new MockFileData(@"---
include: [_folder, .something-else, some-file.tmp, test\somefile.txt, subfolder\childfolder, anotherfolder\tempfile.tmp]
---"));
            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            // assert
            Assert.True(function("folder"));
            Assert.True(function("_folder"));

            // .htaccess is included
            Assert.True(function(".htaccess"));
            Assert.True(function(".something-else"));

            // temp files specified are included
            Assert.True(function("some-file.tmp"));
            Assert.False(function("some-file.TMP"));
            Assert.True(function("another-file.bar"));

            Assert.True(function("_folder\file.txt"));

            Assert.True(function(@"test\somefile.txt"));
            Assert.True(function(@"subfolder\childfolder"));
            Assert.True(function(@"anotherfolder\tempfile.tmp"));
        }

        [Fact]
        public void CanBeIncluded_Scenarios_Exclude()
        {
            // arrange
            Func<string, bool> function = generator.CanBeIncluded;
            fileSystem.AddFile(@"C:\TestSite\_config.yml", new MockFileData(@"---
exclude: [folder, .htaccess, some-file.tmp, test\somefile.txt, subfolder\childfolder, anotherfolder\tempfile.tmp]
---"));
            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            // assert
            Assert.False(function("folder"));
            Assert.False(function("folder\file.txt"));
            Assert.False(function("_folder"));

            // .htaccess is excluded
            Assert.False(function(".htaccess"));
            Assert.False(function(".something-else"));

            // temp files are ignored
            Assert.False(function("some-file.tmp"));
            Assert.False(function("some-file.TMP"));
            Assert.True(function("another-file.bar"));

            Assert.False(function(@"test\somefile.txt"));
            Assert.False(function(@"subfolder\childfolder"));
            Assert.False(function(@"anotherfolder\tempfile.tmp"));
        }

        [Fact]
        public void CanBeIncluded_Scenarios_IncludeExclude()
        {
            // arrange
            Func<string, bool> function = generator.CanBeIncluded;
            fileSystem.AddFile(@"C:\TestSite\_config.yml", new MockFileData(@"---
include: [_folder, .something-else]
exclude: [folder, test\somefile.txt]
---"));
            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            // underscores are ignored
            Assert.False(function("folder"));
            Assert.True(function("_folder"));
            Assert.True(function("somefolder"));
            Assert.False(function("_somefolder"));

            // .htaccess is included
            Assert.True(function(".htaccess"));
            Assert.True(function(".something-else"));
            Assert.False(function(".pointedfile"));

            // temp files are ignored
            Assert.False(function("some-file.tmp"));
            Assert.False(function("some-file.TMP"));
            Assert.True(function("another-file.bar"));

            // and these ones are causing me headaches in Sublime
            Assert.False(function(@"docs\pages\features\.a4agat3qqt3.tmp"));

            Assert.False(function(@"test\somefile.txt"));
            Assert.True(function(@"subfolder\childfolder"));
            Assert.False(function(@"anotherfolder\tempfile.tmp"));
            Assert.True(function(@"anotherfolder\textfile.txt"));
        }

        [Fact]
        public void permalink_with_numbered_category()
        {
            fileSystem.AddFile(@"C:\TestSite\_posts\SomeFile.md", new MockFileData(@"---
permalink: /blog/:category2/:category1/:category3/:category42/index.html
categories: [cat1, cat2]
---# Title"));

            var outputPath = "/blog/cat2/cat1/index.html";

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            var firstPost = siteContext.Posts.First();

            Assert.Equal(outputPath, firstPost.Url);
        }

        [Fact]
        public void permalink_with_numbered_category_without_categories()
        {
            fileSystem.AddFile(@"C:\TestSite\_posts\SomeFile.md", new MockFileData(@"---
permalink: /blog/:category2/:category1/:category3/:category42/index.html
---# Title"));

            var outputPath = "/blog/index.html";

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            var firstPost = siteContext.Posts.First();

            Assert.Equal(outputPath, firstPost.Url);
        }

        [Fact]
        public void permalink_with_unnumbered_category()
        {
            fileSystem.AddFile(@"C:\TestSite\_posts\SomeFile.md", new MockFileData(@"---
permalink: /blog/:category/index.html
category: cat
---# Title"));

            var outputPath = "/blog/cat/index.html";

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            var firstPost = siteContext.Posts.First();

            Assert.Equal(outputPath, firstPost.Url);
        }

        [Fact]
        public void permalink_with_unnumbered_category_without_categories()
        {
            fileSystem.AddFile(@"C:\TestSite\_posts\SomeFile.md", new MockFileData(@"---
permalink: /blog/:category/index.html
---# Title"));

            var outputPath = "/blog/index.html";

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            var firstPost = siteContext.Posts.First();

            Assert.Equal(outputPath, firstPost.Url);
        }

        [Fact]
        public void site_context_generator_processes_page_id_for_post()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\_posts\2012-01-01-SomeFile.markdown", new MockFileData(ToPageContent("# Title")));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            // assert
            Assert.Equal("/2012/01/01/SomeFile", siteContext.Posts[0].Id);
        }

        [Fact]
        public void site_context_generator_processes_page_id_for_post_with_categories()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\_posts\2012-01-01-SomeFile.markdown", new MockFileData(@"---
categories: [cat1, cat2]
---
# Title"));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            // assert
            Assert.Equal("/cat1/cat2/2012/01/01/SomeFile", siteContext.Posts[0].Id);
        }

        [Fact]
        public void site_context_generator_processes_page_id_for_post_with_categories_and_permalink()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\_posts\2012-01-01-SomeFile.markdown", new MockFileData(@"---
categories: [cat1, cat2]
permalink: /blog/:categories/:year/:month/:day/:title/index.html
---
# Title"));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            // assert
            Assert.Equal("/blog/cat1/cat2/2012/01/01/SomeFile/", siteContext.Posts[0].Id);
        }

        [Fact]
        public void site_context_generator_processes_page_id_for_page()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\about.md", new MockFileData(ToPageContent("# Title")));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            // assert
            Assert.Equal("/about", siteContext.Pages[0].Id);
        }

        [Fact]
        public void site_context_generator_processes_page_id_for_page_with_permalink()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\about.md", new MockFileData(@"---
permalink: /about/
---
# Title"));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            // assert
            Assert.Equal("/about/", siteContext.Pages[0].Id);
        }

        [Fact]
        public void site_context_generator_processes_page_id_for_page_with_override()
        {
            // arrange
            fileSystem.AddFile(@"C:\TestSite\about.md", new MockFileData(@"---
id: my_page_id
---
# Title"));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            // assert
            Assert.Equal("/about", siteContext.Pages[0].Id);
        }

        [Fact]
        public void permalink_with_false_numbered_category()
        {
            fileSystem.AddFile(@"C:\TestSite\_posts\SomeFile.md", new MockFileData(@"---
permalink: /blog/:categorya/index.html
categories: [cat1, cat2]
---# Title"));

            var outputPath = "/blog/cat1a/index.html";

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            var firstPost = siteContext.Posts.First();

            Assert.Equal(outputPath, firstPost.Url);
        }

        [Fact]
        public void empty_file_is_processed_and_have_no_metadata()
        {
            fileSystem.AddFile(@"C:\TestSite\SomeFile.md", MockFileData.NullObject);

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            Assert.Equal(1, siteContext.Pages.Count);
            Assert.Equal(0, siteContext.Pages[0].Bag.Count);
        }

        [Fact]
        public void file_with_published_false_is_not_processed()
        {
            fileSystem.AddFile(@"C:\TestSite\SomeFile.md", new MockFileData(@"---
published: false
---# Title"));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            Assert.Equal(0, siteContext.Pages.Count);
        }

        [Fact]
        public void page_default_values()
        {
            fileSystem.AddFile(@"C:\TestSite\SomeFile.md", new MockFileData(@"---
param: value
---# Title"));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            Assert.Equal(1, siteContext.Pages.Count);
            Assert.Equal("this is a post", siteContext.Pages[0].Title);
            Assert.Equal(DateTime.Now.Date, siteContext.Pages[0].Date.Date);
            Assert.Equal("<h1>Title</h1>", siteContext.Pages[0].Content.TrimEnd());
            Assert.Equal(@"C:\TestSite\_site\SomeFile.md", siteContext.Pages[0].Filepath);
            Assert.Equal(@"C:\TestSite\SomeFile.md", siteContext.Pages[0].File);
            Assert.Equal(2, siteContext.Pages[0].Bag.Count); // param, date
            Assert.Equal("value", siteContext.Pages[0].Bag["param"]);
        }

        [Fact]
        public void page_metadata_values()
        {
            var currentDate = new DateTime(2015, 1, 27).ToShortDateString();
            fileSystem.AddFile(@"C:\TestSite\SomeFile.md", new MockFileData(string.Format(@"---
title: my title
date: {0}
param: value
---# Title",
            currentDate)));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            Assert.Equal(1, siteContext.Pages.Count);
            Assert.Equal("my title", siteContext.Pages[0].Title);
            Assert.Equal(new DateTime(2015, 1, 27), siteContext.Pages[0].Date);
            Assert.Equal("<h1>Title</h1>", siteContext.Pages[0].Content.RemoveWhiteSpace());
            Assert.Equal(@"C:\TestSite\_site\SomeFile.md", siteContext.Pages[0].Filepath);
            Assert.Equal(@"C:\TestSite\SomeFile.md", siteContext.Pages[0].File);
            Assert.Equal(3, siteContext.Pages[0].Bag.Count); // title, date, param
            Assert.Equal("value", siteContext.Pages[0].Bag["param"]);
            Assert.Equal("my title", siteContext.Pages[0].Bag["title"]);
            Assert.Equal(currentDate, siteContext.Pages[0].Bag["date"]);
        }

        [Fact]
        public void page_with_date_in_title()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            var currentDate = new DateTime(2015, 1, 26).ToShortDateString();
            var filePath = string.Format(@"C:\TestSite\{0}-SomeFile.md", currentDate.Replace("/", "-"));
            fileSystem.AddFile(filePath, new MockFileData(string.Format(@"---
param: value
---# Title",
            currentDate)));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            Assert.Equal(1, siteContext.Pages.Count);
            Assert.Equal("this is a post", siteContext.Pages[0].Title);
            Assert.Equal(new DateTime(2015, 1, 26), siteContext.Pages[0].Date);
            Assert.Equal("<h1>Title</h1>", siteContext.Pages[0].Content.RemoveWhiteSpace());
            Assert.Equal(string.Format(@"C:\TestSite\_site\{0}-SomeFile.md", currentDate.Replace("/", "-")), siteContext.Pages[0].Filepath);
            Assert.Equal(filePath, siteContext.Pages[0].File);
            Assert.Equal(2, siteContext.Pages[0].Bag.Count); // param, date
            Assert.Equal("value", siteContext.Pages[0].Bag["param"]);
        }

        [Fact]
        public void page_with_false_date_in_title()
        {
            var currentDate = new DateTime(2015, 1, 26).ToShortDateString();
            var filePath = string.Format(@"C:\TestSite\{0}SomeFile.md", currentDate.Replace("/", "-"));
            fileSystem.AddFile(filePath, new MockFileData(string.Format(@"---
param: value
---# Title",
            currentDate)));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            Assert.Equal(1, siteContext.Pages.Count);
            Assert.Equal("this is a post", siteContext.Pages[0].Title);
            Assert.Equal(DateTime.Now.Date, siteContext.Pages[0].Date.Date);
            Assert.Equal("<h1>Title</h1>", siteContext.Pages[0].Content.RemoveWhiteSpace());
            Assert.Equal(string.Format(@"C:\TestSite\_site\{0}SomeFile.md", currentDate.Replace("/", "-")), siteContext.Pages[0].Filepath);
            Assert.Equal(filePath, siteContext.Pages[0].File);
            Assert.Equal(2, siteContext.Pages[0].Bag.Count); // param, date
            Assert.Equal("value", siteContext.Pages[0].Bag["param"]);
        }

        [Fact]
        public void post_default_values()
        {
            fileSystem.AddFile(@"C:\TestSite\_posts\SomeFile.md", new MockFileData(@"---
param: value
---# Title"));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            Assert.Equal(1, siteContext.Posts.Count);
            Assert.Equal("this is a post", siteContext.Posts[0].Title);
            Assert.Equal(DateTime.Now.Date, siteContext.Posts[0].Date.Date);
            Assert.Equal("<h1>Title</h1>", siteContext.Posts[0].Content.RemoveWhiteSpace());
            Assert.Equal(@"C:\TestSite\_site\SomeFile.md", siteContext.Posts[0].Filepath);
            Assert.Equal(@"C:\TestSite\_posts\SomeFile.md", siteContext.Posts[0].File);
            Assert.Equal(2, siteContext.Posts[0].Bag.Count); // param, date
            Assert.Equal("value", siteContext.Posts[0].Bag["param"]);
        }

        [Fact]
        public void post_metadata_values()
        {
            var currentDate = new DateTime(2015, 1, 27).ToShortDateString();
            fileSystem.AddFile(@"C:\TestSite\_posts\SomeFile.md", new MockFileData(string.Format(@"---
title: my title
date: {0}
param: value
---# Title",
            currentDate)));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            Assert.Equal(1, siteContext.Posts.Count);
            Assert.Equal("my title", siteContext.Posts[0].Title);
            Assert.Equal(new DateTime(2015, 1, 27), siteContext.Posts[0].Date);
            Assert.Equal("<h1>Title</h1>", siteContext.Posts[0].Content.RemoveWhiteSpace());
            Assert.Equal(@"C:\TestSite\_site\SomeFile.md", siteContext.Posts[0].Filepath);
            Assert.Equal(@"C:\TestSite\_posts\SomeFile.md", siteContext.Posts[0].File);
            Assert.Equal(3, siteContext.Posts[0].Bag.Count); // title, date, param
            Assert.Equal("value", siteContext.Posts[0].Bag["param"]);
            Assert.Equal("my title", siteContext.Posts[0].Bag["title"]);
            Assert.Equal(currentDate, siteContext.Posts[0].Bag["date"]);
        }

        [Fact]
        public void post_with_date_in_title()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            var currentDate = new DateTime(2015, 1, 26).ToShortDateString();
            var filePath = string.Format(@"C:\TestSite\_posts\{0}-SomeFile.md", currentDate.Replace("/", "-"));
            fileSystem.AddFile(filePath, new MockFileData(string.Format(@"---
param: value
---# Title",
            currentDate)));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            Assert.Equal(1, siteContext.Posts.Count);
            Assert.Equal("this is a post", siteContext.Posts[0].Title);
            Assert.Equal(new DateTime(2015, 1, 26), siteContext.Posts[0].Date);
            Assert.Equal("<h1>Title</h1>", siteContext.Posts[0].Content.RemoveWhiteSpace());
            Assert.Equal(string.Format(@"C:\TestSite\_site\{0}\SomeFile.md", currentDate.Replace("/", "\\")), siteContext.Posts[0].Filepath);
            Assert.Equal(filePath, siteContext.Posts[0].File);
            Assert.Equal(2, siteContext.Posts[0].Bag.Count); // param, date
            Assert.Equal("value", siteContext.Posts[0].Bag["param"]);
        }

        [Fact]
        public void post_with_false_date_in_title()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            var currentDate = new DateTime(2015, 1, 26).ToShortDateString();
            var filePath = string.Format(@"C:\TestSite\_posts\{0}SomeFile.md", currentDate.Replace("/", "-"));
            fileSystem.AddFile(filePath, new MockFileData(string.Format(@"---
param: value
---# Title",
            currentDate)));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            Assert.Equal(1, siteContext.Posts.Count);
            Assert.Equal("this is a post", siteContext.Posts[0].Title);
            Assert.Equal(DateTime.Now.Date, siteContext.Posts[0].Date.Date);
            Assert.Equal("<h1>Title</h1>", siteContext.Posts[0].Content.RemoveWhiteSpace());
            Assert.Equal(string.Format(@"C:\TestSite\_site\{0}SomeFile.md", currentDate.Replace("/", "\\")), siteContext.Posts[0].Filepath);
            Assert.Equal(filePath, siteContext.Posts[0].File);
            Assert.Equal(2, siteContext.Posts[0].Bag.Count); // param, date
            Assert.Equal("value", siteContext.Posts[0].Bag["param"]);
        }

        [Fact]
        public void page_with_false_date_is_not_processed()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            fileSystem.AddFile(@"C:\TestSite\SomeFile.md", new MockFileData(@"---
date: 20150127
---# Title"));
            StringBuilder sb = new StringBuilder();
            TextWriter writer = new StringWriter(sb);
            Tracing.Logger.SetWriter(writer);
            Tracing.Logger.AddCategory("info");
            Tracing.Logger.AddCategory("debug");

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            Assert.Equal(0, siteContext.Pages.Count);
            Assert.Contains(@"Failed to build post from File: C:\TestSite\SomeFile.md", sb.ToString());
            Assert.Contains(@"String was not recognized as a valid DateTime.", sb.ToString());
            Assert.Contains(@"System.FormatException: String was not recognized as a valid DateTime.", sb.ToString());
        }

        [Fact]
        public void render_with_ContentTransformer_should_transform_content()
        {
            fileSystem.AddFile(@"C:\TestSite\SomeFile.md", new MockFileData(@"---
---# Title
[foo]"));
            var contentTransformer = Substitute.For<IContentTransform>();
            contentTransformer.Transform(Arg.Any<string>()).Returns(s => s[0].ToString().Replace("[foo]", "bar"));

            var generator = new SiteContextGenerator(fileSystem, new[] { contentTransformer }, new LinkHelper());

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            Assert.Equal(1, siteContext.Pages.Count);
            Assert.Equal("<h1>Title</h1><p>bar</p>", siteContext.Pages[0].Content.RemoveWhiteSpace());
        }

        [Fact]
        public void render_with_ContentTransformer_exception_should_trace_the_error()
        {
            fileSystem.AddFile(@"C:\TestSite\SomeFile.md", new MockFileData("---\r\n---# Title\r\n[foo]"));
            StringBuilder sb = new StringBuilder();
            TextWriter writer = new StringWriter(sb);
            Tracing.Logger.SetWriter(writer);
            Tracing.Logger.AddCategory("info");
            Tracing.Logger.AddCategory("debug");

            var contentTransformer = Substitute.For<IContentTransform>();
            contentTransformer.Transform(Arg.Any<string>()).Returns(s => { throw new Exception("foo bar"); });

            var generator = new SiteContextGenerator(fileSystem, new[] { contentTransformer }, new LinkHelper());

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            Assert.Equal(1, siteContext.Pages.Count);
            Assert.Equal("<p><b>Error converting markdown</b></p><pre>---\r\n---# Title\r\n[foo]</pre>", siteContext.Pages[0].Content);
            Assert.Contains(@"Error (foo bar) converting C:\TestSite\SomeFile.md", sb.ToString());
            Assert.Contains(@"System.Exception: foo bar", sb.ToString());
        }

        [Fact]
        public void file_with_1_ioexception_on_ReadAllText_is_processed()
        {
            // arrange
            string filePath = Path.Combine(Path.GetTempPath(), "SomeFile.md");
            bool alreadyOccured = false;
            var fileSubstitute = Substitute.For<FileBase>();
            fileSubstitute.OpenText(Arg.Any<string>()).Returns(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes("---"))));
            fileSubstitute.ReadAllText(Arg.Any<string>()).Returns(x =>
            {
                if (alreadyOccured)
                {
                    return "---\r\n---# Title";
                }
                else
                {
                    alreadyOccured = true;
                    throw new IOException();
                }
            });
            fileSubstitute.Exists(filePath).Returns(true);

            var directorySubstitute = Substitute.For<DirectoryBase>();
            directorySubstitute.GetFiles(Arg.Any<string>(), "*.*", SearchOption.AllDirectories).Returns(new[] { @"C:\TestSite\SomeFile.md" });

            var fileInfoSubstitute = Substitute.For<FileInfoBase>();
            fileInfoSubstitute.Name.Returns("SomeFile.md");

            var fileInfoFactorySubstitute = Substitute.For<IFileInfoFactory>();
            fileInfoFactorySubstitute.FromFileName(Arg.Any<string>()).Returns(fileInfoSubstitute);

            var fileSystemSubstitute = Substitute.For<System.IO.Abstractions.IFileSystem>();
            fileSystemSubstitute.File.Returns(fileSubstitute);
            fileSystemSubstitute.Directory.Returns(directorySubstitute);
            fileSystemSubstitute.FileInfo.Returns(fileInfoFactorySubstitute);

            var generator = new SiteContextGenerator(fileSystemSubstitute, Enumerable.Empty<IContentTransform>(), new LinkHelper());

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            // assert
            Assert.Equal(1, siteContext.Pages.Count);
            Assert.Equal("<h1>Title</h1>", siteContext.Pages[0].Content.RemoveWhiteSpace());
            // Check if the temp file have been deleted
            fileSubstitute.Received().Delete(filePath);
        }

        [Fact]
        public void file_with_2_ioexception_on_ReadAllText_is_not_processed_and_exception_traced()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            // arrange
            string filePath = Path.Combine(Path.GetTempPath(), "SomeFile.md");
            var fileSubstitute = Substitute.For<FileBase>();
            fileSubstitute.OpenText(Arg.Any<string>()).Returns(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes("---"))));
            fileSubstitute.ReadAllText(Arg.Any<string>()).Returns(x =>
            {
                throw new IOException();
            });
            fileSubstitute.Exists(filePath).Returns(true);

            var directorySubstitute = Substitute.For<DirectoryBase>();
            directorySubstitute.GetFiles(Arg.Any<string>(), "*.*", SearchOption.AllDirectories).Returns(new[] { @"C:\TestSite\SomeFile.md" });

            var fileInfoSubstitute = Substitute.For<FileInfoBase>();
            fileInfoSubstitute.Name.Returns("SomeFile.md");

            var fileInfoFactorySubstitute = Substitute.For<IFileInfoFactory>();
            fileInfoFactorySubstitute.FromFileName(Arg.Any<string>()).Returns(fileInfoSubstitute);

            var fileSystemSubstitute = Substitute.For<System.IO.Abstractions.IFileSystem>();
            fileSystemSubstitute.File.Returns(fileSubstitute);
            fileSystemSubstitute.Directory.Returns(directorySubstitute);
            fileSystemSubstitute.FileInfo.Returns(fileInfoFactorySubstitute);

            StringBuilder sb = new StringBuilder();
            TextWriter writer = new StringWriter(sb);
            Tracing.Logger.SetWriter(writer);
            Tracing.Logger.AddCategory("info");
            Tracing.Logger.AddCategory("debug");

            var generator = new SiteContextGenerator(fileSystemSubstitute, Enumerable.Empty<IContentTransform>(), new LinkHelper());

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            // assert
            Assert.Equal(0, siteContext.Pages.Count);
            Assert.Contains(@"Failed to build post from File: C:\TestSite\SomeFile.md", sb.ToString());
            Assert.Contains(@"I/O error occurred.", sb.ToString());
            Assert.Contains(@"System.IO.IOException: I/O error occurred.", sb.ToString());
            // Check if the temp file have been deleted
            fileSubstitute.Received().Delete(filePath);
        }

        [Fact]
        public void RemoveDiacritics_should_remove_any_accents()
        {
            Assert.Equal("the cat is running & getting away", SiteContextGenerator.RemoveDiacritics("The cát ís running & getting away"));
        }

        [Fact]
        public void RemoveDiacritics_should_return_null_if_input_null()
        {
            Assert.Equal(null, SiteContextGenerator.RemoveDiacritics(null));
        }

        [Fact]
        public void permalink_with_folder_categories()
        {
            fileSystem.AddFile(@"C:\TestSite\foo\bar\_posts\2015-03-09-SomeFile.md", new MockFileData(@"---
categories: [cat1, cat2]
---# Title"));
            var outputPath = "/foo/bar/cat1/cat2/2015/03/09/SomeFile.html";
            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);
            var firstPost = siteContext.Posts.First();
            Assert.Equal(outputPath, firstPost.Url);
        }

        [InlineData("date", "/cat1/cat2/2015/03/09/foobar-baz.html", "cat1,cat2")]
        [InlineData("date", "/2015/03/09/foobar-baz.html", "")]
        [InlineData("/:dashcategories/:year/:month/:day/:title.html", "/cat1-cat2/2015/03/09/foobar-baz.html", "cat1,cat2")]
        [InlineData("/:dashcategories/:year/:month/:day/:title.html", "/2015/03/09/foobar-baz.html", "")]
        [InlineData("pretty", "/cat1/cat2/2015/03/09/foobar-baz/", "cat1,cat2")]
        [InlineData("ordinal", "/cat1/cat2/2015/068/foobar-baz.html", "cat1,cat2")]
        [InlineData("none", "/cat1/cat2/foobar-baz.html", "cat1,cat2")]
        [InlineData("/:categories/:short_year/:i_month/:i_day/:title.html", "/cat1/cat2/15/3/9/foobar-baz.html", "cat1,cat2")]
        [InlineData("/:category/:title.html", "/cat1/foobar-baz.html", "cat1,cat2")]
        [InlineData("/:category/:title.html", "/foobar-baz.html", "")]
        [InlineData("/:category1/:title.html", "/cat1/foobar-baz.html", "cat1,cat2")]
        [InlineData("/:category2/:title.html", "/cat2/foobar-baz.html", "cat1,cat2")]
        [InlineData("/:category3/:title.html", "/foobar-baz.html", "cat1,cat2")]
        [Theory]
        public void permalink_is_well_formatted(string permalink, string expectedUrl, string categories)
        {
            fileSystem.AddFile(@"C:\TestSite\_config.yml", new MockFileData(string.Format("permalink: {0}", permalink)));
            fileSystem.AddFile(@"C:\TestSite\_posts\2015-03-09-foobar-baz.md", new MockFileData(string.Format(@"---
categories: [{0}]
---# Title", categories)));

            // act
            var siteContext = generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);
            var firstPost = siteContext.Posts.First();
            Assert.Equal(expectedUrl, firstPost.Url);
        }
    }
}
