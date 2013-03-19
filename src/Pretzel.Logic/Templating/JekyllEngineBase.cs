using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Pretzel.Logic.Exceptions;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Logic.Templating
{
    public abstract class JekyllEngineBase : ISiteEngine
    {
        protected SiteContext Context;

#pragma warning disable 0649
        [Import] public IFileSystem FileSystem { get; set; }
#pragma warning restore 0649
        
        [ImportMany]
        public IEnumerable<IFilter> Filters { get; set; }

        public abstract void Initialize();
        protected abstract void PreProcess();
        protected abstract string RenderTemplate(string content, PageContext pageData);

        public void Process(SiteContext siteContext)
        {
            Context = siteContext;
            PreProcess();

            var outputDirectory = Path.Combine(Context.SourceFolder, "_site");

            for (int index = 0; index < siteContext.Posts.Count; index++)
            {
                var p = siteContext.Posts[index];
                var previous = GetPrevious(siteContext.Posts, index);
                var next = GetNext(siteContext.Posts, index);
                ProcessFile(outputDirectory, p, previous, next, p.Filepath);
            }

            for (int index = 0; index < siteContext.Pages.Count; index++)
            {
                var p = siteContext.Pages[index];
                var previous = GetPrevious(siteContext.Pages, index);
                var next = GetNext(siteContext.Pages, index);
                ProcessFile(outputDirectory, p, previous, next);
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

        public virtual string GetOutputDirectory(string path)
        {
            return Path.Combine(path, "_site");
        }

        private void ProcessFile(string outputDirectory, Page page, Page previous, Page next, string relativePath = "")
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
            //pageContext.Content = markdown.Transform(pageContext.Content);
            pageContext.Previous = previous;
            pageContext.Next = next;

            var pageContexts = new List<PageContext> {pageContext};
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
                    var newPaginator = new Paginator(Context, totalPages, paginate, i) {PreviousPageUrl = prevLink};
                    var link = paginateLink.Replace(":page", Convert.ToString(i));
                    paginator.NextPageUrl = link;
                    
                    paginator = newPaginator;
                    prevLink = link;

                    var path = Path.Combine(outputDirectory, link.ToRelativeFile());
                    pageContexts.Add(new PageContext(pageContext) {Paginator = newPaginator, OutputPath = path});
                }
            }

            foreach (var context in pageContexts)
            {
                var metadata = page.Bag;
                while (metadata.ContainsKey("layout"))
                {
                    var layout = metadata["layout"];
                    if ((string) layout == "nil" || layout == null)
                        break;

                    var path = Path.Combine(Context.SourceFolder, "_layouts", layout + LayoutExtension);

                    if (!FileSystem.File.Exists(path))
                        break;

                    try
                    {
                        metadata = ProcessTemplate(context, path);
                    }
                    catch (Exception ex)
                    {
                        throw new PageProcessingException(
                            string.Format("Failed to process layout {0} for {1}, see inner exception for more details",
                                          layout, context.OutputPath), ex);
                    }
                }

                try
                {
                    context.Content = RenderTemplate(context.Content, context);
                }
                catch (Exception ex)
                {
                    throw new PageProcessingException(
                        string.Format("Failed to process {0}, see inner exception for more details",
                                      context.OutputPath), ex);
                }

                CreateOutputDirectory(context.OutputPath);
                FileSystem.File.WriteAllText(context.OutputPath, context.Content);
            }
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

    	protected virtual string LayoutExtension
        {
            get { return ".html"; }
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
            var engineInfo = GetType().GetCustomAttributes(typeof (SiteEngineInfoAttribute), true).SingleOrDefault() as SiteEngineInfoAttribute;
            if (engineInfo == null) return false;
            return context.Engine == engineInfo.Engine;
        }
    }
}