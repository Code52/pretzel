using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MarkdownDeep;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensions;

namespace Pretzel.Logic.Templating.Context
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SiteContextGenerator
    {
        readonly Dictionary<string, Page> pageCache = new Dictionary<string, Page>();
        static readonly Markdown Markdown = new Markdown();
        readonly IFileSystem fileSystem;
        readonly IEnumerable<IContentTransform> contentTransformers;

        [ImportingConstructor]
        public SiteContextGenerator(IFileSystem fileSystem, [ImportMany]IEnumerable<IContentTransform> contentTransformers)
        {
            this.fileSystem = fileSystem;
            this.contentTransformers = contentTransformers;
        }

        public SiteContext BuildContext(string path)
        {
            try
            {
                var config = new Dictionary<string, object>();
                var configPath = Path.Combine(path, "_config.yml");
                if (fileSystem.File.Exists(configPath))
                    config = (Dictionary<string, object>)fileSystem.File.ReadAllText(configPath).YamlHeader(true);

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

                context.Posts = BuildPosts(config, context).OrderByDescending(p => p.Date).ToList();
                BuildTagsAndCategories(context);

                context.Pages = BuildPages(config, context).ToList();

                return context;
            }
            finally
            {
                pageCache.Clear();
            }
        }

        private IEnumerable<Page> BuildPages(Dictionary<string, object> config, SiteContext context)
        {
            var files = from file in fileSystem.Directory.GetFiles(context.SourceFolder, "*.*", SearchOption.AllDirectories)
                        let relativePath = MapToOutputPath(context, file)
                        where !IsSpecialPath(relativePath)
                        select file;

            foreach (var file in files)
            {
                if (!ContainsYamlFrontMatter(file))
                {
                    yield return new NonProcessedPage
                                     {
                                         File = file,
                                         Filepath = Path.Combine(context.OutputFolder, file)
                                     };
                }
                else
                {
                    var page = CreatePage(context, config, file, false);

                    if (page != null)
                        yield return page;
                }
            }
        }

        private IEnumerable<Page> BuildPosts(Dictionary<string, object> config, SiteContext context)
        {
            var postsFolder = Path.Combine(context.SourceFolder, "_posts");
            if (fileSystem.Directory.Exists(postsFolder))
            {
                return fileSystem.Directory
                    .GetFiles(postsFolder, "*.*", SearchOption.AllDirectories)
                    .Select(file => CreatePage(context, config, file, true))
                    .Where(post => post != null);
            }

            return Enumerable.Empty<Page>();
        }

        private static void BuildTagsAndCategories(SiteContext context)
        {
            var tags = new Dictionary<string, List<Page>>();
            var categories = new Dictionary<string, List<Page>>();

            foreach (var post in context.Posts)
            {
                if (post.Tags != null)
                {
                    foreach (var tagName in post.Tags)
                    {
                        if (tags.ContainsKey(tagName))
                        {
                            tags[tagName].Add(post);
                        }
                        else
                        {
                            tags.Add(tagName, new List<Page> {post});
                        }
                    }
                }

                if (post.Categories != null)
                {
                    foreach (var catName in post.Categories)
                    {
                        if (categories.ContainsKey(catName))
                        {
                            categories[catName].Add(post);
                        }
                        else
                        {
                            categories.Add(catName, new List<Page> { post });
                        }
                    }  
                }
                
            }

            context.Tags = tags.Select(x => new Tag { Name = x.Key, Posts = x.Value }).OrderBy(x => x.Name).ToList();
            context.Categories = categories.Select(x => new Category { Name = x.Key, Posts = x.Value }).OrderBy(x=>x.Name).ToList();
        }

        private bool ContainsYamlFrontMatter(string file)
        {
            var postFirstLine = SafeReadLine(file);

            return postFirstLine != null && postFirstLine.StartsWith("---");
        }

        private static bool IsSpecialPath(string relativePath)
        {
            return relativePath.StartsWith("_") || (relativePath.StartsWith(".") && relativePath != ".htaccess");
        }

        private Page CreatePage(SiteContext context, IDictionary<string, object> config, string file, bool isPost)
        {
            try
            {
                if (pageCache.ContainsKey(file))
                    return pageCache[file];
                var contents = SafeReadContents(file);
                var header = contents.YamlHeader();
                var page = new Page
                                {
                                    Title = header.ContainsKey("title") ? header["title"].ToString() : "this is a post",
                                    Date = header.ContainsKey("date") ? DateTime.Parse(header["date"].ToString()) : file.Datestamp(),
                                    Content = RenderContent(file, contents, header),
                                    Filepath = GetPathWithTimestamp(context.OutputFolder, file),
                                    File = file,
                                    Bag = header,
                                };

                if (header.ContainsKey("permalink"))
                    page.Url = EvaluatePermalink(header["permalink"].ToString(), page);
                else if (config.ContainsKey("permalink"))
                    page.Url = EvaluatePermalink(config["permalink"].ToString(), page);

                // The GetDirectoryPage method is reentrant, we need a cache to stop a stack overflow :)
                pageCache.Add(file, page);
                page.DirectoryPages = GetDirectoryPages(context, config, Path.GetDirectoryName(file), isPost).ToList();

                if (isPost)
                {
                    if (header.ContainsKey("categories"))
                        page.Categories = header["categories"] as IEnumerable<string>;

                    if (header.ContainsKey("tags"))
                        page.Tags = header["tags"] as IEnumerable<string>;
                }
                return page;
            }
            catch (Exception e)
            {
                Tracing.Info(String.Format("Failed to build post from File: {0}", file));
                Tracing.Info(e.Message);
                Tracing.Debug(e.ToString());
            }

            return null;
        }

        private IEnumerable<Page> GetDirectoryPages(SiteContext context, IDictionary<string, object> config, string forDirectory, bool isPost)
        {
            return fileSystem
                .Directory
                .GetFiles(forDirectory, "*.*", SearchOption.TopDirectoryOnly)
                .Select(file => CreatePage(context, config, file, isPost))
                .Where(page => page != null);
        }

        private string RenderContent(string file, string contents, IDictionary<string, object> header)
        {
            string html;
            try
            {
                var contentsWithoutHeader = contents.ExcludeHeader();
                html = string.Equals(Path.GetExtension(file), ".md", StringComparison.InvariantCultureIgnoreCase)
                       ? Markdown.Transform(contentsWithoutHeader)
                       : contentsWithoutHeader;

                html = contentTransformers.Aggregate(html, (current, contentTransformer) => contentTransformer.Transform(current));
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
                    using (var streamReader = fileSystem.File.OpenText(tempFile))
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

        static readonly Regex TimestampAndTitleFromPathRegex = new Regex(@"\\(?:(?<timestamp>\d+-\d+-\d+)-)?(?<title>[^\\]*)\.[^\.]+$");
        public static string GetTitle(string file)
        {
            return TimestampAndTitleFromPathRegex.Match(file).Groups["title"].Value;
        }
        private string GetPageTitle(string file)
        {
			  return Path.GetFileNameWithoutExtension(file);
        }
    }
}
