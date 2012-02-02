using System;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Abstractions;

namespace Pretzel.Logic.Recipe
{
    public class Recipe
    {
        public Recipe(IFileSystem fileSystem, string engine, string directory)
        {
            this.fileSystem = fileSystem;
            this.engine = engine;
            this.directory = directory;
        }

        private readonly IFileSystem fileSystem;
        private readonly string engine;
        private readonly string directory;

        public string Create()
        {
            try
            {
                if (!fileSystem.Directory.Exists(directory))
                    fileSystem.Directory.CreateDirectory(directory);

                if (string.Equals("Razor", engine, StringComparison.InvariantCultureIgnoreCase))
                {
                    return "Razor templating hasn't been implemented yet";
                }

                if (string.Equals("Liquid", engine, StringComparison.InvariantCultureIgnoreCase))
                {
                    fileSystem.Directory.CreateDirectory(Path.Combine(directory, @"_posts"));
                    fileSystem.Directory.CreateDirectory(Path.Combine(directory, @"_layouts"));
                    fileSystem.Directory.CreateDirectory(Path.Combine(directory, @"css"));
                    fileSystem.Directory.CreateDirectory(Path.Combine(directory, @"img"));
                    
                    fileSystem.File.WriteAllText(Path.Combine(directory, @"rss.xml"), Properties.Resources.Rss);
                    fileSystem.File.WriteAllText(Path.Combine(directory, @"atom.xml"), Properties.Resources.Atom);
                    fileSystem.File.WriteAllText(Path.Combine(directory, @"_layouts\layout.html"), Properties.Resources.Layout);
                    fileSystem.File.WriteAllText(Path.Combine(directory, @"_layouts\post.html"), Properties.Resources.Post);
                    fileSystem.File.WriteAllText(Path.Combine(directory, @"index.md"), Properties.Resources.Index);
                    fileSystem.File.WriteAllText(Path.Combine(directory, @"about.md"), Properties.Resources.About);
                    fileSystem.File.WriteAllText(Path.Combine(directory, string.Format(@"_posts\{0}-myfirstpost.md", DateTime.Today.ToString("yyyy-MM-dd"))), Properties.Resources.FirstPost);
                    fileSystem.File.WriteAllText(Path.Combine(directory, @"css\style.css"), Properties.Resources.Style);

                    var ms = new MemoryStream();
                    Properties.Resources._25.Save(ms, ImageFormat.Png);
                    fileSystem.File.WriteAllBytes(Path.Combine(directory, @"img\25.png"), ms.ToArray());

                    ms = new MemoryStream();
                    Properties.Resources.faviconpng.Save(ms, ImageFormat.Png);
                    fileSystem.File.WriteAllBytes(Path.Combine(directory, @"img\favicon.png"), ms.ToArray());

                    ms = new MemoryStream();
                    Properties.Resources.logo.Save(ms, ImageFormat.Png);
                    fileSystem.File.WriteAllBytes(Path.Combine(directory, @"img\logo.png"), ms.ToArray());

                    ms = new MemoryStream();
                    Properties.Resources.faviconico.Save(ms);
                    fileSystem.File.WriteAllBytes(Path.Combine(directory, @"img\favicon.ico"), ms.ToArray());
                }
                else
                    return "Templating Engine not found";

                return "Pretzel site template has been created";
            }
            catch (Exception ex)
            {
                return string.Format("Error trying to create template: {0}", ex.Message);
            }
        }
    }
}
