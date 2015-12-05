using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating.Context;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using Pretzel.Logic.Extensibility.Extensions;

namespace Pretzel.Commands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [CommandInfo(CommandName = "newpost")]
    public sealed class NewpostCommand : ICommand
    {
#pragma warning disable 649

        [Import]
        private IFileSystem fileSystem;

#pragma warning restore 649

        public void Execute(IEnumerable<string> arguments)
        {
            var postPath = fileSystem.Path.Combine(fileSystem.Directory.GetCurrentDirectory(), @"_posts");

            Tracing.Info("newpost - create a new post");

            if (arguments.Count() == 0)
            {
                Tracing.Info("A title must be provided.");
                return;
            }

            var title = arguments.First();
            var postName = string.Format("{0}-{1}.md", DateTime.Today.ToString("yyyy-MM-dd"), SlugifyFilter.Slugify(title));
            var pageContents = string.Format("---\r\n layout: post \r\n title: {0}\r\n comments: true\r\n---\r\n", title);


            if (!fileSystem.Directory.Exists(postPath))
            {
                Tracing.Info("_posts folder not created.");
                return;
            }

            if (fileSystem.File.Exists(fileSystem.Path.Combine(postPath, postName)))
            {
                Tracing.Info(String.Format("The \"{0}\" file already exists", postName));
                return;
            }

            fileSystem.File.WriteAllText(fileSystem.Path.Combine(postPath, postName), pageContents);

            Tracing.Info(String.Format("Created the \"{0}\" post ({1})", title, postName));
        }

        public void WriteHelp(TextWriter writer)
        {
            writer.Write("   Create a new post\r\n");
        }
    }
}
