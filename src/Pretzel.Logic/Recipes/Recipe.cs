using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
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
                        CreateFile(@"Resources\RazorWiki\layout.cshtml", directory, @"_layouts", "layout.cshtml");
                        CreateFile(@"Resources\RazorWiki\index.md", directory, "index.md");
                        CreateFile(@"Resources\RazorWiki\style.css", directory, @"css", "style.css");
                        CreateFile(@"Resources\RazorWiki\default.css", directory, @"css", "default.css");
                        CreateFile(@"Resources\Razor\Config.cshtml", directory, "_config.yml");
                        CreateFavicon();
                    }
                    else
                    {
                        CreateFile(@"Resources\Razor\Rss.cshtml", directory, @"rss.xml");
                        CreateFile(@"Resources\Razor\Atom.cshtml", directory, @"atom.xml");
                        CreateFile(@"Resources\Razor\Sitemap.cshtml", directory, @"sitemap.xml");
                        CreateFile(@"Resources\Razor\Layout.cshtml", directory, @"_layouts", "layout.cshtml");
                        CreateFile(@"Resources\Razor\Post.cshtml", directory, @"_layouts", "post.cshtml");
                        CreateFile(@"Resources\Razor\Index.cshtml", directory, "index.cshtml");
                        CreateFile(@"Resources\Razor\About.cshtml", directory, "about.md");
                        CreateFile(@"Resources\Razor\FirstPost.cshtml", directory, @"_posts", $"{DateTime.Today.ToString("yyyy-MM-dd")}-myfirstpost.md");
                        CreateFile(@"Resources\Style.css", directory, @"css", "style.css");
                        CreateFile(@"Resources\Razor\Config.cshtml", directory, "_config.yml");
                        CreateFile(@"Resources\Razor\Head.cshtml", directory, @"_includes", "head.cshtml");
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

                    CreateFile(@"Resources\Liquid\Rss.liquid", directory, @"rss.xml");
                    CreateFile(@"Resources\Liquid\Atom.liquid", directory, @"atom.xml");
                    CreateFile(@"Resources\Liquid\Sitemap.liquid", directory, @"sitemap.xml");
                    CreateFile(@"Resources\Liquid\Layout.liquid", directory, @"_layouts", "layout.html");
                    CreateFile(@"Resources\Liquid\Post.liquid", directory, @"_layouts", "post.html");
                    CreateFile(@"Resources\Liquid\Index.liquid", directory, @"index.html");
                    CreateFile(@"Resources\Liquid\About.liquid", directory, @"about.md");
                    CreateFile(@"Resources\Liquid\FirstPost.liquid", directory, @"_posts", $"{DateTime.Today.ToString("yyyy-MM-dd")}-myfirstpost.md");
                    CreateFile(@"Resources\Style.css", directory, @"css", "style.css");
                    CreateFile(@"Resources\Liquid\Config.liquid", directory, @"_config.yml");
                    CreateFile(@"Resources\Liquid\Head.liquid", directory, @"_includes", "head.html");

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

            CreateFile(@"Resources\RazorCsProject\AssemblyInfo_cs", layoutDirectory, @"Properties", "AssemblyInfo.cs");
            CreateFile(@"Resources\RazorCsProject\Category_cs", layoutDirectory, @"PretzelClasses", "Category.cs");
            CreateFile(@"Resources\RazorCsProject\LayoutProject_csproj", layoutDirectory, "LayoutProject.csproj");
            CreateFile(@"Resources\RazorCsProject\LayoutSolution_sln", layoutDirectory, "layoutSolution.sln");
            CreateFile(@"Resources\RazorCsProject\NonProcessedPage_cs", layoutDirectory, @"PretzelClasses", "NonProcessedPage.cs");
            CreateFile(@"Resources\RazorCsProject\NuGet_Config", layoutDirectory, @".nuget", "NuGet.config");
            CreateFile(@"Resources\RazorCsProject\NuGet_exe", layoutDirectory, @".nuget", "NuGet.exe");
            CreateFile(@"Resources\RazorCsProject\NuGet_targets", layoutDirectory, @".nuget", "NuGet.targets");
            CreateFile(@"Resources\RazorCsProject\PageContext_cs", layoutDirectory, @"PretzelClasses", "PageContext.cs");
            CreateFile(@"Resources\RazorCsProject\Page_cs", layoutDirectory, @"PretzelClasses", "Page.cs");
            CreateFile(@"Resources\RazorCsProject\Paginator_cs", layoutDirectory, @"PretzelClasses", "Paginator.cs");
            CreateFile(@"Resources\RazorCsProject\SiteContext_cs", layoutDirectory, @"PretzelClasses", "SiteContext.cs");
            CreateFile(@"Resources\RazorCsProject\Tag_cs", layoutDirectory, @"PretzelClasses", "Tag.cs");
            CreateFile(@"Resources\RazorCsProject\Web_config", layoutDirectory, "web.config");
            CreateFile(@"Resources\RazorCsProject\packages_config", layoutDirectory, "packages.config");
        }

        private void CreateImages()
        {
            CreateFile(@"Resources\25.png", directory, @"img", "25.png");
            CreateFile(@"Resources\favicon.png", directory, @"img", "favicon.png");
            CreateFile(@"Resources\logo.png", directory, @"img", "logo.png");

            CreateFavicon();
        }

        private void CreateFavicon()
        {
            CreateFile(@"Resources\favicon.ico", directory, @"img", "favicon.ico");
        }

        private void CreateFile(string resourceName, params string[] pathSegments)
        {
            using (var ms = new MemoryStream())
            using (var resourceStream = GetResourceStream(resourceName))
            {
                resourceStream.CopyTo(ms);
                fileSystem.File.WriteAllBytes(Path.Combine(pathSegments), ms.ToArray());
            }
        }

        internal static void CreateFile(Type type, IFileSystem fileSystem, string resourceName, params string[] pathSegments)
        {
            using (var ms = new MemoryStream())
            using (var resourceStream = GetResourceStream(type, resourceName))
            {
                resourceStream.CopyTo(ms);
                fileSystem.File.WriteAllBytes(Path.Combine(pathSegments), ms.ToArray());
            }
        }

        //https://github.com/dotnet/corefx/issues/12565
        private static Stream GetResourceStream(Type type, string path)
        {
            var assembly = type.Assembly;
            var name = type.Assembly.GetName().Name;

            path = path.Replace("/", ".").Replace("\\", ".");

            var fullPath = $"{name}.{path}";
            var stream = assembly.GetManifestResourceStream(fullPath);

            return stream;
        }

        private Stream GetResourceStream(string path)
            => GetResourceStream(GetType(), path);

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
