using System.Collections.Generic;
using System.IO;
using Pretzel.Logic.Extensions;
using Xunit;
using System;

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
                var input = File.ReadAllText("data\\yaml-header-input.md");
                var expected = File.ReadAllText("data\\markdown-no-header-output.md");

                var actual = input.ExcludeHeader();

                Assert.Equal(expected, actual);
            }

            [Fact]
            public void YamlHeader_WhenNoMetadataSet_ReturnsEmptyDictionary()
            {
                var actual = "".YamlHeader();
                Assert.NotNull(actual);
                Assert.IsType<Dictionary<string, object>>(actual);
                Assert.Equal(0, actual.Count);
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

            [Fact]
            public void YamlHeader_WithSampleData_HandleBoolean()
            {
                const string header = @"---
                        active: true
                        comments: false
                        other: 'true'
                        ---

                        ##Test

                        This is a test of YAML parsing";

                var result = header.YamlHeader();

                Assert.True((bool)result["active"]);
                Assert.False((bool)result["comments"]);
                Assert.Equal("True", result["other"].ToString());
            }

            [Fact]
            public void YamlHeader_WithListOfList_CanBeAccessed()
            {
                const string header = @"---
text: is a simple string
nestedlist:
 - [this, is, a]
 - [nested, list]
---

# Test

This is a test of YAML parsing";
                var result = header.YamlHeader();

                Assert.Equal("is a simple string", (string)result["text"]);

                var nestedlist = (List<object>)result["nestedlist"];
                Assert.Equal(2, nestedlist.Count);
                var nested0 = (List<string>)nestedlist[0];
                var nested1 = (List<string>)nestedlist[1];
                Assert.Equal(3, nested0.Count);
                Assert.Equal(2, nested1.Count);
            }


            [Fact]
            public void YamlHeader_WithDictionary_CanBeAccessed()
            {
                const string header = @"---
text: is a simple string
adictionary:
 a_field: true
 otherfield: 3
 facts:
  - it
  - should
  - work
 inception:
  furternested: it is
  but_it_works: false
  coeff: 1.0
---

# Test

This is a test of YAML parsing";
                var result = header.YamlHeader();

                Assert.Equal("is a simple string", (string)result["text"]);

                var adictionary = (Dictionary<string, object>)result["adictionary"];

                Assert.True((bool)adictionary["a_field"]);
                Assert.Equal("3", adictionary["otherfield"]);
                Assert.Equal(new[] { "it", "should", "work" }, (List<string>)adictionary["facts"]);

                var inception = (Dictionary<string, object>)adictionary["inception"];
                Assert.Equal("it is", (string)inception["furternested"]);
                Assert.False((bool)inception["but_it_works"]);
                Assert.Equal("1.0", inception["coeff"]);
            }

            [Fact]
            public void YamlHeader_WithListOfDictionary_CanBeAccessed()
            {
                const string header = @"---
text: is a simple string
adictlist:
 - nr: 1
   name: John Doe
   skip: false
 - nr: 2
   name: John Smith
   skip: true
 - nr: 3
   name: Lady Lorem
   skip: false
---

# Test

This is a test of YAML parsing";
                var result = header.YamlHeader();

                Assert.Equal("is a simple string", (string)result["text"]);

                var adictlist = (List<object>)result["adictlist"];

                var items = new[] {
                    new { nr = "1", name = "John Doe", skip = false },
                    new { nr = "2", name = "John Smith", skip = true },
                    new { nr = "3", name = "Lady Lorem", skip = false }
                };

                Assert.Equal(items.Length, adictlist.Count);
                for (int i = 0; i < items.Length; i++)
                {
                    var item = (Dictionary<string, object>)adictlist[i];
                    Assert.Equal(items[i].nr, (string)item["nr"]);
                    Assert.Equal(items[i].name, (string)item["name"]);
                    Assert.Equal(items[i].skip, (bool)item["skip"]);
                }
            }
        }

        public class ParseYamlTests
        {
            [Fact]
            public void ParseYaml_HandleYaml()
            {
                const string header = @"layout: post
title: This is a test jekyll document
description: TEST ALL THE THINGS
date: 2012-01-30
tags :
- test
- alsotest
- lasttest";

                var result = header.ParseYaml();

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
            public void ParseYaml_DoesntHandleHeader()
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

                Assert.ThrowsAny<Exception>(() => header.ParseYaml());
            }

            [Fact]
            public void ParseYaml_WhenNoData_ReturnsEmptyDictionary()
            {
                var actual = "".ParseYaml();
                Assert.NotNull(actual);
                Assert.IsType<Dictionary<string, object>>(actual);
                Assert.Equal(0, actual.Count);
            }
        }
    }
}
