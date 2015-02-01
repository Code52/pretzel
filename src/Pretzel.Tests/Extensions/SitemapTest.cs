using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating.Context;
using Pretzel.Logic.Templating.Jekyll;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace Pretzel.Tests.Extensions
{
    public class SitemapTest
    {
        [Fact]
        public void CompressSitemap_compress_existing_sitemap()
        {
            // arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> { { @"C:\website\_site\sitemap.xml", MockFileData.NullObject } });
            var siteContext = new SiteContext { OutputFolder = @"C:\website\_site" };
            
            // act
            new LiquidEngine().CompressSitemap(siteContext, fileSystem);

            // assert
            Assert.True(fileSystem.File.Exists(@"C:\website\_site\sitemap.xml.gz"));
        }

        [Fact]
        public void CompressSitemap_without_existing_sitemap_do_nothing()
        {
            // arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> { });
            var siteContext = new SiteContext { OutputFolder = @"C:\website\_site" };

            // act
            new LiquidEngine().CompressSitemap(siteContext, fileSystem);

            // assert
            Assert.False(fileSystem.File.Exists(@"C:\website\_site\sitemap.xml.gz"));
        }
    }
}
