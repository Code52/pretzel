using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;

namespace Pretzel.Logic.Templating.Context
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SiteContextGenerator
    {
        private readonly Dictionary<string, Page> pageCache = new Dictionary<string, Page>();
        private readonly IFileSystem fileSystem;
        private readonly IEnumerable<IContentTransform> contentTransformers;
        private readonly List<string> includes = new List<string>();
        private readonly List<string> excludes = new List<string>();
        private readonly LinkHelper linkHelper;

        [ImportingConstructor]
        public SiteContextGenerator(IFileSystem fileSystem, [ImportMany]IEnumerable<IContentTransform> contentTransformers, LinkHelper linkHelper)
        {
            this.fileSystem = fileSystem;
            this.contentTransformers = contentTransformers;
            this.linkHelper = linkHelper;
        }

        public SiteContext BuildContext(string path, string destinationPath, bool includeDrafts)
        {
            try
            {
                var config = new Dictionary<string, object>();
                var configPath = Path.Combine(path, "_config.yml");
                if (fileSystem.File.Exists(configPath))
                    config = (Dictionary<string, object>)fileSystem.File.ReadAllText(configPath).YamlHeader(true);

                if (!config.ContainsKey("permalink"))
                {
                    config.Add("permalink", "date");
                }

                if (config.ContainsKey("include"))
                {
                    includes.AddRange((IEnumerable<string>)config["include"]);
                }
                if (config.ContainsKey("exclude"))
                {
                    excludes.AddRange((IEnumerable<string>)config["exclude"]);
                }

                var context = new SiteContext
                {
                    SourceFolder = path,
                    OutputFolder = destinationPath,
                    Posts = new List<Page>(),
                    Pages = new List<Page>(),
                    Config = config,
                    Time = DateTime.Now,
                    UseDrafts = includeDrafts
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
                        where CanBeIncluded(relativePath)
                        select file;

            foreach (var file in files)
            {
                if (!ContainsYamlFrontMatter(file))
                {
                    yield return new NonProcessedPage
                                     {
                                         File = file,
                                         Filepath = Path.Combine(context.OutputFolder, MapToOutputPath(context, file))
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
            var posts = new List<Page>();

            var postsFolders = fileSystem.Directory.GetDirectories(context.SourceFolder, "_posts", SearchOption.AllDirectories);

            foreach (var postsFolder in postsFolders)
            {
                posts.AddRange(fileSystem.Directory
                    .GetFiles(postsFolder, "*.*", SearchOption.AllDirectories)
                    .Select(file => CreatePage(context, config, file, true))
                    .Where(post => post != null)
                );
            }

            var draftsFolder = Path.Combine(context.SourceFolder, "_drafts");
            if (context.UseDrafts && fileSystem.Directory.Exists(draftsFolder))
            {
                posts.AddRange(fileSystem.Directory
                    .GetFiles(draftsFolder, "*.*", SearchOption.AllDirectories)
                    .Select(file => CreatePage(context, config, file, true))
                    .Where(post => post != null)
                );
            }

            return posts;
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
                            tags.Add(tagName, new List<Page> { post });
                        }
                    }
                }

                if (post.Categories != null)
                {
                    foreach (var categoryName in post.Categories)
                    {
                        AddCategory(categories, categoryName, post);
                    }
                }
            }

            context.Tags = tags.Select(x => new Tag { Name = x.Key, Posts = x.Value }).OrderBy(x => x.Name).ToList();
            context.Categories = categories.Select(x => new Category { Name = x.Key, Posts = x.Value }).OrderBy(x => x.Name).ToList();
        }

        private static void AddCategory(Dictionary<string, List<Page>> categories, string categoryName, Page post)
        {
            if (categories.ContainsKey(categoryName))
            {
                categories[categoryName].Add(post);
            }
            else
            {
                categories.Add(categoryName, new List<Page> { post });
            }
        }

        private bool ContainsYamlFrontMatter(string file)
        {
            var postFirstLine = SafeReadLine(file);

            return postFirstLine != null && postFirstLine.StartsWith("---");
        }

        private bool IsExcludedPath(string relativePath)
        {
            return excludes.Contains(relativePath) || excludes.Any(e => relativePath.StartsWith(e));
        }

        private bool IsIncludedPath(string relativePath)
        {
            return includes.Contains(relativePath) || includes.Any(e => relativePath.StartsWith(e));
        }

        public bool CanBeIncluded(string relativePath)
        {
            if (excludes.Count > 0 && IsExcludedPath(relativePath))
            {
                return false;
            }

            if (includes.Count > 0 && IsIncludedPath(relativePath))
            {
                return true;
            }

            return !IsSpecialPath(relativePath);
        }

        public static bool IsSpecialPath(string relativePath)
        {
            return relativePath.StartsWith("_")
                    || relativePath.Contains("_posts")
                    || (relativePath.StartsWith(".") && relativePath != ".htaccess")
                    || relativePath.EndsWith(".TMP", StringComparison.OrdinalIgnoreCase);
        }

        private Page CreatePage(SiteContext context, IDictionary<string, object> config, string file, bool isPost)
        {
            try
            {
                if (pageCache.ContainsKey(file))
                    return pageCache[file];
                var contents = SafeReadContents(file);
                var header = contents.YamlHeader();
                var content = RenderContent(file, contents, header);

                if (header.ContainsKey("published") && header["published"].ToString().ToLower() == "false")
                {
                    return null;
                }

                var page = new Page
                                {
                                    Title = header.ContainsKey("title") ? header["title"].ToString() : "this is a post",
                                    Date = header.ContainsKey("date") ? DateTime.Parse(header["date"].ToString()) : file.Datestamp(),
                                    Content = content,
                                    Filepath = isPost ? GetPathWithTimestamp(context.OutputFolder, file) : GetFilePathForPage(context, file),
                                    File = file,
                                    Bag = header,
                                };

                // resolve categories and tags
                if (isPost)
                {
                    page.Categories = ResolveCategories(context, header, page);

                    if (header.ContainsKey("tags"))
                        page.Tags = header["tags"] as IEnumerable<string>;
                }

                // resolve permalink
                if (header.ContainsKey("permalink"))
                {
                    page.Url = linkHelper.EvaluatePermalink(header["permalink"].ToString(), page);
                }
                else if (isPost && config.ContainsKey("permalink"))
                {
                    page.Url = linkHelper.EvaluatePermalink(config["permalink"].ToString(), page);
                }
                else
                {
                    page.Url = linkHelper.EvaluateLink(context, page);
                }

                // resolve id
                page.Id = page.Url.Replace(".html", string.Empty).Replace("index", string.Empty);

                // ensure the date is accessible in the hash
                if (!page.Bag.ContainsKey("date"))
                {
                    page.Bag["date"] = page.Date;
                }

                // The GetDirectoryPage method is reentrant, we need a cache to stop a stack overflow :)
                pageCache.Add(file, page);
                page.DirectoryPages = GetDirectoryPages(context, config, Path.GetDirectoryName(file), isPost).ToList();

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

        private List<string> ResolveCategories(SiteContext context, IDictionary<string, object> header, Page page)
        {
            var categories = new List<string>();

            var postPath = page.File.Replace(context.SourceFolder, string.Empty);
            string rawCategories = postPath.Replace(fileSystem.Path.GetFileName(page.File), string.Empty).Replace("_posts", string.Empty);
            categories.AddRange(rawCategories.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries));

            if (header.ContainsKey("categories") && header["categories"] is IEnumerable<string>)
            {
                categories.AddRange((IEnumerable<string>)header["categories"]);
            }
            else if (header.ContainsKey("category"))
            {
                categories.Add((string)header["category"]);
            }

            return categories;
        }

        private string GetFilePathForPage(SiteContext context, string file)
        {
            return Path.Combine(context.OutputFolder, MapToOutputPath(context, file));
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

                html = Path.GetExtension(file).IsMarkdownFile()
                       ? CommonMark.CommonMarkConverter.Convert(contentsWithoutHeader).Trim()
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
                if (SanityCheck.IsLockedByAnotherProcess(file))
                {
                    Tracing.Info(String.Format("File {0} is locked by another process. Skipping", file));
                    return string.Empty;
                }

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
    }
}
