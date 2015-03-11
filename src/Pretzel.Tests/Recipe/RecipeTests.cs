using NSubstitute;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensions;
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

namespace Pretzel.Tests.Recipe
{
    public class RecipeTests
    {
        private const string BaseSite = @"c:\site\";
        private MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
        private readonly StringBuilder sb = new StringBuilder();
        private readonly TextWriter writer;

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
            var recipe = new Logic.Recipe.Recipe(fileSystem, "liquid", BaseSite, Enumerable.Empty<IAdditionalIngredient>(), false, false);
            recipe.Create();

            Assert.True(fileSystem.Directory.Exists(BaseSite + @"_posts\"));
            Assert.True(fileSystem.Directory.Exists(BaseSite + @"_layouts\"));
            Assert.True(fileSystem.Directory.Exists(BaseSite + @"css\"));
            Assert.True(fileSystem.Directory.Exists(BaseSite + @"img\"));
            Assert.True(fileSystem.Directory.Exists(BaseSite + @"_includes\"));

            Assert.True(fileSystem.File.Exists(BaseSite + "sitemap.xml"));
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
            Assert.True(fileSystem.File.Exists(BaseSite + @"_includes\head.html"));

            Assert.True(writer.ToString().Contains("Pretzel site template has been created"));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Files_and_Folders_Are_Created_for_Razor(bool wiki)
        {
            var recipe = new Logic.Recipe.Recipe(fileSystem, "razor", BaseSite, Enumerable.Empty<IAdditionalIngredient>(), false, wiki);
            recipe.Create();

            Assert.True(fileSystem.Directory.Exists(BaseSite + @"_posts\"));
            Assert.True(fileSystem.Directory.Exists(BaseSite + @"_layouts\"));
            Assert.True(fileSystem.Directory.Exists(BaseSite + @"css\"));
            Assert.True(fileSystem.Directory.Exists(BaseSite + @"img\"));
            Assert.True(fileSystem.Directory.Exists(BaseSite + @"_includes\"));
            Assert.True(fileSystem.File.Exists(BaseSite + @"_layouts\layout.cshtml"));
            Assert.True(fileSystem.File.Exists(BaseSite + "index.md"));
            Assert.True(fileSystem.File.Exists(BaseSite + @"css\style.css"));
            Assert.True(fileSystem.File.Exists(BaseSite + @"img\favicon.ico"));
            
            if(!wiki)
            { 
                Assert.True(fileSystem.File.Exists(BaseSite + @"_includes\head.cshtml"));
            }

            Assert.Equal(!wiki, fileSystem.File.Exists(BaseSite + @"_layouts\post.cshtml"));
            Assert.Equal(!wiki, fileSystem.File.Exists(BaseSite + "about.md"));
            Assert.Equal(!wiki, fileSystem.File.Exists(BaseSite + string.Format(@"_posts\{0}-myfirstpost.md", DateTime.Today.ToString("yyyy-MM-dd"))));
            Assert.Equal(!wiki, fileSystem.File.Exists(BaseSite + @"img\25.png"));
            Assert.Equal(!wiki, fileSystem.File.Exists(BaseSite + @"img\favicon.png"));
            Assert.Equal(!wiki, fileSystem.File.Exists(BaseSite + @"img\logo.png"));
            Assert.Equal(!wiki, fileSystem.File.Exists(BaseSite + "rss.xml"));
            Assert.Equal(!wiki, fileSystem.File.Exists(BaseSite + "atom.xml"));
            Assert.Equal(!wiki, fileSystem.File.Exists(BaseSite + "sitemap.xml"));

            Assert.True(writer.ToString().Contains("Pretzel site template has been created"));
        }

        [Fact]
        public void Other_Engine_returns_error()
        {
            fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
            var recipe = new Logic.Recipe.Recipe(fileSystem, "Musak", BaseSite, Enumerable.Empty<IAdditionalIngredient>(), false, false);

            recipe.Create();

            Assert.True(writer.ToString().Contains("Templating Engine not found"));
        }

        [Fact]
        public void can_mixin_additional_ingredients_for_razor()
        {
            fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
            var additionalIngredient = Substitute.For<IAdditionalIngredient>();
            var recipe = new Logic.Recipe.Recipe(fileSystem, "Razor", BaseSite, new[] { additionalIngredient }, false, false);
            recipe.Create();

            additionalIngredient.Received().MixIn(BaseSite);
        }

        [Fact]
        public void can_mixin_additional_ingredients_for_liquid()
        {
            fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
            var additionalIngredient = Substitute.For<IAdditionalIngredient>();
            var recipe = new Logic.Recipe.Recipe(fileSystem, "Liquid", BaseSite, new[] { additionalIngredient }, false, false);
            recipe.Create();

            additionalIngredient.Received().MixIn(BaseSite);
        }

        [Fact]
        public void additional_ingredients_not_mixed_in_for_other_engine()
        {
            fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
            var additionalIngredient = Substitute.For<IAdditionalIngredient>();
            var recipe = new Logic.Recipe.Recipe(fileSystem, "Musak", BaseSite, new[] { additionalIngredient }, false, false);
            recipe.Create();

            additionalIngredient.DidNotReceive().MixIn(BaseSite);
        }

        [Fact]
        public void liquid_engine_with_wiki()
        {
            fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());

            var recipe = new Logic.Recipe.Recipe(fileSystem, "liquid", BaseSite, Enumerable.Empty<IAdditionalIngredient>(), false, true);
            recipe.Create();

            Assert.True(writer.ToString().Contains("Wiki switch not valid with liquid templating engine"));
        }

        [Fact]
        public void razor_engine_with_project()
        {
            fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());

            var recipe = new Logic.Recipe.Recipe(fileSystem, "razor", BaseSite, Enumerable.Empty<IAdditionalIngredient>(), true, false);
            recipe.Create();

            Assert.Equal(40, fileSystem.AllPaths.Count());
            Assert.True(fileSystem.AllFiles.Contains(@"c:\site\_layouts\Properties\AssemblyInfo.cs"));
            Assert.True(fileSystem.AllFiles.Contains(@"c:\site\_layouts\PretzelClasses\Category.cs"));
            Assert.True(fileSystem.AllFiles.Contains(@"c:\site\_layouts\LayoutProject.csproj"));
            Assert.True(fileSystem.AllFiles.Contains(@"c:\site\_layouts\layoutSolution.sln"));
            Assert.True(fileSystem.AllFiles.Contains(@"c:\site\_layouts\PretzelClasses\NonProcessedPage.cs"));
            Assert.True(fileSystem.AllFiles.Contains(@"c:\site\_layouts\.nuget\NuGet.config"));
            Assert.True(fileSystem.AllFiles.Contains(@"c:\site\_layouts\.nuget\NuGet.exe"));
            Assert.True(fileSystem.AllFiles.Contains(@"c:\site\_layouts\.nuget\NuGet.targets"));
            Assert.True(fileSystem.AllFiles.Contains(@"c:\site\_layouts\PretzelClasses\PageContext.cs"));
            Assert.True(fileSystem.AllFiles.Contains(@"c:\site\_layouts\PretzelClasses\Page.cs"));
            Assert.True(fileSystem.AllFiles.Contains(@"c:\site\_layouts\PretzelClasses\Paginator.cs"));
            Assert.True(fileSystem.AllFiles.Contains(@"c:\site\_layouts\PretzelClasses\SiteContext.cs"));
            Assert.True(fileSystem.AllFiles.Contains(@"c:\site\_layouts\PretzelClasses\Tag.cs"));
            Assert.True(fileSystem.AllFiles.Contains(@"c:\site\_layouts\web.config"));
            Assert.True(fileSystem.AllFiles.Contains(@"c:\site\_layouts\packages.config"));
        }

        [Fact]
        public void error_is_traced()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            var fileSubstitute = Substitute.For<FileBase>();
            fileSubstitute.When(f => f.WriteAllText(Arg.Any<string>(), Arg.Any<string>())).Do(x => { throw new Exception("Error!!!"); });

            var fileSystemSubstitute = Substitute.For<IFileSystem>();
            fileSystemSubstitute.File.Returns(fileSubstitute);

            var recipe = new Logic.Recipe.Recipe(fileSystemSubstitute, "liquid", BaseSite, Enumerable.Empty<IAdditionalIngredient>(), false, false);
            recipe.Create();

            Assert.Contains(@"Error trying to create template: System.Exception: Error!!!", writer.ToString());
            Assert.Contains(@"at Pretzel.Tests.Recipe.RecipeTests.<error_is_traced>b__0(CallInfo x)", writer.ToString());
        }

        [Fact]
        public void Drafts_Folders_Is_Created()
        {
            var recipe = new Logic.Recipe.Recipe(fileSystem, "liquid", BaseSite, Enumerable.Empty<IAdditionalIngredient>(), false, false, true);
            recipe.Create();

            Assert.True(fileSystem.Directory.Exists(BaseSite + @"_drafts\"));
        }
    }
}
