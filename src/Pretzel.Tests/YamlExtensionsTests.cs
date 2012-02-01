using Pretzel.Logic.Extensions;
using Xunit;

namespace Pretzel.Tests
{
    public class YamlExtensionsTests
    {
        public class YamlHeaderTests
        {
            [Fact]
            public void Parse_multiple_fields_in_header()
            {
                const string header = @"---
                        layout: post
                        title: This is a test jekyll document
                        description: TEST ALL THE THINGS
                        date: 2012-01-30
                        tags : 
                        - test
                        - alsotest
                        - lasttest
                        ---
            
                        ##Test
            
                        This is a test of YAML parsing";

                var result = header.YamlHeader();

                Assert.Equal("post", result["layout"].ToString());
                Assert.Equal("This is a test jekyll document", result["title"].ToString());
                Assert.Equal("2012-01-30", result["date"].ToString());
                Assert.Equal("TEST ALL THE THINGS", result["description"].ToString());
                Assert.Equal("[ test, alsotest, lasttest ]", result["tags"].ToString());
            }
        }
    }
}