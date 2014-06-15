using Pretzel.Logic.Templating;
using Pretzel.Logic.Templating.Context;
using System.IO;

namespace Pretzel.Logic.Extensions
{
    public static class Sitemap
    {
        public static void CompressSitemap(this ISiteEngine engine, SiteContext siteContext) 
        {
            var sitemap = Path.Combine(siteContext.OutputFolder, @"sitemap.xml");
            var compressedSitemap = sitemap + ".gz";

            if (File.Exists(sitemap))
            {
                using (var sitemapStream = File.OpenRead(sitemap))
                {
                    using (var compressedMap = File.Create(compressedSitemap))
                    {
                        using (var gzip = new System.IO.Compression.GZipStream(compressedMap, System.IO.Compression.CompressionMode.Compress))
                        {
                            sitemapStream.CopyTo(gzip);
                        }
                    }
                }
            }
        }
    }
}
