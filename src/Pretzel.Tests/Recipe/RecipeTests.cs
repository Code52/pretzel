using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace Pretzel.Tests.Recipe
{
    public class RecipeTests
    {
        public class Can_Setup_Site_Using_Liquid_Template
        {
            private MockFileSystem _fileSystem;
            const string _baseSite = @"c:\site\";

            [Fact]
            public void Files_and_Folders_Are_Created()
            {
                _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
                var recipe = new Logic.Recipe(_fileSystem, "Liquid", _baseSite);

                var result = recipe.Create();

                Assert.True(_fileSystem.Directory.Exists(_baseSite + @"_posts\"));
                Assert.True(_fileSystem.Directory.Exists(_baseSite + @"_layouts\"));
                Assert.True(_fileSystem.Directory.Exists(_baseSite + @"css\"));
                Assert.True(_fileSystem.Directory.Exists(_baseSite + @"img\"));

                Assert.True(_fileSystem.File.Exists(_baseSite + "rss.xml"));
                Assert.True(_fileSystem.File.Exists(_baseSite + "atom.xml"));
                Assert.True(_fileSystem.File.Exists(_baseSite + @"_layouts\layout.html"));
                Assert.True(_fileSystem.File.Exists(_baseSite + @"_layouts\post.html"));
                Assert.True(_fileSystem.File.Exists(_baseSite + "index.md"));
                Assert.True(_fileSystem.File.Exists(_baseSite + "about.md"));
                Assert.True(_fileSystem.File.Exists(_baseSite + string.Format(@"_posts\{0}-myfirstpost.md", DateTime.Today.ToString("yyyy-MM-dd"))));
                Assert.True(_fileSystem.File.Exists(_baseSite + @"css\style.css"));
                Assert.True(_fileSystem.File.Exists(_baseSite + @"img\25.png"));
                Assert.True(_fileSystem.File.Exists(_baseSite + @"img\favicon.png"));
                Assert.True(_fileSystem.File.Exists(_baseSite + @"img\logo.png"));
                Assert.True(_fileSystem.File.Exists(_baseSite + @"img\favicon.ico"));

                Assert.Equal(result, "Pretzel site template has been created");
            }

            [Fact]
            public void Razor_Engine_returns_error()
            {
                _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
                var recipe = new Logic.Recipe(_fileSystem, "Razor", _baseSite);

                var result = recipe.Create();

                Assert.Equal(result, "Razor templating hasn't been implemented yet");
            }

            [Fact]
            public void Other_Engine_returns_error()
            {
                _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
                var recipe = new Logic.Recipe(_fileSystem, "Musak", _baseSite);

                var result = recipe.Create();

                Assert.Equal(result, "Templating Engine not found");
            }
        }
    }
}
