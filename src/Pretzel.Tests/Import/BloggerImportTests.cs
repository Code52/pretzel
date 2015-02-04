using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit;
using System.IO.Abstractions.TestingHelpers;
using Pretzel.Logic.Import;
using Pretzel.Logic.Extensions;
using System.Text;
using System.IO;
using NSubstitute;
using System.IO.Abstractions;
using System.Threading;

namespace Pretzel.Tests.Import
{
    public class BloggerImportTests
    {
        const string BaseSite = @"c:\site\";
        const string ImportFile = @"c:\import.xml";
        // test data based on feed from helloworld.blogspot.com
        const string ImportContent = @"<?xml version='1.0' encoding='UTF-8'?>
<?xml-stylesheet href=""http://www.blogger.com/styles/atom.css"" type=""text/css""?>
<feed xmlns='http://www.w3.org/2005/Atom' xmlns:openSearch='http://a9.com/-/spec/opensearchrss/1.0/' xmlns:georss='http://www.georss.org/georss' xmlns:gd='http://schemas.google.com/g/2005' xmlns:thr='http://purl.org/syndication/thread/1.0'>
    <id>tag:blogger.com,1999:blog-786740</id>
    <updated>2012-02-06T23:20:56.200+08:00</updated>
    <title type='text'>Hello, world</title>
    <subtitle type='html'>Testing blogger</subtitle>
    <link rel='http://schemas.google.com/g/2005#feed' type='application/atom+xml' href='http://helloworld.blogspot.com/feeds/posts/default'/>
    <link rel='self' type='application/atom+xml' href='http://www.blogger.com/feeds/786740/posts/default'/>
    <link rel='alternate' type='text/html' href='http://helloworld.blogspot.com/'/>
    <author><name>Trevor</name><uri>http://www.blogger.com/profile/11478756950805515790</uri><email>noreply@blogger.com</email>
    <gd:image rel='http://schemas.google.com/g/2005#thumbnail' width='16' height='16' src='http://img2.blogblog.com/img/b16-rounded.gif'/></author>
    <generator version='7.00' uri='http://www.blogger.com'>Blogger</generator>
    <openSearch:totalResults>2</openSearch:totalResults><openSearch:startIndex>1</openSearch:startIndex>
    <openSearch:itemsPerPage>25</openSearch:itemsPerPage>
    <entry>
        <id>tag:blogger.com,1999:blog-786740.post-786756</id>
        <category scheme='http://schemas.google.com/g/2005#kind' term='http://schemas.google.com/blogger/2008/kind#post'/>
        <published>2000-09-07T13:25:00.000+08:00</published>
        <updated>2000-09-07T13:25:05.590+08:00</updated>
        <title type='text'>Hello World 1</title>
        <content type='html'>hello again&lt;div class=""blogger-post-footer""&gt;&lt;img width='1' height='1' src='https://blogger.googleusercontent.com/tracker/786740-786756?l=helloworld.blogspot.com' alt='' /&gt;&lt;/div&gt;</content>
        <link rel='edit' type='application/atom+xml' href='http://www.blogger.com/feeds/786740/posts/default/786756'/>
        <link rel='self' type='application/atom+xml' href='http://www.blogger.com/feeds/786740/posts/default/786756'/>
        <link rel='alternate' type='text/html' href='http://helloworld.blogspot.com/2000_09_03_archive.html#786756' title=''/>
        <author>
            <name>Trevor</name>
            <uri>http://www.blogger.com/profile/11478756950805515790</uri>
            <email>noreply@blogger.com</email>
            <gd:image rel='http://schemas.google.com/g/2005#thumbnail' width='16' height='16' src='http://img2.blogblog.com/img/b16-rounded.gif'/>
        </author>
    </entry>
    <entry>
        <id>tag:blogger.com,1999:blog-786740.post-786751</id>
        <category scheme='http://schemas.google.com/g/2005#kind' term='http://schemas.google.com/blogger/2008/kind#post'/>
        <category scheme='http://www.blogger.com/atom/ns#' term='aTag'/>
        <published>2000-09-07T13:24:00.000+08:00</published>
        <updated>2000-09-07T13:24:23.890+08:00</updated>
        <title type='text'>Hello World 2</title>
        <content type='html'>hello world&lt;div class=""blogger-post-footer""&gt;&lt;img width='1' height='1' src='https://blogger.googleusercontent.com/tracker/786740-786751?l=helloworld.blogspot.com' alt='' /&gt;&lt;/div&gt;</content>
        <link rel='edit' type='application/atom+xml' href='http://www.blogger.com/feeds/786740/posts/default/786751'/>
        <link rel='self' type='application/atom+xml' href='http://www.blogger.com/feeds/786740/posts/default/786751'/>
        <link rel='alternate' type='text/html' href='http://helloworld.blogspot.com/2000_09_03_archive.html#786751' title=''/>
        <author>
            <name>Trevor</name>
            <uri>http://www.blogger.com/profile/11478756950805515790</uri>
            <email>noreply@blogger.com</email>
            <gd:image rel='http://schemas.google.com/g/2005#thumbnail' width='16' height='16' src='http://img2.blogblog.com/img/b16-rounded.gif'/>
        </author>
    </entry>
</feed>";

