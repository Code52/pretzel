using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Pretzel.Logic.Exceptions;
using Pretzel.Logic.Templating.Context;
using Pretzel.Logic.Templating.Razor;
using Pretzel.Tests.Templating.Jekyll;
using Xunit;

namespace Pretzel.Tests.Templating.Razor
{
    public class RazorTests : BakingEnvironment<RazorSiteEngine>
    {
        public override RazorSiteEngine Given()
        {
            return new RazorSiteEngine();
        }

        public override void When()
        {
        }

        private void ProcessContents(string layout, string content, Dictionary<string, object> bag)
        {
            FileSystem.AddFile(@"C:\website\_layouts\Test.cshtml", new MockFileData(layout));
            var context = new SiteContext { SourceFolder = @"C:\website\", Title = "My Web Site" };
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

            var bag = new Dictionary<string, object> {{"title", title}};
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
        public void File_with_include_but_missing_is_processed()
        {
            const string templateContents = "<html><head><title>@Model.Title</title></head><body>@Raw(Model.Content)</body></html>";
            const string pageContents = "<i>@Include(\"TestInclude\")</i>";

            Assert.ThrowsDelegate action = () => ProcessContents(templateContents, pageContents, new Dictionary<string, object> {{"title", "My Web Site"}});

            Assert.Throws<PageProcessingException>(action);
        }
    }

    public class When_Paginate_Razor : BakingEnvironment<RazorSiteEngine>
    {
        const string TemplateContents = "@model Pretzel.Logic.Templating.Context.PageContext \r\n<html><body>@Raw(Model.Content)</body></html>";
        const string PostContents = "---\r\n layout: default \r\n title: 'Post'\r\n---\r\n<h1>Post{0}</h1>";
        const string IndexContents = "---\r\n layout: default \r\n paginate: 2 \r\n paginate_link: /blog/page:page/index.html \r\n---\r\n @model Pretzel.Logic.Templating.Context.PageContext \r\n @foreach(var post in Model.Paginator.Posts) { @Raw(post.Content) }";
        const string ExpectedfileContents = "<html><body><p><h1>Post{0}</h1><h1>Post{1}</h1></p></body></html>";
        const string ExpectedLastFileContents = "<html><body><p><h1>Post{0}</h1></p></body></html>";

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

            var generator = new SiteContextGenerator(FileSystem);
            var context = generator.BuildContext(@"C:\website\");
            Subject.FileSystem = FileSystem;
            Subject.Process(context);
        }

        [Fact]
        public void Posts_Properly_Paginated()
        {
            Assert.Equal(String.Format(ExpectedfileContents, 7, 6),
                         FileSystem.File.ReadAllText(@"C:\website\_site\index.html").RemoveWhiteSpace());

            Assert.Equal(String.Format(ExpectedfileContents, 5, 4),
                         FileSystem.File.ReadAllText(@"C:\website\_site\blog\page2\index.html").RemoveWhiteSpace());

            Assert.Equal(String.Format(ExpectedfileContents, 3, 2),
                         FileSystem.File.ReadAllText(@"C:\website\_site\blog\page3\index.html").RemoveWhiteSpace());

            Assert.Equal(String.Format(ExpectedLastFileContents, 1),
                         FileSystem.File.ReadAllText(@"C:\website\_site\blog\page4\index.html").RemoveWhiteSpace());
        }
    }
}
