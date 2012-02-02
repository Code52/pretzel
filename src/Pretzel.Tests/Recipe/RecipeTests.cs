using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace Pretzel.Tests.Recipe
{
    public class RecipeTests
    {
        const string BaseSite = @"c:\site\";
        MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
        readonly TestTraceListener listener = new TestTraceListener();
        
        public RecipeTests()
        {
            Trace.Listeners.Add(listener);
        }

        [Fact]
        public void Files_and_Folders_Are_Created()
        {
            var recipe = new Logic.Recipe(fileSystem, "Jekyll", BaseSite);
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

            Assert.True(listener.Received("Pretzel site template has been created"));
        }

        [Fact]
        public void Razor_Engine_returns_error()
        {
            var recipe = new Logic.Recipe(fileSystem, "Razor", BaseSite);

            recipe.Create();

            Assert.True(listener.Received("Razor templating hasn't been implemented yet"));
        }

        [Fact]
        public void Other_Engine_returns_error()
        {
            fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
            var recipe = new Logic.Recipe(fileSystem, "Musak", BaseSite);

            recipe.Create();

            Assert.True(listener.Received("Templating Engine not found"));
        }
    }
}
