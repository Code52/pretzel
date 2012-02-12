using DotLiquid;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Logic.Templating.Jekyll.Extensions
{
    public static class PageExtensions
    {
        public static Hash ToHash(this Page page)
        {
            var p = Hash.FromDictionary(page.Bag);
            p.Add("Content", page.Content);
            return p;
        }
    }
}
