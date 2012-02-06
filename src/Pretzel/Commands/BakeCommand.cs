using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using MarkdownDeep;
using NDesk.Options;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Commands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [CommandInfo(CommandName = "bake")]
    public sealed class BakeCommand : ICommand
    {
        private static readonly Markdown Markdown = new Markdown();

        [Import]
        private TemplateEngineCollection templateEngines;

        [Import]
        public IFileSystem FileSystem { get; set; }

        public string Path { get; private set; }
        public string Engine { get; set; }

        private OptionSet Settings
        {
            get
            {
                return new OptionSet
                           {
                               { "e|engine=", "The render engine", v => Engine = v },
                               { "p|path=", "The path to site directory", p => Path = p },
                           };
            }
        }

        public void Execute(string[] arguments)
        {
            Tracing.Info("bake - transforming content into a website");

            Settings.Parse(arguments);

            if (string.IsNullOrWhiteSpace(Path))
            {
                Path = Directory.GetCurrentDirectory();
            }

            if (string.IsNullOrWhiteSpace(Engine))
            {
                Engine = InferEngineFromDirectory(Path);
            }

            var engine = templateEngines[Engine];
            if (engine != null)
            {
                var watch = new Stopwatch();
                watch.Start();
                engine.Initialize();
                var c = BuildContext(Path);
                engine.Process(c);
                watch.Stop();
                Tracing.Info(string.Format("done - took {0}ms", watch.ElapsedMilliseconds));
            }
            else
            {
                Tracing.Info(String.Format("Cannot find engine for input: '{0}'", Engine));
            }
        }

        private SiteContext BuildContext(string path)
        {
            var context = new SiteContext
                              {
                                  SourceFolder = path,
                                  OutputFolder = System.IO.Path.Combine(path, "_site"),
                                  Posts = new List<Page>()
                              };

            var postsFolder = System.IO.Path.Combine(context.SourceFolder, "_posts");
            if (FileSystem.Directory.Exists(postsFolder))
            {
                foreach (var file in FileSystem.Directory.GetFiles(postsFolder, "*.*", SearchOption.AllDirectories))
                {
                    var contents = FileSystem.File.ReadAllText(file);
                    var header = contents.YamlHeader();
                    var post = new Page
                    {
                        Title = header.ContainsKey("title") ? header["title"].ToString() : "this is a post", // should this be the Site title?
                        Date = header.ContainsKey("date") ? DateTime.Parse(header["date"].ToString()) : file.Datestamp(),
                        Content = Markdown.Transform(contents.ExcludeHeader()),
                        Filepath =  GetPathWithTimestamp(context.OutputFolder, file),
                        File = file,
                        Bag = header,
                    };
                    context.Posts.Add(post);
                }

                context.Posts = context.Posts.OrderByDescending(p => p.Date).ToList();
            }

            return context;
        }

        private string GetPathWithTimestamp(string outputDirectory, string file)
        {
            // TODO: detect mode from site config
            var fileName = file.Substring(file.LastIndexOf("\\"));

            var tokens = fileName.Split('-');
            var timestamp = string.Join("\\", tokens.Take(3)).Trim('\\');
            var title = string.Join("-", tokens.Skip(3));
            return System.IO.Path.Combine(outputDirectory, timestamp, title);
        }

        private string InferEngineFromDirectory(string path)
        {
            foreach (var engine in templateEngines.Engines)
            {
                if (!engine.Value.CanProcess(path)) continue;
                Tracing.Info(String.Format("Recommended engine for directory: '{0}'", engine.Key));
                return engine.Key;
            }

            return string.Empty;
        }

        public void WriteHelp(TextWriter writer)
        {
            Settings.WriteOptionDescriptions(writer);
        }
    }
}
