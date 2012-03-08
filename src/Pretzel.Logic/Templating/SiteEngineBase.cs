using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating.Context;
using System.ComponentModel.Composition;

namespace Pretzel.Logic.Templating
{
    public abstract class SiteEngineBase : ISiteEngine
    {
        protected SiteContext Context;

#pragma warning disable 0649
        [Import] public IFileSystem FileSystem { get; set; }
#pragma warning restore 0649


        public abstract void Initialize();
        protected abstract void PreProcess();
        protected abstract string RenderTemplate(string content, PageContext pageData);

        public void Process(SiteContext siteContext)
        {
            Context = siteContext;
            PreProcess();

            var outputDirectory = Path.Combine(Context.SourceFolder, "_site");

            foreach (var p in siteContext.Posts)
            {
                ProcessFile(outputDirectory, p, p.Filepath);
            }

            foreach (var p in siteContext.Pages)
            {
                ProcessFile(outputDirectory, p);
            }
        }

        public virtual string GetOutputDirectory(string path)
        {
            return Path.Combine(path, "_site");
        }

        private void ProcessFile(string outputDirectory, Page page, string relativePath = "")
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                relativePath = MapToOutputPath(page.File);

            page.OutputFile = Path.Combine(outputDirectory, relativePath);

            var directory = Path.GetDirectoryName(page.OutputFile);
            if (!FileSystem.Directory.Exists(directory))
                FileSystem.Directory.CreateDirectory(directory);

            var extension = Path.GetExtension(page.File);

            if (extension.IsImageFormat())
            {
                FileSystem.File.Copy(page.File, page.OutputFile, true);
                return;
            }

            if (page is NonProcessedPage)
            {
                FileSystem.File.Copy(page.File, page.OutputFile, true);
                return;
            }

            if (extension.IsMarkdownFile())
                page.OutputFile = page.OutputFile.Replace(extension, ".html");

            var pageContext = PageContext.FromPage(page, outputDirectory, page.OutputFile);
            var metadata = page.Bag;
            while (metadata.ContainsKey("layout"))
            {
                if ((string)metadata["layout"] == "nil" || metadata["layout"] == null)
                    break;

                var path = Path.Combine(Context.SourceFolder, "_layouts", metadata["layout"] + ".html");

                if (!FileSystem.File.Exists(path))
                    break;

                metadata = ProcessTemplate(pageContext, path);
            }

            pageContext.Content = RenderTemplate(pageContext.Content, pageContext);

            FileSystem.File.WriteAllText(pageContext.OutputPath, pageContext.Content);
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

        public bool CanProcess(string directory)
        {
            var configPath = Path.Combine(directory, "_config.yml");
            return FileSystem.File.Exists(configPath);
        }
    }
}