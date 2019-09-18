using DotLiquid;
using Pretzel.Logic.Templating.Context;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Pretzel.Tests.Templating.Context
{
    public class DataTests
    {
        private readonly Data data;
        private readonly MockFileSystem fileSystem;
        private readonly string dataDirectory = @"C:\_data";

        public DataTests()
        {
            fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
            data = new Data(fileSystem, dataDirectory);
        }



        [Fact]
        public void renders_empty_string_if_data_directory_does_not_exist()
        {
            var template = Template.Parse(@"{{ data.people }}");

            var hash = Hash.FromAnonymousObject(new { Data = data });

            var result = template.Render(hash);

            Assert.Equal("", result.Trim());
        }

        [Fact]
        public void renders_yaml_nested_object()
        {
            fileSystem.AddFile(Path.Combine(dataDirectory, "person.yml"), new MockFileData(@"name: Eric Mill"));

            var template = Template.Parse(@"{{ data.person.name }}");

            var hash = Hash.FromAnonymousObject(new
            {
                Data = data
            });

            var result = template.Render(hash);

            Assert.Equal("Eric Mill", result.Trim());
        }

        [Fact]
        public void renders_yaml_deep_nested_object()
        {
            fileSystem.AddFile(Path.Combine(dataDirectory, "person.yml"), new MockFileData(@"name: Eric Mill
address:
  street: Some Street
  postalcode: 1234"));

            var template = Template.Parse(@"{{ data.person.address.postalcode }}");

            var hash = Hash.FromAnonymousObject(new
            {
                Data = data
            });

            var result = template.Render(hash);

            Assert.Equal("1234", result.Trim());
        }

        [Fact]
        public void renders_yaml_nested_lists()
        {
            fileSystem.AddFile(Path.Combine(dataDirectory, "members.yml"), new MockFileData(@"- name: Eric Mill
  github: konklone

- name: Parker Moore
  github: parkr

- name: Liu Fengyun
  github: liufengyun"));

            var template = Template.Parse(@"{{ data.members | size }}");

            var hash = Hash.FromAnonymousObject(new
            {
                Data = data
            });

            var result = template.Render(hash);

            Assert.Equal("3", result.Trim());
        }

        [Fact]
        public void renders_yaml_dictionary_accessors()
        {
            fileSystem.AddFile(Path.Combine(dataDirectory, "people.yml"), new MockFileData(@"dave:
    name: David Smith
    twitter: DavidSilvaSmith"));

            var template = Template.Parse(@"{{ data.people['dave'].name }}");

            var hash = Hash.FromAnonymousObject(new
            {
                Data = data
            });

            var result = template.Render(hash);

            Assert.Equal("David Smith", result.Trim());
        }
    }
}
