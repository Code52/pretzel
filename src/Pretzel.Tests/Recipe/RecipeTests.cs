using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using Pretzel.Logic.Extensions;
using Xunit;

namespace Pretzel.Tests.Recipe
{
    public class RecipeTests
    {
        const string BaseSite = @"c:\site\";
        MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
		readonly StringBuilder sb = new StringBuilder();
		readonly TextWriter writer;

        public RecipeTests()
        {
            writer = new StringWriter(sb);
            Tracing.Logger.SetWriter(writer);
            Tracing.Logger.AddCategory("info");
            Tracing.Logger.AddCategory("error");
        }

        [Fact]
        public void Files_and_Folders_Are_Created_for_Jekyll()
        {
            var recipe = new Logic.Recipe(fileSystem, "liquid", BaseSite);
            recipe.Create();

            Assert.True(fileSystem.Directory.Exists(BaseSite + @"_posts\"));
            Assert.True(fileSystem.Directory.Exists(BaseSite + @"_layouts\"));
            Assert.True(fileSystem.Directory.Exists(BaseSite + @"css\"));
            Assert.True(fileSystem.Directory.Exists(BaseSite + @"img\"));

            Assert.True(fileSystem.File.Exists(BaseSite + "rss.xml"));
            Assert.True(fileSystem.File.Exists(BaseSite + "atom.xml"));
            Assert.True(fileSystem.File.Exists(BaseSite + @"_layouts\layout.html"));
            Assert.True(fileSystem.File.Exists(BaseSite + @"_layouts\post.html"));
            Assert.True(fileSystem.File.Exists(BaseSite + "index.md"));
            Assert.True(fileSystem.File.Exists(BaseSite + "about.md"));
            Assert.True(fileSystem.File.Exists(BaseSite + string.Format(@"_posts\{0}-myfirstpost.md", DateTime.Today.ToString("yyyy-MM-dd"))));
            Assert.True(fileSystem.File.Exists(BaseSite + @"css\style.css"));
            Assert.True(fileSystem.File.Exists(BaseSite + @"img\25.png"));
            Assert.True(fileSystem.File.Exists(BaseSite + @"img\favicon.png"));
            Assert.True(fileSystem.File.Exists(BaseSite + @"img\logo.png"));
            Assert.True(fileSystem.File.Exists(BaseSite + @"img\favicon.ico"));

            Assert.True(writer.ToString().Contains("Pretzel site template has been created"));
        }

        [Fact]
        public void Files_and_Folders_Are_Created_for_Razor()
        {
            var recipe = new Logic.Recipe(fileSystem, "razor", BaseSite);
            recipe.Create();

            Assert.True(fileSystem.Directory.Exists(BaseSite + @"_posts\"));
            Assert.True(fileSystem.Directory.Exists(BaseSite + @"_layouts\"));
            Assert.True(fileSystem.Directory.Exists(BaseSite + @"css\"));
            Assert.True(fileSystem.Directory.Exists(BaseSite + @"img\"));

            Assert.True(fileSystem.File.Exists(BaseSite + "rss.xml"));
            Assert.True(fileSystem.File.Exists(BaseSite + "atom.xml"));
            Assert.True(fileSystem.File.Exists(BaseSite + @"_layouts\layout.cshtml"));
            Assert.True(fileSystem.File.Exists(BaseSite + @"_layouts\post.cshtml"));
            Assert.True(fileSystem.File.Exists(BaseSite + "index.cshtml"));
            Assert.True(fileSystem.File.Exists(BaseSite + "about.cshtml"));
            Assert.True(fileSystem.File.Exists(BaseSite + string.Format(@"_posts\{0}-myfirstpost.cshtml", DateTime.Today.ToString("yyyy-MM-dd"))));
            Assert.True(fileSystem.File.Exists(BaseSite + @"css\style.css"));
            Assert.True(fileSystem.File.Exists(BaseSite + @"img\25.png"));
            Assert.True(fileSystem.File.Exists(BaseSite + @"img\favicon.png"));
            Assert.True(fileSystem.File.Exists(BaseSite + @"img\logo.png"));
            Assert.True(fileSystem.File.Exists(BaseSite + @"img\favicon.ico"));

            Assert.True(writer.ToString().Contains("Pretzel site template has been created"));
        }

        [Fact]
        public void Other_Engine_returns_error()
        {
            fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
            var recipe = new Logic.Recipe(fileSystem, "Musak", BaseSite);

            recipe.Create();

            Assert.True(writer.ToString().Contains("Templating Engine not found"));
        }
    }
}
