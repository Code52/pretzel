using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensibility.Extensions;
using Pretzel.Logic.Templating.Context;
using Pretzel.Logic.Templating.Razor;
using Pretzel.Tests.Templating.Jekyll;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Xunit;

namespace Pretzel.Tests.Templating.Razor
{
    public class RazorTests : BakingEnvironment<RazorSiteEngine>
    {
        public override RazorSiteEngine Given()
        {
            var engine = new RazorSiteEngine();
            engine.Initialize();
            return engine;
        }

        public override void When()
        {
        }

        private void ProcessContents(string layout, string content, Dictionary<string, object> bag)
        {
            FileSystem.AddFile(@"C:\website\_layouts\Test.cshtml", new MockFileData(layout));
            var context = new SiteContext { SourceFolder = @"C:\website\", OutputFolder = @"C:\website\_site", Title = "My Web Site" };
            bag.Add("layout", "Test");
            context.Posts.Add(new Page { File = "index.cshtml", Content = content, OutputFile = @"C:\website\_site\index.html", Bag = bag });
            FileSystem.AddFile(@"C:\website\index.cshtml", new MockFileData(layout));
            Subject.FileSystem = FileSystem;
            Subject.Process(context);
        }

        [Fact]
        public void File_with_no_replacements_is_unaltered()
        {
            const string fileContents = "<html><head><title></title></head><body></body></html>";
            ProcessContents(fileContents, string.Empty, new Dictionary<string, object>());
            Assert.Equal(fileContents, FileSystem.File.ReadAllText(@"C:\website\_site\index.html"));
        }

        [Fact]
        public void File_with_title_is_processed()
        {
            const string fileContents = "<html><head><title>@Model.Title</title></head><body></body></html>";
            const string title = "This is the title!";

            var bag = new Dictionary<string, object> { { "title", title } };
            ProcessContents(fileContents, string.Empty, bag);
            var expected = fileContents.Replace(@"@Model.Title", title);
            var output = FileSystem.File.ReadAllText(@"C:\website\_site\index.html");
            Assert.Equal(expected, output);
        }

        [Fact]
        public void File_with_content_is_processed()
        {
            const string templateContents = "<html><head><title>@Model.Title</title></head><body>@Raw(Model.Content)</body></html>";
            const string pageContents = "<h1>Hello World!</h1>";
            const string expectedfileContents = "<html><head><title>My Web Site</title></head><body><h1>Hello World!</h1></body></html>";

            ProcessContents(templateContents, pageContents, new Dictionary<string, object> { { "title", "My Web Site" } });
            var output = FileSystem.File.ReadAllText(@"C:\website\_site\index.html");
            Assert.Equal(expectedfileContents, output);
        }

        [Fact]
        public void File_with_include_is_processed()
        {
            const string templateContents = "<html><head><title>@Model.Title</title></head><body>@Raw(Model.Content)</body></html>";
            const string pageContents = "<i>@Include(\"TestInclude\")</i>";
            const string layoutContents = "<b>Included!</b>";
            const string expectedfileContents = "<html><head><title>My Web Site</title></head><body><i><b>Included!</b></i></body></html>";

            FileSystem.AddFile(@"C:\website\_includes\TestInclude.cshtml", new MockFileData(layoutContents));
            ProcessContents(templateContents, pageContents, new Dictionary<string, object> { { "title", "My Web Site" } });
            string output = FileSystem.File.ReadAllText(@"C:\website\_site\index.html");
            Assert.Equal(expectedfileContents, output);
        }

        [Fact]
        public void File_with_include_and_model_is_processed()
        {
            const string templateContents = "<html><head><title>@Model.Title</title></head><body>@Raw(Model.Content)</body></html>";
            const string pageContents = "<i>@Include(\"TestInclude\", @Model.Title)</i>";
            const string layoutContents = "<b>Included from @Model!</b>";
            const string expectedfileContents = "<html><head><title>My Web Site</title></head><body><i><b>Included from My Web Site!</b></i></body></html>";

            FileSystem.AddFile(@"C:\website\_includes\TestInclude.cshtml", new MockFileData(layoutContents));
            ProcessContents(templateContents, pageContents, new Dictionary<string, object> { { "title", "My Web Site" } });
            string output = FileSystem.File.ReadAllText(@"C:\website\_site\index.html");
            Assert.Equal(expectedfileContents, output);
        }

        [Fact]
        public void File_with_include_but_missing_is_processed()
        {
            const string templateContents = "<html><head><title>@Model.Title</title></head><body>@Raw(Model.Content)</body></html>";
            const string pageContents = "<i>@Include(\"TestInclude\")</i>";
            const string expectedfileContents = "<html><head><title>My Web Site</title></head><body><i></i></body></html>";
            ProcessContents(templateContents, pageContents, new Dictionary<string, object> { { "title", "My Web Site" } });

            var output = FileSystem.File.ReadAllText(@"C:\website\_site\index.html");
            Assert.Equal(expectedfileContents, output);
        }

        [Fact]
        public void File_with_extension_is_processed()
        {
            const string templateContents = "<html><body>@Raw(Model.Content) @Filter.Slugify(\".ASP.NET MVC\")</body></html>";
            const string pageContents = "<h1>Hello</h1>";
            const string expectedfileContents = "<html><body><h1>Hello</h1> asp-net-mvc</body></html>";

            Subject.Filters = new IFilter[] { new SlugifyFilter() };
            ProcessContents(templateContents, pageContents, new Dictionary<string, object>());
            var output = FileSystem.File.ReadAllText(@"C:\website\_site\index.html");
            Assert.Equal(expectedfileContents, output);
        }

        [Fact]
        public void Filter_PrettifyUrl_is_processed()
        {
            const string templateContents = "<html><body>@Raw(Model.Content) @Filter.PrettifyUrl(\"http://mysite.com/index.html\")</body></html>";
            const string pageContents = "<h1>Hello</h1>";
            const string expectedfileContents = "<html><body><h1>Hello</h1> http://mysite.com/</body></html>";

            Subject.Filters = new IFilter[] { new PrettifyUrlFilter() };
            ProcessContents(templateContents, pageContents, new Dictionary<string, object>());
            var output = FileSystem.File.ReadAllText(@"C:\website\_site\index.html");
            Assert.Equal(expectedfileContents, output);
        }

        [Fact]
        public void Comments_true_is_processed_correctly()
        {
            // arrange
            const string fileContents = "<html><head><title>Some title</title></head><body>@if (Model.Comments){<span>Comments is true</span>}</body></html>";

            var bag = new Dictionary<string, object> { { "comments", true } };
            ProcessContents(fileContents, string.Empty, bag);
            var expected = "<html><head><title>Some title</title></head><body><span>Comments is true</span></body></html>";

            // act
            var output = FileSystem.File.ReadAllText(@"C:\website\_site\index.html");

            // assert
            Assert.Equal(expected, output);
        }

        [Fact]
        public void Comments_false_is_processed_correctly()
        {
            // arrange
            const string fileContents = "<html><head><title>Some title</title></head><body>@if (Model.Comments){ <span>Comments is true</span> }</body></html>";

            var bag = new Dictionary<string, object> { { "comments", false } };
            ProcessContents(fileContents, string.Empty, bag);
            var expected = "<html><head><title>Some title</title></head><body></body></html>";

            // act
            var output = FileSystem.File.ReadAllText(@"C:\website\_site\index.html");

            // assert
            Assert.Equal(expected, output);
        }

        [Fact]
        public void Comments_inexisting_is_processed_correctly()
        {
            // arrange
            const string fileContents = "<html><head><title>Some title</title></head><body>@if (Model.Comments){ <span>Comments is true</span> }</body></html>";

            var bag = new Dictionary<string, object> { };
            ProcessContents(fileContents, string.Empty, bag);
            var expected = "<html><head><title>Some title</title></head><body></body></html>";

            // act
            var output = FileSystem.File.ReadAllText(@"C:\website\_site\index.html");

            // assert
            Assert.Equal(expected, output);
        }

        [Fact]
        public void Page_Layout_is_available_in_model()
        {
            // arrange
            const string fileContents = "<html><head><title>Some title</title></head><body>@Model.Page.Layout / @Model.Site.Posts[0].Layout / @Model.Bag[\"layout\"]</body></html>";

            var bag = new Dictionary<string, object> { };
            ProcessContents(fileContents, string.Empty, bag);
            var expected = "<html><head><title>Some title</title></head><body>Test / Test / Test</body></html>";

            // act
            var output = FileSystem.File.ReadAllText(@"C:\website\_site\index.html");

            // assert
            Assert.Equal(expected, output);
        }

        [Fact]
        public void Use_non_existing_filter_do_not_render_page()
        {
            const string templateContents = "<html><body>@Raw(Model.Content) @Filter.DoSomething(\"http://mysite.com/index.html\")</body></html>";
            const string pageContents = "<h1>Hello</h1>";

            Subject.Filters = new IFilter[] { new PrettifyUrlFilter() };
            ProcessContents(templateContents, pageContents, new Dictionary<string, object>());
            var output = FileSystem.File.ReadAllText(@"C:\website\_site\index.html");
            Assert.Equal(templateContents, output);
        }

        [Fact]
        public void Custom_tag_should_be_used()
        {
            // arrange
            const string templateContents = "<html><body>@Raw(Model.Content) @Tag.Custom()</body></html>";
            const string pageContents = "<h1>Hello</h1>";
            const string expected = "<html><body><h1>Hello</h1> custom tag</body></html>";
            Subject.Tags = new ITag[] { new CustomTag() };

            // act
            ProcessContents(templateContents, pageContents, new Dictionary<string, object>());

            // assert
            Assert.Equal(expected, FileSystem.File.ReadAllText(@"C:\website\_site\index.html"));
        }

        [Fact]
        public void PostUrlTag_should_be_used()
        {
            // arrange
            const string templateContents = "<html><body>@Raw(Model.Content) @Tag.PostUrl(\"post-title.md\")</body></html>";
            const string pageContents = "<h1>Hello</h1>";
            const string expected = "<html><body><h1>Hello</h1> post/title.html</body></html>";
            Subject.Tags = new ITag[] { new PostUrlTag() };

            // act
            ProcessContents(templateContents, pageContents, new Dictionary<string, object>());

            // assert
            Assert.Equal(expected, FileSystem.File.ReadAllText(@"C:\website\_site\index.html"));
        }

        public class CustomTag : DotLiquid.Tag, ITag
        {
            public new string Name { get { return "Custom"; } }

            public static string Custom()
            {
                return "custom tag";
            }

            public override void Render(DotLiquid.Context context, TextWriter result)
            {
                result.WriteLine(Custom());
            }
        }
    }

