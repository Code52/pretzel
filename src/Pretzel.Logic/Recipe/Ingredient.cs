using System;
using System.IO.Abstractions;
using Pretzel.Logic.Extensibility.Extensions;
using Pretzel.Logic.Extensions;

namespace Pretzel.Logic.Recipe
{
    public class Ingredient
    {
        private readonly string directory;

        private readonly IFileSystem fileSystem;

        private readonly string title;

        private readonly bool withDrafts;

        public Ingredient(IFileSystem fileSystem, string title, string directory, bool withDrafts)
        {
            this.fileSystem = fileSystem;
            this.title = title;
            this.directory = directory;
            this.withDrafts = withDrafts;
        }

        public void Create()
        {
            var postPath = fileSystem.Path.Combine(directory, !this.withDrafts ? @"_posts" : @"_drafts");

            var postName = string.Format("{0}-{1}.md", DateTime.Today.ToString("yyyy-MM-dd"), SlugifyFilter.Slugify(title));
            var pageContents = string.Format("---\r\n layout: post \r\n title: {0}\r\n comments: true\r\n---\r\n", title);

            if (!fileSystem.Directory.Exists(postPath))
            {
                Tracing.Info(string.Format("{0} folder not found", postPath));
                return;
            }

            if (fileSystem.File.Exists(fileSystem.Path.Combine(postPath, postName)))
            {
                Tracing.Info(string.Format("The \"{0}\" file already exists", postName));
                return;
            }

            fileSystem.File.WriteAllText(fileSystem.Path.Combine(postPath, postName), pageContents);

            Tracing.Info(string.Format("Created the \"{0}\" post ({1})", title, postName));
        }
    }
}