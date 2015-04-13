using Pretzel.Logic.Exceptions;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating.Context;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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

        public abstract void Initialize();

        protected abstract void PreProcess();

        protected abstract string RenderTemplate(string content, PageContext pageData);

        public void Process(SiteContext siteContext, bool skipFileOnError = false)
        {
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
        }

        private static Page GetNext(IList<Page> pages, int index)
        {
            return index < pages.Count - 1 ? pages[index + 1] : null;
        }

        private static Page GetPrevious(IList<Page> pages, int index)
        {
            return index >= 1 ? pages[index - 1] : null;
        }

        private void ProcessFile(string outputDirectory, Page page, Page previous, Page next, bool skipFileOnError, string relativePath = "")
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
                page.OutputFile = page.OutputFile.Replace(extension, ".html");

            var pageContext = PageContext.FromPage(Context, page, outputDirectory, page.OutputFile);

            pageContext.Previous = previous;
            pageContext.Next = next;

            var pageContexts = new List<PageContext> { pageContext };
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
                    pageContexts.Add(new PageContext(pageContext) { Paginator = newPaginator, OutputPath = path });
                }
            }

            foreach (var context in pageContexts)
            {
                var metadata = page.Bag;
                var failed = false;

                var excerptSeparator = context.Bag.ContainsKey("excerpt_separator")
                    ? context.Bag["excerpt_separator"].ToString()
                    : Context.ExcerptSeparator;
                try
                {
                    context.Bag["excerpt"] = GetContentExcerpt(RenderTemplate(context.Content, context), excerptSeparator);
                }
                catch (Exception ex)
                {
                    if (!skipFileOnError)
                    {
                        var message = string.Format("Failed to process {0}, see inner exception for more details", context.OutputPath);
                        throw new PageProcessingException(message, ex);
                    }

                    Console.WriteLine(@"Failed to process {0}, see inner exception for more details", context.OutputPath);
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
                            var message = string.Format("Failed to process layout {0} for {1}, see inner exception for more details", layout, context.OutputPath);
                            throw new PageProcessingException(message, ex);
                        }

                        Console.WriteLine(@"Failed to process layout {0} for {1} because '{2}'. Skipping file", layout, context.OutputPath, ex.Message);
                        failed = true;
                        break;
                    }
                }
                if (failed)
                    continue;

                try
                {
                    context.Content = RenderTemplate(context.Content, context);
                }
                catch (Exception ex)
                {
                    if (!skipFileOnError)
                    {
                        var message = string.Format("Failed to process {0}, see inner exception for more details", context.OutputPath);
                        throw new PageProcessingException(message, ex);
                    }

                    Console.WriteLine(@"Failed to process {0}, see inner exception for more details", context.OutputPath);
                    continue;
                }

                CreateOutputDirectory(context.OutputPath);
                FileSystem.File.WriteAllText(context.OutputPath, context.Content);
            }
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

            pageContext.Content = RenderTemplate(templateContent, pageContext);
            return metadata;
        }

        private string MapToOutputPath(string file)
        {
            return file.Replace(Context.SourceFolder, "").TrimStart('\\');
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
