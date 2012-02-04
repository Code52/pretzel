using System.Collections.Generic;

namespace Pretzel.Logic.Templating.Jekyll
{
    public class Tag
    {
        public IEnumerable<Post> Posts { get; set; }
        public string Name { get; set; }
    }

    public class Post
    {
    }

    public class SiteContext
    {
        public string Folder { get; set; }
        public string Title { get; set; }
        public IEnumerable<Tag> Tags { get; set; }
    }
}