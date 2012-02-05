using System.Collections.Generic;

namespace Pretzel.Logic.Templating.Context
{
    public class Tag
    {
        public IEnumerable<Post> Posts { get; set; }
        public string Name { get; set; }
    }
}