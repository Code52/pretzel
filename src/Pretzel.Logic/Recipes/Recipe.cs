using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Abstractions;
using System.Reflection;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensions;

namespace Pretzel.Logic.Recipes
{
    public class Recipe
    {
        public Recipe(IFileSystem fileSystem, string engine, string directory, IEnumerable<IAdditionalIngredient> additionalIngredients, bool withProject, bool wiki, bool withDrafts = false)
        {
            this.fileSystem = fileSystem;
            this.engine = engine;
            this.directory = directory;
            this.additionalIngredients = additionalIngredients;
            this.withProject = withProject;
            this.wiki = wiki;
            this.withDrafts = withDrafts;
        }

        private readonly IFileSystem fileSystem;
        private readonly string engine;
        private readonly string directory;
        private readonly IEnumerable<IAdditionalIngredient> additionalIngredients;
        private readonly bool withProject;
        private readonly bool wiki;
        private readonly bool withDrafts;

        public void Create()
        {
            try
            {
                if (!fileSystem.Directory.Exists(directory))
                    fileSystem.Directory.CreateDirectory(directory);

                if (string.Equals("razor", engine, StringComparison.InvariantCultureIgnoreCase))
                {
                    CreateDirectories();

                    if (wiki)
                    {
                        fileSystem.File.WriteAllText(Path.Combine(directory, @"_layouts", "layout.cshtml"), Properties.RazorWiki.Layout);
                        fileSystem.File.WriteAllText(Path.Combine(directory, @"index.md"), Properties.RazorWiki.Index);
                        fileSystem.File.WriteAllText(Path.Combine(directory, @"css", "style.css"), Properties.RazorWiki.Style);
                        fileSystem.File.WriteAllText(Path.Combine(directory, @"css", "default.css"), Properties.RazorWiki.DefaultStyle);
                        fileSystem.File.WriteAllText(Path.Combine(directory, @"_config.yml"), Properties.Razor.Config);
                        CreateFavicon();
                    }
                    else
                    {
                        fileSystem.File.WriteAllText(Path.Combine(directory, @"rss.xml"), Properties.Razor.Rss);
                        fileSystem.File.WriteAllText(Path.Combine(directory, @"atom.xml"), Properties.Razor.Atom);
                        fileSystem.File.WriteAllText(Path.Combine(directory, @"sitemap.xml"), Properties.Razor.Sitemap);
                        fileSystem.File.WriteAllText(Path.Combine(directory, @"_layouts", "layout.cshtml"), Properties.Razor.Layout);
                        fileSystem.File.WriteAllText(Path.Combine(directory, @"_layouts", "post.cshtml"), Properties.Razor.Post);
                        fileSystem.File.WriteAllText(Path.Combine(directory, @"index.cshtml"), Properties.Razor.Index);
                        fileSystem.File.WriteAllText(Path.Combine(directory, @"about.md"), Properties.Razor.About);
                        fileSystem.File.WriteAllText(Path.Combine(directory, @"_posts", string.Format("{0}-myfirstpost.md", DateTime.Today.ToString("yyyy-MM-dd"))), Properties.Razor.FirstPost);
                        fileSystem.File.WriteAllText(Path.Combine(directory, @"css", "style.css"), Properties.Resources.Style);
                        fileSystem.File.WriteAllText(Path.Combine(directory, @"_config.yml"), Properties.Razor.Config);
                        fileSystem.File.WriteAllText(Path.Combine(directory, @"_includes", "head.cshtml"), Properties.Razor.Head);
                        CreateImages();
                    }

                    if (withProject)
                        CreateProject();

                    Tracing.Info("Pretzel site template has been created");
                }
                else if (string.Equals("liquid", engine, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (wiki)
                        Tracing.Info("Wiki switch not valid with liquid templating engine");
                    CreateDirectories();

                    fileSystem.File.WriteAllText(Path.Combine(directory, @"rss.xml"), Properties.Liquid.Rss);
                    fileSystem.File.WriteAllText(Path.Combine(directory, @"atom.xml"), Properties.Liquid.Atom);
                    fileSystem.File.WriteAllText(Path.Combine(directory, @"sitemap.xml"), Properties.Liquid.Sitemap);

                    fileSystem.File.WriteAllText(Path.Combine(directory, @"_layouts", "layout.html"), Properties.Liquid.Layout);
                    fileSystem.File.WriteAllText(Path.Combine(directory, @"_layouts", "post.html"), Properties.Liquid.Post);
                    fileSystem.File.WriteAllText(Path.Combine(directory, @"index.html"), Properties.Liquid.Index);
                    fileSystem.File.WriteAllText(Path.Combine(directory, @"about.md"), Properties.Liquid.About);
                    fileSystem.File.WriteAllText(Path.Combine(directory, @"_posts", string.Format("{0}-myfirstpost.md", DateTime.Today.ToString("yyyy-MM-dd"))), Properties.Liquid.FirstPost);
                    fileSystem.File.WriteAllText(Path.Combine(directory, @"css", "style.css"), Properties.Resources.Style);
                    fileSystem.File.WriteAllText(Path.Combine(directory, @"_config.yml"), Properties.Liquid.Config);
                    fileSystem.File.WriteAllText(Path.Combine(directory, @"_includes", "head.html"), Properties.Liquid.Head);

                    CreateImages();

                    Tracing.Info("Pretzel site template has been created");
                }
                else
                {
                    Tracing.Info("Templating Engine not found");
                    return;
                }

                foreach (var additionalIngredient in additionalIngredients)
                {
                    additionalIngredient.MixIn(directory);
                }
            }
            catch (Exception ex)
            {
                Tracing.Error("Error trying to create template: {0}", ex);
            }
        }

        private void CreateProject()
        {
            var layoutDirectory = Path.Combine(directory, "_layouts");
            fileSystem.Directory.CreateDirectory(Path.Combine(layoutDirectory, @"Properties"));
            fileSystem.Directory.CreateDirectory(Path.Combine(layoutDirectory, @"PretzelClasses"));
            fileSystem.Directory.CreateDirectory(Path.Combine(layoutDirectory, @".nuget"));

            fileSystem.File.WriteAllBytes(Path.Combine(layoutDirectory, @"Properties", "AssemblyInfo.cs"), Properties.RazorCsProject.AssemblyInfo_cs);
            fileSystem.File.WriteAllBytes(Path.Combine(layoutDirectory, @"PretzelClasses", "Category.cs"), Properties.RazorCsProject.Category_cs);
            fileSystem.File.WriteAllBytes(Path.Combine(layoutDirectory, @"LayoutProject.csproj"), Properties.RazorCsProject.LayoutProject_csproj);
            fileSystem.File.WriteAllBytes(Path.Combine(layoutDirectory, @"layoutSolution.sln"), Properties.RazorCsProject.LayoutSolution_sln);
            fileSystem.File.WriteAllBytes(Path.Combine(layoutDirectory, @"PretzelClasses", "NonProcessedPage.cs"), Properties.RazorCsProject.NonProcessedPage_cs);
            fileSystem.File.WriteAllBytes(Path.Combine(layoutDirectory, @".nuget", "NuGet.config"), Properties.RazorCsProject.NuGet_Config);
            fileSystem.File.WriteAllBytes(Path.Combine(layoutDirectory, @".nuget", "NuGet.exe"), Properties.RazorCsProject.NuGet_exe);
            fileSystem.File.WriteAllBytes(Path.Combine(layoutDirectory, @".nuget", "NuGet.targets"), Properties.RazorCsProject.NuGet_targets);
            fileSystem.File.WriteAllBytes(Path.Combine(layoutDirectory, @"PretzelClasses", "PageContext.cs"), Properties.RazorCsProject.PageContext_cs);
            fileSystem.File.WriteAllBytes(Path.Combine(layoutDirectory, @"PretzelClasses", "Page.cs"), Properties.RazorCsProject.Page_cs);
            fileSystem.File.WriteAllBytes(Path.Combine(layoutDirectory, @"PretzelClasses", "Paginator.cs"), Properties.RazorCsProject.Paginator_cs);
            fileSystem.File.WriteAllBytes(Path.Combine(layoutDirectory, @"PretzelClasses", "SiteContext.cs"), Properties.RazorCsProject.SiteContext_cs);
            fileSystem.File.WriteAllBytes(Path.Combine(layoutDirectory, @"PretzelClasses", "Tag.cs"), Properties.RazorCsProject.Tag_cs);
            fileSystem.File.WriteAllBytes(Path.Combine(layoutDirectory, @"web.config"), Properties.RazorCsProject.Web_config);
            fileSystem.File.WriteAllBytes(Path.Combine(layoutDirectory, @"packages.config"),
                                          Properties.RazorCsProject.packages_config);
        }

        private void CreateImages()
        {
            CreateImage(@"Resources\25.png", directory, @"img", "25.png");
            CreateImage(@"Resources\favicon.png", directory, @"img", "favicon.png");
            CreateImage(@"Resources\logo.png", directory, @"img", "logo.png");

            CreateFavicon();
        }

        private void CreateFavicon()
        {
            CreateImage(@"Resources\favicon.ico", directory, @"img", "favicon.ico");
        }

        private void CreateImage(string resourceName, params string[] pathSegments)
        {
            using (var ms = new MemoryStream())
            using (var resourceStream = GetResourceStream(resourceName))
            {
                resourceStream.CopyTo(ms);
                fileSystem.File.WriteAllBytes(Path.Combine(pathSegments), ms.ToArray());
            }
        }

        //https://github.com/dotnet/corefx/issues/12565
        private Stream GetResourceStream(string path)
        {
            var assembly = GetType().Assembly;
            var name = GetType().Assembly.GetName().Name;

            path = path.Replace("/", ".").Replace("\\", ".");

            var fullPath = $"{name}.{path}";
            var stream = assembly.GetManifestResourceStream(fullPath);

            return stream;
        }

        private void CreateDirectories()
        {
            fileSystem.Directory.CreateDirectory(Path.Combine(directory, @"_posts"));
            fileSystem.Directory.CreateDirectory(Path.Combine(directory, @"_layouts"));
            fileSystem.Directory.CreateDirectory(Path.Combine(directory, @"_includes"));
            fileSystem.Directory.CreateDirectory(Path.Combine(directory, @"css"));
            fileSystem.Directory.CreateDirectory(Path.Combine(directory, @"img"));
            if (withDrafts)
            {
                fileSystem.Directory.CreateDirectory(Path.Combine(directory, @"_drafts"));
            }
        }
    }
}
