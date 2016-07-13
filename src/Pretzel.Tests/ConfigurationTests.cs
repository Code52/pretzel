using System;
using System.IO.Abstractions.TestingHelpers;
using Pretzel.Logic;
using Pretzel.Logic.Extensions;
using Xunit;

namespace Pretzel.Tests
{
    public class ConfigurationTests
    {
        private readonly Configuration _sut;

        private const string SampleConfig = @"
pretzel:
  engine: liquid

title: 'Site Title'

defaults:
  -
    scope:
      path: ''
    values:
      author: 'default-author'
  -
    scope:
      path: '_posts'
    values:
      layout: 'post'
      author: 'posts-specific-author'
  -
    scope:
      path: '_posts\2016'
    values:
      layout: 'post-layout-for-2016'
";

        public ConfigurationTests()
        {
            var fileSystem = new MockFileSystem();
            fileSystem.AddFile(@"C:\WebSite\_config.yml", new MockFileData(SampleConfig));

            _sut = new Configuration(fileSystem, @"C:\WebSite");
            _sut.ReadFromFile();
        }

        [Fact]
        public void Indexer_should_correctly_return_the_value()
        {
            Assert.Equal("Site Title", _sut["title"]);
        }

        [Fact]
        public void Permalinks_should_be_added_with_default_value_if_not_specified_in_file()
        {
            Assert.Equal(Configuration.DefaultPermalink, _sut["permalink"]);
        }

        [Fact]
        public void DefaultsForScope_should_layer_the_most_specific_scope_on_top()
        {
            var defaults = _sut.Defaults.ForScope(@"_posts\2016");

            Assert.Equal("post-layout-for-2016", defaults["layout"]);
        }

        [Fact]
        public void DefaultsForScope_should_take_value_from_less_specific_when_not_found_in_most_specific()
        {
            var defaults = _sut.Defaults.ForScope(@"_posts\2016");

            Assert.Equal("posts-specific-author", defaults["author"]);
        }

        [Fact]
        public void DefaultsForScope_should_fallback_to_value_from_empty_path_when_given_path_not_found()
        {
            var defaults = _sut.Defaults.ForScope("_nonexisting");

            Assert.Equal("default-author", defaults["author"]);
        }
    }
}
