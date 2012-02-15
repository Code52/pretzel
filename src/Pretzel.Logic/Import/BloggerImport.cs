using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Abstractions;
using System.Xml.Linq;
using Pretzel.Logic.Extensions;
using System.IO;

namespace Pretzel.Logic.Import
{
    public class BloggerImport
    {
        private readonly IFileSystem fileSystem;
        private readonly string pathToSite;
        private readonly string pathToImportFile;

        public BloggerImport(IFileSystem fileSystem, string pathToSite, string pathToImportFile)
        {
            this.fileSystem = fileSystem;
            this.pathToSite = pathToSite;
            this.pathToImportFile = pathToImportFile;
        }

        public void Import()
        {
            var xml = fileSystem.File.ReadAllText(pathToImportFile);
            var root = XElement.Parse(xml);

            // key bits of the atom xml format:
            // <feed>
            //    <category /> - can be many
            //    <title /> = title of blog
            //    <author><name /></author>
            //    <entry>
            //       <id/>
            //       <category scheme='http://schemas.google.com/g/2005#kind' term='http://schemas.google.com/blogger/2008/kind#post'/>
            //       <category scheme='http://www.blogger.com/atom/ns#' term='A Category'/>
            //       <published/> formatted like this: 2007-02-01T14:01:23.326Z
            //       <updated/>
            //       <title/>
            //       <content type='html'/>
            //       <author><name /></author>
            //    <entry>

            // <category scheme='http://schemas.google.com/g/2005#kind' term='http://schemas.google.com/blogger/2008/kind#comment'/><title type='text'>Hi Mark, I use your excellent  library on my open ...</title><content type='html'>Hi Mark, I use your excellent  library on my open source project ispy - &lt;br /&gt;&lt;br /&gt;https://sourceforge.net/projects/ispysoftware/&lt;br /&gt;&lt;br /&gt;any chance of adding a link to my website from your project page?&lt;br /&gt;&lt;br /&gt;http://www.ispyconnect.com&lt;br /&gt;&lt;br /&gt;Thanks and great work!&lt;br /&gt;&lt;br /&gt;I went to Southampton Uni at the same time as you by the way - studied Aero/Astro Eng.&lt;br /&gt;&lt;br /&gt;Sean</content>
            XNamespace atom = "http://www.w3.org/2005/Atom";
            var count = root.Descendants(atom + "entry").Count();

            var posts = from e in root.Descendants(atom + "entry")
                        where e.Elements(atom + "category").Where(x => x.Attribute("term").Value == "http://schemas.google.com/blogger/2008/kind#post").Count() > 0
                        select new BloggerPost
                        {
                            Title = e.Element(atom + "title").Value,
                            //PostName = e.Element(wp + "post_name").Value,
                            Published = Convert.ToDateTime(e.Element(atom + "published").Value),
                            Updated = Convert.ToDateTime(e.Element(atom + "updated").Value),
                            Content = e.Element(atom + "content").Value,
                            /*Tags = from t in e.Elements(atom + "category")
                                   where t.Attribute("domain").Value == "post_tag"
                                   select t.Value,*/
                            // blogger categories are more like
                            Tags = from t in e.Elements(atom + "category")
                                         where t.Attribute("scheme").Value == "http://www.blogger.com/atom/ns#"
                                         select t.Value
                        };

            foreach (var p in posts)
            {
                ImportPost(p);
            }
        }

        private void ImportPost(BloggerPost p)
        {
            var header = new
            {
                title = p.Title,
                date = p.Published,
                layout = "post",
                categories = p.Categories,
                tags = p.Tags
            };

            var yamlHeader = string.Format("---\r\n{0}---\r\n\r\n", header.ToYaml());
            var postContent = yamlHeader + p.Content; //todo would be nice to convert to proper md
            var slug = p.Title.Replace(' ', '-');
            slug = p.Title.Replace("\"", "");
            var fileName = string.Format(@"_posts\{0}-{1}.md", p.Published.ToString("yyyy-MM-dd"), slug); //not sure about post name

            fileSystem.File.WriteAllText(Path.Combine(pathToSite, fileName), postContent);
        }


        protected class BloggerPost
        {
            public string Title { get; set; }
            public DateTime Updated { get; set; }
            public DateTime Published { get; set; }
            public string Content { get; set; }
            public IEnumerable<string> Tags { get; set; }
            public IEnumerable<string> Categories { get; set; }
        }
    }
}