        public BloggerImportTests()
        {
            //ImportContent = System.IO.File.ReadAllText(@"path-to-test-data.xml");
        }

        [Fact]
        public void Posts_Are_Imported()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { ImportFile, new MockFileData(ImportContent) }
            });

            var bloggerImporter = new BloggerImport(fileSystem, BaseSite, ImportFile);
            bloggerImporter.Import();

            string expectedPost = @"_posts\2000-09-07-Hello-World-1.md";
            Assert.True(fileSystem.File.Exists(BaseSite + expectedPost));

            var postContent = fileSystem.File.ReadAllText(BaseSite + expectedPost);
            var header = postContent.YamlHeader();

            Assert.Equal("Hello World 1", header["title"].ToString());
            Assert.Equal(0, ((List<string>)header["tags"]).Count);

            string expectedPost2 = @"_posts\2000-09-07-Hello-World-2.md";
            Assert.True(fileSystem.File.Exists(BaseSite + expectedPost2));

            var postContent2 = fileSystem.File.ReadAllText(BaseSite + expectedPost2);
            var header2 = postContent2.YamlHeader();

            Assert.Equal("Hello World 2", header2["title"].ToString());
            var tags = (List<string>)header2["tags"];
            Assert.Equal(1, tags.Count);
            Assert.Equal("aTag", tags[0]);
        }

        [Fact]
        public void Error_on_write_is_traced()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            // arrange
            StringBuilder sb = new StringBuilder();
            TextWriter writer = new StringWriter(sb);
            Tracing.Logger.SetWriter(writer);
            Tracing.Logger.AddCategory("info");
            Tracing.Logger.AddCategory("debug");

            var fileSubstitute = Substitute.For<FileBase>();
            fileSubstitute.ReadAllText(ImportFile).Returns(ImportContent);
            fileSubstitute.When(f => f.WriteAllText(Arg.Any<string>(), Arg.Any<string>())).Do(x => { throw new Exception(); });

            var fileSystemSubstitute = Substitute.For<IFileSystem>();
            fileSystemSubstitute.File.Returns(fileSubstitute);

            // act
            var bloggerImporter = new BloggerImport(fileSystemSubstitute, BaseSite, ImportFile);
            bloggerImporter.Import();

            // assert
            Assert.Contains(@"Failed to write out 2000-09-07-Hello-World-1.md", sb.ToString());
            Assert.Contains("Exception of type 'System.Exception' was thrown.", sb.ToString());
            Assert.Contains(@"Failed to write out 2000-09-07-Hello-World-2.md", sb.ToString());
        }
    }
}
