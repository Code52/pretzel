using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using MarkdownDeep;
using Pretzel.Logic.Extensions;
using ImportAttribute = System.ComponentModel.Composition.ImportAttribute;

namespace Pretzel.Logic.Templating.Context
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SiteContextGenerator
    {
        private static readonly Markdown Markdown = new Markdown();

#pragma warning disable 0649
		[Import]
        IFileSystem fileSystem;
#pragma warning restore 0649

		public SiteContext BuildContext(string path)
        {
            var config = new Dictionary<string, object>();
            if (File.Exists(Path.Combine(path, "_config.yml")))
                config = (Dictionary<string, object>)File.ReadAllText(Path.Combine(path, "_config.yml")).YamlHeader(true);

            if (!config.ContainsKey("permalink"))
                config.Add("permalink", "/:year/:month/:day/:title.html");

            var context = new SiteContext
            {
                SourceFolder = path,
                OutputFolder = Path.Combine(path, "_site"),
                Posts = new List<Page>(),
                Pages = new List<Page>(),
            };

            var postsFolder = Path.Combine(context.SourceFolder, "_posts");
            if (fileSystem.Directory.Exists(postsFolder))
            {
                foreach (var file in fileSystem.Directory.GetFiles(postsFolder, "*.*", SearchOption.AllDirectories))
                {
                    var contents = fileSystem.File.ReadAllText(file);
                    var header = contents.YamlHeader();
                    var post = new Page
                    {
                        Title = header.ContainsKey("title") ? header["title"].ToString() : "this is a post", // should this be the Site title?
                        Date = header.ContainsKey("date") ? DateTime.Parse(header["date"].ToString()) : file.Datestamp(),
                        Content = Markdown.Transform(contents.ExcludeHeader()),
                        Filepath = GetPathWithTimestamp(context.OutputFolder, file),
                        File = file,
                        Bag = header,
                    };

                    if (header.ContainsKey("permalink"))
                        post.Url = EvaluatePermalink(header["permalink"].ToString(), post);
                    else if (config.ContainsKey("permalink"))
                        post.Url = EvaluatePermalink(config["permalink"].ToString(), post);

                    if (string.IsNullOrEmpty(post.Url))
                    {
                        Tracing.Info("whaaa");
                    }
                    context.Posts.Add(post);
                }

                context.Posts = context.Posts.OrderByDescending(p => p.Date).ToList();
            }

            foreach (var file in fileSystem.Directory.GetFiles(context.SourceFolder, "*.*", SearchOption.AllDirectories))
            {
                var relativePath = MapToOutputPath(context, file);
                if (relativePath.StartsWith("_"))
                    continue;

                if (relativePath.StartsWith("."))
                    continue;

                using (var reader = new StreamReader(file))
                {
                    var x = reader.ReadLine();
                    if (x == null || !x.StartsWith("---"))
                    {
                        context.Pages.Add(new NonProcessedPage
                                              {
                                                  File = file, 
                                                  Filepath = Path.Combine(context.OutputFolder, file)
                                              });
                        continue;
                    }
                }

                var contents = fileSystem.File.ReadAllText(file);
                var header = contents.YamlHeader();
                var page = new Page
                {
                    Title = header.ContainsKey("title") ? header["title"].ToString() : "this is a post", // should this be the Site title?
                    Date = header.ContainsKey("date") ? DateTime.Parse(header["date"].ToString()) : DateTime.Now,
                    Content = Markdown.Transform(contents.ExcludeHeader()),
                    Filepath = GetPathWithTimestamp(context.OutputFolder, file),
                    File = file,
                    Bag = header,
                };

                context.Pages.Add(page);
            }

            return context;
        }
        //https://github.com/mojombo/jekyll/wiki/permalinks
        private string EvaluatePermalink(string permalink, Page page)
        {
            permalink = permalink.Replace(":year", page.Date.Year.ToString(CultureInfo.InvariantCulture));
            permalink = permalink.Replace(":month", page.Date.ToString("MM"));
            permalink = permalink.Replace(":day", page.Date.ToString("dd"));
            permalink = permalink.Replace(":title", GetTitle(page.File));

            return permalink;
        }

        private string SanitizeTitle(string title)
        {
            title = title.Replace(" ", "_");
            title = title.Replace(":", "");
            title = RemoveDiacritics(title);

            return title;
        }

        //http://stackoverflow.com/questions/6716832/sanitizing-string-to-url-safe-format
        public static string RemoveDiacritics(string strThis)
        {
            if (strThis == null)
                return null;

            strThis = strThis.ToLowerInvariant();

            var sb = new StringBuilder();

            foreach (char c in strThis.Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString();
        }

        private string MapToOutputPath(SiteContext context, string file)
        {
            return file.Replace(context.SourceFolder, "").TrimStart('\\');
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
        private string GetTitle(string file)
        {
            // TODO: detect mode from site config
            var fileName = file.Substring(file.LastIndexOf("\\"));

            var tokens = fileName.Split('-');
            var title = string.Join("-", tokens.Skip(3));
            title = title.Substring(0, title.LastIndexOf("."));
            return title;
        }
    }
}
