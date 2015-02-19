using Pretzel.Logic.Templating.Context;
using Pretzel.Logic.Templating.Razor;
using Pretzel.Tests.Templating.Jekyll;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace Pretzel.Tests.Templating.Razor
{
    public class When_Recieving_A_Razor_File : BakingEnvironment<RazorSiteEngine>
    {
        private const string TemplateContents = "<html><head><title>@Model.Title</title></head><body>@Raw(Model.Content)</body></html>";
        private const string PageContents = "<h1>Hello World!</h1>";
        private const string ExpectedfileContents = "<html><head><title>My Web Site</title></head><body><h1>Hello World!</h1></body></html>";

        public override RazorSiteEngine Given()
        {
            return new RazorSiteEngine();
        }

        public override void When()
        {
            FileSystem.AddFile(@"C:\website\_layouts\default.cshtml", new MockFileData(TemplateContents));
            FileSystem.AddFile(@"C:\website\index.cshtml", new MockFileData(PageContents));
            var context = new SiteContext { SourceFolder = @"C:\website\", OutputFolder = @"C:\website\_site", Title = "My Web Site" };
            var dictionary = new Dictionary<string, object>
                                 {
                                     {"layout", "default"}
                                 };
            context.Posts.Add(new Page { File = "index.cshtml", Content = PageContents, OutputFile = @"C:\website\_site\index.html", Bag = dictionary });
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
