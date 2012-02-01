using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Pretzel.Logic
{
    public class Recipe
    {
        public Recipe(string engine, string directory)
        {
            _engine = engine;
            _directory = directory;
        }

        private readonly string _engine;
        private readonly string _directory;

        public string Create()
        {
            try
            {
                if (!Directory.Exists(_directory))
                    Directory.CreateDirectory(_directory);

                if (string.Equals("Razor", _engine, StringComparison.InvariantCultureIgnoreCase))
                {
                    return "Razor templating hasn't been implemented yet";
                }

                if (string.Equals("Liquid", _engine, StringComparison.InvariantCultureIgnoreCase))
                {
                    Directory.CreateDirectory(Path.Combine(_directory, @"_posts"));
                    Directory.CreateDirectory(Path.Combine(_directory, @"_layouts"));
                    Directory.CreateDirectory(Path.Combine(_directory, @"css"));
                    Directory.CreateDirectory(Path.Combine(_directory, @"img"));
                    
                    using (StreamWriter fs = new StreamWriter(Path.Combine(_directory, @"rss.xml"), false))
                    {
                        fs.Write(Properties.Resources.Rss);
                    }

                    using (StreamWriter fs = new StreamWriter(Path.Combine(_directory, @"atom.xml"), false))
                    {
                        fs.Write(Properties.Resources.Atom);
                    }

                    using (StreamWriter fs = new StreamWriter(Path.Combine(_directory, @"_layouts\layout.html"), false))
                    {
                        fs.Write(Properties.Resources.Layout);
                    }

                    using (StreamWriter fs = new StreamWriter(Path.Combine(_directory, @"_layouts\post.html"), false))
                    {
                        fs.Write(Properties.Resources.Post);
                    }

                    Properties.Resources._25.Save(Path.Combine(_directory, @"img\25.png"));
                    Properties.Resources.faviconpng.Save(Path.Combine(_directory, @"img\favicon.png"));
                    Properties.Resources.logo.Save(Path.Combine(_directory, @"img\logo.png"));

                    using (StreamWriter fs = new StreamWriter(Path.Combine(_directory, @"img\favicon.ico")))
                    {
                        Properties.Resources.faviconico.Save(fs.BaseStream);
                    }

                    using (StreamWriter fs = new StreamWriter(Path.Combine(_directory, @"index.md"), false))
                    {
                        fs.Write(Properties.Resources.Index);
                    }

                    using (StreamWriter fs = new StreamWriter(Path.Combine(_directory, @"about.md"), false))
                    {
                        fs.Write(Properties.Resources.About);
                    }
                
                    using (StreamWriter fs = new StreamWriter(Path.Combine(_directory, string.Format(@"_posts\{0}-myfirstpost.md", DateTime.Today.ToString("yyyy-MM-dd"))), false))
                    {
                        fs.Write(Properties.Resources.FirstPost);
                    }

                    using (StreamWriter fs = new StreamWriter(Path.Combine(_directory, string.Format(@"css\style.css")), false))
                    {
                        fs.Write(Properties.Resources.Style);
                    }
                }

                return "Pretzel site template has been created";
            }
            catch (Exception ex)
            {
                return string.Format("Error trying to create template: {0}", ex.Message);
            }
        }
    }
}
