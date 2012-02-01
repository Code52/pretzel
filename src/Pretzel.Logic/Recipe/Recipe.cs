using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;

namespace Pretzel.Logic
{
    public class Recipe
    {
        public Recipe(IFileSystem fileSystem, string engine, string directory)
        {
            _fileSystem = fileSystem;
            _engine = engine;
            _directory = directory;
        }

        private readonly IFileSystem _fileSystem;
        private readonly string _engine;
        private readonly string _directory;

        public string Create()
        {
            try
            {
                if (!_fileSystem.Directory.Exists(_directory))
                    _fileSystem.Directory.CreateDirectory(_directory);

                if (string.Equals("Razor", _engine, StringComparison.InvariantCultureIgnoreCase))
                {
                    return "Razor templating hasn't been implemented yet";
                }

                if (string.Equals("Liquid", _engine, StringComparison.InvariantCultureIgnoreCase))
                {
                    _fileSystem.Directory.CreateDirectory(Path.Combine(_directory, @"_posts"));
                    _fileSystem.Directory.CreateDirectory(Path.Combine(_directory, @"_layouts"));
                    _fileSystem.Directory.CreateDirectory(Path.Combine(_directory, @"css"));
                    _fileSystem.Directory.CreateDirectory(Path.Combine(_directory, @"img"));
                    
                    _fileSystem.File.WriteAllText(Path.Combine(_directory, @"rss.xml"), Properties.Resources.Rss);
                    _fileSystem.File.WriteAllText(Path.Combine(_directory, @"atom.xml"), Properties.Resources.Atom);
                    _fileSystem.File.WriteAllText(Path.Combine(_directory, @"_layouts\layout.html"), Properties.Resources.Layout);
                    _fileSystem.File.WriteAllText(Path.Combine(_directory, @"_layouts\post.html"), Properties.Resources.Post);
                    _fileSystem.File.WriteAllText(Path.Combine(_directory, @"index.md"), Properties.Resources.Index);
                    _fileSystem.File.WriteAllText(Path.Combine(_directory, @"about.md"), Properties.Resources.About);
                    _fileSystem.File.WriteAllText(Path.Combine(_directory, string.Format(@"_posts\{0}-myfirstpost.md", DateTime.Today.ToString("yyyy-MM-dd"))), Properties.Resources.FirstPost);
                    _fileSystem.File.WriteAllText(Path.Combine(_directory, @"css\style.css"), Properties.Resources.Style);

                    var ms = new MemoryStream();
                    Properties.Resources._25.Save(ms, ImageFormat.Png);
                    _fileSystem.File.WriteAllBytes(Path.Combine(_directory, @"img\25.png"), ms.ToArray());

                    ms = new MemoryStream();
                    Properties.Resources.faviconpng.Save(ms, ImageFormat.Png);
                    _fileSystem.File.WriteAllBytes(Path.Combine(_directory, @"img\favicon.png"), ms.ToArray());

                    ms = new MemoryStream();
                    Properties.Resources.logo.Save(ms, ImageFormat.Png);
                    _fileSystem.File.WriteAllBytes(Path.Combine(_directory, @"img\logo.png"), ms.ToArray());

                    ms = new MemoryStream();
                    Properties.Resources.faviconico.Save(ms);
                    _fileSystem.File.WriteAllBytes(Path.Combine(_directory, @"img\favicon.ico"), ms.ToArray());
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
