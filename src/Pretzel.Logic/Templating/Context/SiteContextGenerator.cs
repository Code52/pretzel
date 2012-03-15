﻿using System;
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
        [Import] IFileSystem fileSystem;
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
                Config = config,
                Time = DateTime.Now,
            };

            BuildPosts(config, context);

            BuildPages(config, context);

            return context;
        }

        private void BuildPages(Dictionary<string, object> config, SiteContext context)
        {
            foreach (var file in fileSystem.Directory.GetFiles(context.SourceFolder, "*.*", SearchOption.AllDirectories))
            {
                var relativePath = MapToOutputPath(context, file);
                if (relativePath.StartsWith("_"))
                    continue;

                if (relativePath.StartsWith("."))
                    continue;

                var postFirstLine = SafeReadLine(file);
                if (postFirstLine == null || !postFirstLine.StartsWith("---"))
                {
                    context.Pages.Add(new NonProcessedPage
                    {
                        File = file,
                        Filepath = Path.Combine(context.OutputFolder, file)
                    });
                    continue;
                }

                var contents = SafeReadContents(file);
                var header = contents.YamlHeader();
                var page = new Page
                {
                    Title = header.ContainsKey("title") ? header["title"].ToString() : "this is a post", // should this be the Site title?
                    Date = header.ContainsKey("date") ? DateTime.Parse(header["date"].ToString()) : file.Datestamp(),
                    Content = RenderContent(file, contents, header), 
                    Filepath = GetPathWithTimestamp(context.OutputFolder, file),
                    File = file,
                    Bag = header,
                };

                if (header.ContainsKey("permalink"))
                {
                    page.Url = EvaluatePagePermalink(header["permalink"].ToString(), page);
                }

                context.Pages.Add(page);
            }
        }

        private void BuildPosts(Dictionary<string, object> config, SiteContext context)
        {
            var postsFolder = Path.Combine(context.SourceFolder, "_posts");
            if (fileSystem.Directory.Exists(postsFolder))
            {
                foreach (var file in fileSystem.Directory.GetFiles(postsFolder, "*.*", SearchOption.AllDirectories))
                {
                    BuildPost(config, context, file);
                }

                context.Posts = context.Posts.OrderByDescending(p => p.Date).ToList();
            }
        }

        private void BuildPost(Dictionary<string, object> config, SiteContext context, string file)
        {
            try
            {
                var contents = SafeReadContents(file);
                var header = contents.YamlHeader();
                var post = new Page
                {
                    Title = header.ContainsKey("title") ? header["title"].ToString() : "this is a post",
                    Date = header.ContainsKey("date") ? DateTime.Parse(header["date"].ToString()) : file.Datestamp(),
                    Content = RenderContent(file, contents, header),
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
            catch (Exception e)
            {
                Tracing.Info(String.Format("Failed to build post from File: {0}", file));
                Tracing.Info(e.Message);
                Tracing.Debug(e.ToString());
            }

        }

        private static string RenderContent(string file, string contents, IDictionary<string, object> header)
        {
            string html;
            try
            {
                var contentsWithoutHeader = contents.ExcludeHeader();
                html = string.Equals(Path.GetExtension(file), ".md", StringComparison.InvariantCultureIgnoreCase)
                       ? Markdown.Transform(contentsWithoutHeader)
                       : contentsWithoutHeader;
            }
            catch (Exception e)
            {
                Tracing.Info(String.Format("Error ({0}) converting {1}", e.Message, file));
                Tracing.Debug(e.ToString());
                html = String.Format("<p><b>Error converting markdown</b></p><pre>{0}</pre>", contents);
            }
            return html;
        }

        private string SafeReadLine(string file)
        {
            string postFirstLine;
            try
            {
                using (var reader = fileSystem.File.OpenText(file))
                {
                    postFirstLine = reader.ReadLine();
                }
            }
            catch (IOException)
            {
                var fileInfo = fileSystem.FileInfo.FromFileName(file);
                var tempFile = Path.Combine(Path.GetTempPath(), fileInfo.Name);
                try
                {
                    fileInfo.CopyTo(tempFile, true);
                    using(var streamReader = fileSystem.File.OpenText(tempFile))
                    {
                        return streamReader.ReadLine();
                    }
                }
                finally
                {
                    if (fileSystem.File.Exists(tempFile))
                        fileSystem.File.Delete(tempFile);
                }
            }
            return postFirstLine;
        }

        private string SafeReadContents(string file)
        {
            try
            {
                return fileSystem.File.ReadAllText(file);
            }
            catch (IOException)
            {
                var fileInfo = fileSystem.FileInfo.FromFileName(file);
                var tempFile = Path.Combine(Path.GetTempPath(), fileInfo.Name);
                try
                {
                    fileInfo.CopyTo(tempFile, true);
                    return fileSystem.File.ReadAllText(tempFile);
                }
                finally
                {
                    if (fileSystem.File.Exists(tempFile))
                        fileSystem.File.Delete(tempFile);                    
                }
            }
        }

        // https://github.com/mojombo/jekyll/wiki/permalinks
        private string EvaluatePermalink(string permalink, Page page)
        {
            permalink = permalink.Replace(":year", page.Date.Year.ToString(CultureInfo.InvariantCulture));
            permalink = permalink.Replace(":month", page.Date.ToString("MM"));
            permalink = permalink.Replace(":day", page.Date.ToString("dd"));
            permalink = permalink.Replace(":title", GetTitle(page.File));

            return permalink;
        }

        private string EvaluatePagePermalink(string permalink, Page page)
        {
            permalink = permalink.Replace(":year", page.Date.Year.ToString(CultureInfo.InvariantCulture));
            permalink = permalink.Replace(":month", page.Date.ToString("MM"));
            permalink = permalink.Replace(":day", page.Date.ToString("dd"));
            permalink = permalink.Replace(":title", GetPageTitle(page.File));

            return permalink;
        }

        // http://stackoverflow.com/questions/6716832/sanitizing-string-to-url-safe-format
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
            return Path.Combine(outputDirectory, timestamp, title);
        }
        private string GetTitle(string file)
        {
            // TODO: detect mode from site config
            var fileName = file.Substring(file.LastIndexOf("\\"));

            var tokens = fileName.Split('-');
            if (tokens.Length < 3)
            {
                return fileName.Substring(1, fileName.LastIndexOf(".") - 1);
            }
            var title = string.Join("-", tokens.Skip(3));
            title = title.Substring(0, title.LastIndexOf("."));
            return title;
        }
        private string GetPageTitle(string file)
        {
            //Page title dont have dates in them so use the full filename as the title
            if (file.Contains("."))
            {
                return file.Substring(0, file.LastIndexOf("."));
            }
            //Something better here?
            return file;
        }
    }
}
