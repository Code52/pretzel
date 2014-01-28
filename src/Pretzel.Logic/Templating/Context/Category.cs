using System.Collections.Generic;
using DotLiquid;

namespace Pretzel.Logic.Templating.Context
{
    public class Category : Drop
    {
        public IEnumerable<Page> Posts { get; set; }
        public string Name { get; set; }
    }
}