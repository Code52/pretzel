using NSubstitute;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensibility.Extensions;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Recipes;
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
    public class IngredientTests
    {
        private MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
        private const string BaseSite = @"c:\site\";
        private const string PostsFolder = @"_posts";
        private const string DraftsFolder = @"_drafts";

        private readonly StringBuilder trace = new StringBuilder();

        public IngredientTests()
        {
            Tracing.SetTrace((message, traceLevel) => { trace.AppendLine(message); });
            Tracing.SetMinimalLevel(TraceLevel.Debug);
        }

        [Fact]
        public void Post_Is_Created()
        {
            fileSystem.Directory.CreateDirectory(BaseSite + PostsFolder);
            var postTitle = "Post title";
            var postName = string.Format("{0}-{1}.md", DateTime.Today.ToString("yyyy-MM-dd"), SlugifyFilter.Slugify(postTitle));

            var ingredient = new Ingredient(fileSystem, postTitle, BaseSite, false);
            ingredient.Create();

            Assert.True(fileSystem.File.Exists(fileSystem.Path.Combine(BaseSite + PostsFolder, postName)));
        }

        [Fact]
        public void Post_Has_Correct_Content()
        {
            fileSystem.Directory.CreateDirectory(BaseSite + PostsFolder);
            var postTitle = "Post title";
            var expectedContent = string.Format("---\r\n layout: post \r\n title: {0}\r\n comments: true\r\n---\r\n", postTitle);
            var postName = string.Format("{0}-{1}.md", DateTime.Today.ToString("yyyy-MM-dd"), SlugifyFilter.Slugify(postTitle));

            var ingredient = new Ingredient(fileSystem, "Post title", BaseSite, false);
            ingredient.Create();

            Assert.Equal(expectedContent, fileSystem.File.ReadAllText(fileSystem.Path.Combine(BaseSite + PostsFolder, postName)));
        }

        [Fact]
        public void Post_Already_Exists()
        {
            fileSystem.Directory.CreateDirectory(BaseSite + PostsFolder);
            var postTitle = "Post title";
            var postName = string.Format("{0}-{1}.md", DateTime.Today.ToString("yyyy-MM-dd"), SlugifyFilter.Slugify(postTitle));

            var ingredient = new Ingredient(fileSystem, postTitle, BaseSite, false);
            ingredient.Create();
            ingredient.Create();

            Assert.Contains(string.Format("The \"{0}\" file already exists", postName), trace.ToString());
        }

        [Fact]
        public void Post_Folder_Not_Found()
        {
            var ingredient = new Ingredient(fileSystem, string.Empty, BaseSite, false);
            ingredient.Create();

            Assert.Contains(string.Format(@"{0} folder not found", BaseSite + PostsFolder), trace.ToString());
        }

        [Fact]
        public void Draft_Folder_Not_Found()
        {
            var ingredient = new Ingredient(fileSystem, string.Empty, BaseSite, true);
            ingredient.Create();

            Assert.Contains(string.Format(@"{0} folder not found", BaseSite + DraftsFolder), trace.ToString());
        }
    }
}
