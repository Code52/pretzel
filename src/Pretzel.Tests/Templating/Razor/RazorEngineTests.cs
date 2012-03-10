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

        private void ProcessContents(string template, string content, Dictionary<string, object> bag)
        {
            var context = new SiteContext { SourceFolder = @"C:\website\", Title = "My Web Site" };
            context.Posts.Add(new Page { File = "index.cshtml", Content = content, OutputFile = @"C:\website\_site\index.html", Bag = bag });
            FileSystem.AddFile(@"C:\website\index.cshtml", new MockFileData(template));
            var subject = Subject;
            subject.FileSystem = FileSystem;
            subject.Process(context);
        }

        [Fact]
        public void File_with_no_replacements_is_unaltered()
        {
            string FileContents = "<html><head><title></title></head><body></body></html>";
            ProcessContents(string.Empty, FileContents, new Dictionary<string,object>());
            string expected = FileContents;
            Assert.Equal(expected, FileSystem.File.ReadAllText(@"C:\website\_site\index.html"));
        }

        [Fact]
        public void File_with_title_is_processed()
        {
            string FileContents = "<html><head><title>@Model.Title</title></head><body></body></html>";
            string Title = "This is the title!";

            var bag = new Dictionary<string, object>();
            bag.Add("title", Title);
            ProcessContents(string.Empty, FileContents, bag);
            string expected = FileContents.Replace(@"@Model.Title", Title);
            string output = FileSystem.File.ReadAllText(@"C:\website\_site\index.html");
            Assert.Equal(expected, output);
        }

        [Fact]
        public void File_with_content_is_processed()
        {
            string TemplateContents = "<html><head><title></title></head><body>@Model.Content</body></html>";
            string PageContents = "<h1>Hello World!</h1>";
            string ExpectedfileContents = "<html><head><title>My Web Site</title></head><body><h1>Hello World!</h1></body></html>";

            var bag = new Dictionary<string, object>();
            bag.Add("Content", PageContents);
            ProcessContents(PageContents, TemplateContents, bag);
            string expected = TemplateContents.Replace(@"@Model.Content", PageContents);
            string output = FileSystem.File.ReadAllText(@"C:\website\_site\index.html");
            Assert.Equal(expected, output);
        }
    }

    public class When_Recieving_A_Razor_File : BakingEnvironment<RazorSiteEngine>
    {
        const string TemplateContents = "<html><head><title>@Model.Title</title></head><body>@Model.Content</body></html>";
        const string PageContents = "<h1>Hello World!</h1>";
        const string ExpectedfileContents = "<html><head><title>My Web Site</title></head><body><h1>Hello World!</h1></body></html>";

        public override RazorSiteEngine Given()
        {
            return new RazorSiteEngine();
        }

        public override void When()
        {
            FileSystem.AddFile(@"C:\website\_layouts\default.html", new MockFileData(TemplateContents));
            FileSystem.AddFile(@"C:\website\index.md", new MockFileData(PageContents));
            var context = new SiteContext { SourceFolder = @"C:\website\", Title = "My Web Site" };
            Subject.FileSystem = FileSystem;
            Subject.Process(context);
        }

        [Fact]
        public void The_File_Is_Applies_Data_To_The_Template()
        {
            Assert.Equal(ExpectedfileContents, FileSystem.File.ReadAllText(@"C:\website\_site\index.html").RemoveWhiteSpace());
        }

        [Fact]
        public void Does_Not_Copy_Template_To_Output()
        {
            Assert.False(FileSystem.File.Exists(@"C:\website\_site\_layouts\default.html"));
        }
    }
}
