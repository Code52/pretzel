using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.IO.Abstractions.TestingHelpers;
using Pretzel.Logic.Import;

namespace Pretzel.Tests.Import
{
    public class WordpressImportTests
    {
        const string BaseSite = @"c:\site\";
        const string ImportFile = @"c:\import.xml";
        const string ImportContent = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
<rss version=""2.0""
	xmlns:excerpt=""http://wordpress.org/export/1.1/excerpt/""
	xmlns:content=""http://purl.org/rss/1.0/modules/content/""
	xmlns:wfw=""http://wellformedweb.org/CommentAPI/""
	xmlns:dc=""http://purl.org/dc/elements/1.1/""
	xmlns:wp=""http://wordpress.org/export/1.1/""
>

<channel>
	<title>test blog</title>
	<link>http://lobsterino.wordpress.com</link>
	<description>Just another WordPress.com weblog</description>
	<pubDate>Sun, 05 Feb 2012 01:45:31 +0000</pubDate>
	<language>en</language>
	<wp:wxr_version>1.1</wp:wxr_version>
	<wp:base_site_url>http://wordpress.com/</wp:base_site_url>
	<wp:base_blog_url>http://lobsterino.wordpress.com</wp:base_blog_url>

	<wp:author><wp:author_id>7397984</wp:author_id><wp:author_login>lobsterino</wp:author_login><wp:author_email>lukenlowrey@gmail.com</wp:author_email><wp:author_display_name><![CDATA[lobsterino]]></wp:author_display_name><wp:author_first_name><![CDATA[]]></wp:author_first_name><wp:author_last_name><![CDATA[]]></wp:author_last_name></wp:author>


	<generator>http://wordpress.com/</generator>
<cloud domain='lobsterino.wordpress.com' port='80' path='/?rsscloud=notify' registerProcedure='' protocol='http-post' />
<image>
		<url>https://s-ssl.wordpress.com/i/buttonw-com.png</url>
		<title>test blog</title>
		<link>http://lobsterino.wordpress.com</link>
	</image>
	<atom:link rel=""search"" type=""application/opensearchdescription+xml"" href=""http://lobsterino.wordpress.com/osd.xml"" title=""test blog"" />
	<atom:link rel='hub' href='http://lobsterino.wordpress.com/?pushpress=hub'/>

	<item>
		<title>Hello world!</title>
		<link>http://lobsterino.wordpress.com/2010/02/06/hello-world/</link>
		<pubDate>Sat, 06 Feb 2010 11:22:38 +0000</pubDate>
		<dc:creator>lobsterino</dc:creator>
		<guid isPermaLink=""false""></guid>
		<description></description>
		<content:encoded><![CDATA[Welcome to <a href=""http://wordpress.com/"">WordPress.com</a>. This is your first post. Edit or delete it and start blogging!]]></content:encoded>
		<excerpt:encoded><![CDATA[]]></excerpt:encoded>
		<wp:post_id>1</wp:post_id>
		<wp:post_date>2010-02-06 11:22:38</wp:post_date>
		<wp:post_date_gmt>2010-02-06 11:22:38</wp:post_date_gmt>
		<wp:comment_status>open</wp:comment_status>
		<wp:ping_status>open</wp:ping_status>
		<wp:post_name>hello-world</wp:post_name>
		<wp:status>publish</wp:status>
		<wp:post_parent>0</wp:post_parent>
		<wp:menu_order>0</wp:menu_order>
		<wp:post_type>post</wp:post_type>
		<wp:post_password></wp:post_password>
		<wp:is_sticky>0</wp:is_sticky>
		<category domain=""category"" nicename=""uncategorized""><![CDATA[Uncategorized]]></category>
		<category domain=""post_tag"" nicename=""tag""><![CDATA[tag]]></category>
		<wp:postmeta>
			<wp:meta_key>_edit_last</wp:meta_key>
			<wp:meta_value><![CDATA[7397984]]></wp:meta_value>
		</wp:postmeta>
		<wp:comment>
			<wp:comment_id>1</wp:comment_id>
			<wp:comment_author><![CDATA[Mr WordPress]]></wp:comment_author>
			<wp:comment_author_email></wp:comment_author_email>
			<wp:comment_author_url>http://WordPress.com/</wp:comment_author_url>
			<wp:comment_author_IP>127.0.0.1</wp:comment_author_IP>
			<wp:comment_date>2010-02-06 11:22:38</wp:comment_date>
			<wp:comment_date_gmt>2010-02-06 11:22:38</wp:comment_date_gmt>
			<wp:comment_content><![CDATA[Hi, this is a comment.<br />To delete a comment, just log in, and view the posts' comments, there you will have the option to edit or delete them.]]></wp:comment_content>
			<wp:comment_approved>1</wp:comment_approved>
			<wp:comment_type></wp:comment_type>
			<wp:comment_parent>0</wp:comment_parent>
			<wp:comment_user_id>0</wp:comment_user_id>
		</wp:comment>
	</item>
</channel>
</rss>
";

        public WordpressImportTests()
        {

        }

        [Fact]
        public void Posts_Are_Imported()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { ImportFile, new MockFileData(ImportContent) }
            });

            var wordpressImporter = new WordpressImport(fileSystem, BaseSite, ImportFile);
            wordpressImporter.Import();


        }
    }
}
