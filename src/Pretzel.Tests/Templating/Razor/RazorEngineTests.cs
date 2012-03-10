using System.IO.Abstractions.TestingHelpers;
using Pretzel.Logic.Templating.Context;
using Pretzel.Logic.Templating.Razor;
using Pretzel.Tests.Templating.Jekyll;
using Xunit;
using System.Collections.Generic;

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
            string expected = fileContents.Replace(@"@Model.Title", title);
            string output = FileSystem.File.ReadAllText(@"C:\website\_site\index.html");
            Assert.Equal(expected, output);
        }

        [Fact]
        public void File_with_content_is_processed()
        {
            const string templateContents = "<html><head><title>@Model.Title</title></head><body>@Raw(Model.Content)</body></html>";
            const string pageContents = "<h1>Hello World!</h1>";
            const string expectedfileContents = "<html><head><title>My Web Site</title></head><body><h1>Hello World!</h1></body></html>";

            ProcessContents(templateContents, pageContents, new Dictionary<string, object> { { "title", "My Web Site" } });
            string output = FileSystem.File.ReadAllText(@"C:\website\_site\index.html");
            Assert.Equal(expectedfileContents, output);
        }
    }
}
