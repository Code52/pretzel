using System.Collections.Generic;
using Pretzel.Logic.Extensions;
using Xunit;

namespace Pretzel.Tests
{
    public class YamlExtensionsTests
    {
        public class YamlHeaderTests
        {
            [Fact]
            public void YamlHeader_WithSampleData_ReturnsExpectedValues()
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
                
                var tags = result["tags"] as IList<string>;
                Assert.Equal(3, tags.Count);
                Assert.Equal("test", tags[0]);
                Assert.Equal("alsotest", tags[1]);
                Assert.Equal("lasttest", tags[2]);
            }

            [Fact]
            public void RemoveHeader_WithSampleValue_ContainsRestOfDocument()
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

                var result = header.ExcludeHeader();

                Assert.Equal("##Test\r\n            \r\n                        This is a test of YAML parsing", result);
            }


            [Fact]
            public void YamlHeader_WhenNoMetadataSet_ReturnsEmptyDictionary()
            {
                Assert.NotNull("".YamlHeader());
            }

            [Fact]
            public void YamlHeader_WithInlineDataThatLooksLikeAnotherHeader_IgnoresTheInlineData()
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

This is a test of YAML parsing

    ---
    layout: inline-post
    title: inline-title
    description: inline description
    date: 2010-04-09
    tags : 
    - foo
    - bar
    - baz
    ---";

                var result = header.YamlHeader();

                Assert.Equal("post", result["layout"].ToString());
                Assert.Equal("This is a test jekyll document", result["title"].ToString());
                Assert.Equal("2012-01-30", result["date"].ToString());
                Assert.Equal("TEST ALL THE THINGS", result["description"].ToString());

                var tags = result["tags"] as IList<string>;
                Assert.Equal(3, tags.Count);
                Assert.Equal("test", tags[0]);
                Assert.Equal("alsotest", tags[1]);
                Assert.Equal("lasttest", tags[2]);
            }
        }
    }
}