    public class When_Paginate_Razor : BakingEnvironment<RazorSiteEngine>
    {
        private const string TemplateContents = "@model Pretzel.Logic.Templating.Context.PageContext \r\n<html><body>@Raw(Model.Content)</body></html>";
        private const string PostContents = "---\r\n layout: default \r\n title: 'Post'\r\n---\r\n<h1>Post{0}</h1>";
        private const string IndexContents = "---\r\n layout: default \r\n paginate: 2 \r\n paginate_link: /blog/page:page/index.html \r\n---\r\n @model Pretzel.Logic.Templating.Context.PageContext \r\n @foreach(var post in Model.Paginator.Posts) { @Raw(post.Content) }";
        private const string ExpectedFileContents = "<html><body><p><h1>Post{0}</h1><h1>Post{1}</h1></p></body></html>";
        private const string ExpectedLastFileContents = "<html><body><p><h1>Post{0}</h1></p></body></html>";

        public override RazorSiteEngine Given()
        {
            return new RazorSiteEngine();
        }

        public override void When()
        {
            FileSystem.AddFile(@"C:\website\_layouts\default.cshtml", new MockFileData(TemplateContents));
            FileSystem.AddFile(@"C:\website\index.md", new MockFileData(IndexContents));

            for (var i = 1; i <= 7; i++)
            {
                FileSystem.AddFile(String.Format(@"C:\website\_posts\2012-02-0{0}-p{0}.md", i), new MockFileData(String.Format(PostContents, i)));
            }

            var generator = new SiteContextGenerator(FileSystem, Enumerable.Empty<IContentTransform>(), new LinkHelper());
            var context = generator.BuildContext(@"C:\website\", @"C:\website\_site", false);
            Subject.FileSystem = FileSystem;
            Subject.Process(context);
        }

        [Fact]
        public void Posts_Properly_Paginated()
        {
            Assert.Equal(String.Format(ExpectedFileContents, 7, 6),
                         FileSystem.File.ReadAllText(@"C:\website\_site\index.html").RemoveWhiteSpace());

            Assert.Equal(String.Format(ExpectedFileContents, 5, 4),
                         FileSystem.File.ReadAllText(@"C:\website\_site\blog\page2\index.html").RemoveWhiteSpace());

            Assert.Equal(String.Format(ExpectedFileContents, 3, 2),
                         FileSystem.File.ReadAllText(@"C:\website\_site\blog\page3\index.html").RemoveWhiteSpace());

            Assert.Equal(String.Format(ExpectedLastFileContents, 1),
                         FileSystem.File.ReadAllText(@"C:\website\_site\blog\page4\index.html").RemoveWhiteSpace());
        }
    }
}
