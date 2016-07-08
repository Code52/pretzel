using Pretzel.Logic.Exceptions;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensibility.Extensions;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating.Context;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pretzel.Logic.Templating
{
    public abstract class JekyllEngineBase : ISiteEngine
    {
        private static readonly Regex paragraphRegex = new Regex(@"(<(?:p|h\d{1})>.*?</(?:p|h\d{1})>)", RegexOptions.Compiled | RegexOptions.Singleline);
        protected SiteContext Context;

#pragma warning disable 0649

        [Import]
        public IFileSystem FileSystem { get; set; }

#pragma warning restore 0649

        [ImportMany]
        public IEnumerable<IFilter> Filters { get; set; }

        [ImportMany]
        public IEnumerable<ITag> Tags { get; set; }

        [ImportMany]
        public IEnumerable<TagFactoryBase> TagFactories { get; set; }

        [ImportMany]
        public IEnumerable<IContentTransform> ContentTransformers;

        [Import(AllowDefault = true)]
        private ILightweightMarkupEngine _lightweightMarkupEngine;

        public abstract void Initialize();

        protected abstract void PreProcess();

        protected abstract string RenderTemplate(string content, PageContext pageData);

        public void Process(SiteContext siteContext, bool skipFileOnError = false)
        {
            // Default rendering engine
            if (_lightweightMarkupEngine == null)
            {
                _lightweightMarkupEngine = new CommonMarkEngine();
            }

            Tracing.Logger.Write(string.Format("LightweightMarkupEngine: {0}", _lightweightMarkupEngine.GetType().Name), Tracing.Category.Debug);

            Context = siteContext;
            PreProcess();

            for (int index = 0; index < siteContext.Posts.Count; index++)
            {
                var p = siteContext.Posts[index];
                var previous = GetPrevious(siteContext.Posts, index);
                var next = GetNext(siteContext.Posts, index);
                ProcessFile(siteContext.OutputFolder, p, previous, next, skipFileOnError, p.Filepath);
            }

            for (int index = 0; index < siteContext.Pages.Count; index++)
            {
                var p = siteContext.Pages[index];
                var previous = GetPrevious(siteContext.Pages, index);
                var next = GetNext(siteContext.Pages, index);
                ProcessFile(siteContext.OutputFolder, p, previous, next, skipFileOnError);
            }

            Tracing.Info("\nRedirects:");
            foreach (var post in Context.Posts.Concat(Context.Pages).Where(p => !(p is NonProcessedPage)))
            {
                object redirectFromParam;
                if (post.Bag.TryGetValue("redirect_from", out redirectFromParam))
                {
                    var sourceUrls = redirectFromParam as IEnumerable<string>;
                    if ((sourceUrls != null) && (sourceUrls.Any()))
                    {
                        foreach (var sourceUrl in sourceUrls)
                        {
                            // Read YAML header
                            var redirectBag = new Dictionary<string, object>(post.Bag)
                            {
                                ["layout"] = "redirect",
                                ["redirect_from_url"] = sourceUrl.Trim('/', '\\'),
                                ["redirect_to_url"] = post.Url
                            };

                            ProcessFile(siteContext.OutputFolder, new Page
                            {
                                Title = post.Title,
                                Url = sourceUrl.Trim('/', '\\') + "/",
                                Date = post.Date,
                                Id = post.Id,
                                Categories = post.Categories,
                                Tags = post.Tags,
                                Bag = redirectBag,
                                Content = String.Empty,
                                File = Path.Combine(Context.SourceFolder, sourceUrl.Trim('/', '\\').Replace('/', '\\'), "index.html")
                            }, null, null, skipFileOnError);

                            Tracing.Info(String.Format("Redirect: {0} [{1}]", post.Url, String.Join(", ", sourceUrls.ToArray())));
                        }
                    }
                }
            }
        }

        private static Page GetPrevious(IList<Page> pages, int index)
        {
            return index < pages.Count - 1 ? pages[index + 1] : null;
        }

        private static Page GetNext(IList<Page> pages, int index)
        {
            return index >= 1 ? pages[index - 1] : null;
        }

        private void ProcessFile(string outputDirectory, Page page, Page previous, Page next, bool skipFileOnError,
            string relativePath = "")
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                relativePath = MapToOutputPath(page.File);

            page.OutputFile = Path.Combine(outputDirectory, relativePath);
            var extension = Path.GetExtension(page.File);

            if (extension.IsImageFormat())
            {
                CreateOutputDirectory(page.OutputFile);
                CopyFileIfSourceNewer(page.File, page.OutputFile, true);
                return;
            }

            if (page is NonProcessedPage)
            {
                CreateOutputDirectory(page.OutputFile);
                CopyFileIfSourceNewer(page.File, page.OutputFile, true);
                return;
            }

            if (extension.IsMarkdownFile() || extension.IsRazorFile())
            {
                page.OutputFile = page.OutputFile.Replace(extension, ".html");
            }

            var listType = page.Bag.ContainsKey("list") ? page.Bag["list"] as string : null;
            var pageContexts = new List<PageContext> { };

            if (listType == null)
            {
                var pageContext = PageContext.FromPage(Context, page, outputDirectory, page.OutputFile);

                pageContext.Previous = previous;
                pageContext.Next = next;

                pageContexts.Add(pageContext);

                object paginateObj;
                if (page.Bag.TryGetValue("paginate", out paginateObj))
                {
                    var paginate = Convert.ToInt32(paginateObj);
                    var totalPages = (int)Math.Ceiling(Context.Posts.Count / Convert.ToDouble(paginateObj));
                    var paginator = new Paginator(Context, totalPages, paginate, 1);
                    pageContext.Paginator = paginator;

                    var paginateLink = "/page/:page/index.html";
                    if (page.Bag.ContainsKey("paginate_link"))
                        paginateLink = Convert.ToString(page.Bag["paginate_link"]);

                    var prevLink = page.Url;
                    for (var i = 2; i <= totalPages; i++)
                    {
                        var newPaginator = new Paginator(Context, totalPages, paginate, i) { PreviousPageUrl = prevLink };
                        var link = paginateLink.Replace(":page", Convert.ToString(i));
                        paginator.NextPageUrl = link;

                        paginator = newPaginator;
                        prevLink = link;

                        var path = Path.Combine(outputDirectory, link.ToRelativeFile());
                        if (path.EndsWith(FileSystem.Path.DirectorySeparatorChar.ToString()))
                        {
                            path = Path.Combine(path, "index.html");
                        }
                        var context = new PageContext(pageContext) { Paginator = newPaginator, OutputPath = path };
                        context.Bag["url"] = link;
                        pageContexts.Add(context);
                    }
                }
            }
            else if (listType == "tags")
            {
                var paginateLink = "/tags/:name/:page/";
                if (page.Bag.ContainsKey("paginate_link"))
                    paginateLink = Convert.ToString(page.Bag["paginate_link"]);

                foreach (var tag in Context.Tags)
                {
                    var pageContext = PageContext.FromPage(Context, page, outputDirectory, page.OutputFile);

                    var paginate = int.MaxValue;
                    object paginateObj;
                    if (page.Bag.TryGetValue("paginate", out paginateObj))
                    {
                        paginate = Convert.ToInt32(paginateObj);
                    }
                    var totalPages =
                        (int)
                            Math.Ceiling(Context.Posts.Count(p => p.Tags.Contains(tag.Name)) /
                                         Convert.ToDouble(paginateObj));

                    string prevLink = null;
                    for (var i = 1; i <= totalPages; i++)
                    {
                        var newPaginator = new Paginator(Context, totalPages, paginate, i,
                            p => p.Tags.Contains(tag.Name), tag.Name)
                        { PreviousPageUrl = prevLink };
                        var link = paginateLink.Replace(":name", tag.Name)
                            .Replace(":page", i != 1 ? Convert.ToString(i) : string.Empty)
                            .Replace("//", "/")
                            .Replace("\\\\", "\\");
                        if (i < totalPages)
                        {
                            newPaginator.NextPageUrl = paginateLink.Replace(":name", tag.Name)
                                .Replace(":page", Convert.ToString(i + 1));
                            ;
                        }
                        else
                        {
                            newPaginator.NextPageUrl = null;
                        }

                        prevLink = link;

                        var path = Path.Combine(outputDirectory, link.ToRelativeFile());
                        if (path.EndsWith(FileSystem.Path.DirectorySeparatorChar.ToString()))
                        {
                            path = Path.Combine(path, "index.html");
                        }
                        var context = new PageContext(pageContext) { Paginator = newPaginator, OutputPath = path };
                        context.Bag["url"] = link;
                        pageContexts.Add(context);
                    }
                }
            }
            else if (listType == "categories")
            {
                var paginateLink = "/categories/:name/:page/";
                if (page.Bag.ContainsKey("paginate_link"))
                    paginateLink = Convert.ToString(page.Bag["paginate_link"]);

                foreach (var category in Context.Categories)
                {
                    var pageContext = PageContext.FromPage(Context, page, outputDirectory, page.OutputFile);

                    var paginate = int.MaxValue;
                    object paginateObj;
                    if (page.Bag.TryGetValue("paginate", out paginateObj))
                    {
                        paginate = Convert.ToInt32(paginateObj);
                    }
                    var totalPages =
                        (int)
                            Math.Ceiling(Context.Posts.Count(p => p.Categories.Contains(category.Name)) /
                                         Convert.ToDouble(paginateObj));

                    string prevLink = null;
                    for (var i = 1; i <= totalPages; i++)
                    {
                        var newPaginator = new Paginator(Context, totalPages, paginate, i,
                            p => p.Categories.Contains(category.Name), category.Name)
                        { PreviousPageUrl = prevLink };
                        var link = paginateLink.Replace(":name", category.Name)
                            .Replace(":page", i != 1 ? Convert.ToString(i) : string.Empty)
                            .Replace("//", "/")
                            .Replace("\\\\", "\\");
                        if (i < totalPages)
                        {
                            newPaginator.NextPageUrl = paginateLink.Replace(":name", category.Name)
                                .Replace(":page", Convert.ToString(i + 1));
                            ;
                        }
                        else
                        {
                            newPaginator.NextPageUrl = null;
                        }

                        prevLink = link;

                        var path = Path.Combine(outputDirectory, link.ToRelativeFile());
                        if (path.EndsWith(FileSystem.Path.DirectorySeparatorChar.ToString()))
                        {
                            path = Path.Combine(path, "index.html");
                        }
                        var context = new PageContext(pageContext) { Paginator = newPaginator, OutputPath = path };
                        context.Bag["url"] = link;
                        pageContexts.Add(context);
                    }
                }
            }

            foreach (var context in pageContexts)
            {
                var stopwatch = Stopwatch.StartNew();

                var metadata = page.Bag;
                var failed = false;

                var excerptSeparator = context.Bag.ContainsKey("excerpt_separator")
                    ? context.Bag["excerpt_separator"].ToString()
                    : Context.ExcerptSeparator;
                try
                {
                    if ((context.Paginator != null) && (context.Paginator.Posts != null))
                    {
                        foreach (var post in context.Paginator.Posts)
                        {
                            var postContent = RenderContent(post.File, RenderTemplate(post.Content, context));
                            post.Excerpt = GetContentExcerpt(postContent, excerptSeparator);
                        }
                    }

                    context.Content = RenderContent(page.File, RenderTemplate(context.Content, context));
                    context.FullContent = context.Content;
                    context.Bag["excerpt"] = GetContentExcerpt(context.Content, excerptSeparator);
                }
                catch (Exception ex)
                {
                    if (!skipFileOnError)
                    {
                        var message = string.Format("Failed to process {0}, see inner exception for more details",
                            context.OutputPath);
                        throw new PageProcessingException(message, ex);
                    }

                    Console.WriteLine(@"Failed to process {0}: {1}", context.OutputPath, ex);
                    continue;
                }

                while (metadata.ContainsKey("layout"))
                {
                    var layout = metadata["layout"];
                    if ((string)layout == "nil" || layout == null)
                        break;

                    var path = FindLayoutPath(layout.ToString());

                    if (path == null)
                        break;

                    try
                    {
                        metadata = ProcessTemplate(context, path);
                    }
                    catch (Exception ex)
                    {
                        if (!skipFileOnError)
                        {
                            var message =
                                string.Format(
                                    "Failed to process layout {0} for {1}, see inner exception for more details", layout,
                                    context.OutputPath);
                            throw new PageProcessingException(message, ex);
                        }

                        Console.WriteLine(@"Failed to process layout {0} for {1} because '{2}'. Skipping file", layout,
                            context.OutputPath, ex.Message);
                        failed = true;
                        break;
                    }
                }
                if (failed)
                {
                    continue;
                }

                try
                {
                    context.FullContent = RenderTemplate(context.FullContent, context);
                }
                catch (Exception ex)
                {
                    if (!skipFileOnError)
                    {
                        var message = string.Format("Failed to process {0}, see inner exception for more details",
                            context.OutputPath);
                        throw new PageProcessingException(message, ex);
                    }

                    Console.WriteLine(@"Failed to process {0}: {1}", context.OutputPath, ex);
                    continue;
                }

                CreateOutputDirectory(context.OutputPath);
                FileSystem.File.WriteAllText(context.OutputPath, context.FullContent);

                stopwatch.Stop();

                Tracing.Info(String.Format("  {0} [{1}]", context.OutputPath.Replace(context.Site.OutputFolder, String.Empty), stopwatch.Elapsed));
            }
        }

        private string RenderContent(string file, string contents)
        {
            string html;
            try
            {
                var contentsWithoutHeader = contents.ExcludeHeader();

                html = Path.GetExtension(file).IsMarkdownFile()
                       ? _lightweightMarkupEngine.Convert(contentsWithoutHeader).Trim()
                       : contentsWithoutHeader;

                if (ContentTransformers != null)
                {
                    html = ContentTransformers.Aggregate(html, (current, contentTransformer) => contentTransformer.Transform(current));
                }
            }
            catch (Exception e)
            {
                Tracing.Info(String.Format("Error ({0}) converting {1}", e.Message, file));
                Tracing.Debug(e.ToString());
                html = String.Format("<p><b>Error converting markdown</b></p><pre>{0}</pre>", contents);
            }
            return html;
        }

        private static string GetContentExcerpt(string content, string excerptSeparator)
        {
            var excerptSeparatorIndex = content.IndexOf(excerptSeparator, StringComparison.InvariantCulture);
            string excerpt = null;
            if (excerptSeparatorIndex == -1)
            {
                var match = paragraphRegex.Match(content);
                if (match.Success)
                {
                    excerpt = match.Groups[1].Value;
                }
            }
            else
            {
                excerpt = content.Substring(0, excerptSeparatorIndex);
                if (excerpt.StartsWith("<p>") && !excerpt.EndsWith("</p>"))
                {
                    excerpt += "</p>";
                }
            }
            return excerpt;
        }

        public void CopyFileIfSourceNewer(string sourceFileName, string destFileName, bool overwrite)
        {
            if (!FileSystem.File.Exists(destFileName) ||
                FileSystem.File.GetLastWriteTime(sourceFileName) > FileSystem.File.GetLastWriteTime(destFileName))
            {
                FileSystem.File.Copy(sourceFileName, destFileName, overwrite);
            }
        }

        private void CreateOutputDirectory(string outputFile)
        {
            var directory = Path.GetDirectoryName(outputFile);
            if (!FileSystem.Directory.Exists(directory))
                FileSystem.Directory.CreateDirectory(directory);
        }

        private static readonly string[] layoutExtensions = { ".html", ".htm" };

        protected virtual string[] LayoutExtensions
        {
            get { return layoutExtensions; }
        }

        private IDictionary<string, object> ProcessTemplate(PageContext pageContext, string path)
        {
            var templateFile = FileSystem.File.ReadAllText(path);
            var metadata = templateFile.YamlHeader();
            var templateContent = templateFile.ExcludeHeader();

            pageContext.FullContent = RenderTemplate(templateContent, pageContext);

            return metadata;
        }

        private string MapToOutputPath(string file)
        {
            var temp = file.Replace(Context.SourceFolder, "")
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return temp;
        }

        public bool CanProcess(SiteContext context)
        {
            var engineInfo = GetType().GetCustomAttributes(typeof(SiteEngineInfoAttribute), true).SingleOrDefault() as SiteEngineInfoAttribute;
            if (engineInfo == null) return false;
            return context.Engine == engineInfo.Engine;
        }

        private string FindLayoutPath(string layout)
        {
            foreach (var extension in LayoutExtensions)
            {
                var path = Path.Combine(Context.SourceFolder, "_layouts", layout + extension);
                if (FileSystem.File.Exists(path))
                    return path;
            }

            return null;
        }
    }
}