using DotLiquid;
using NSubstitute;
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

        [Theory]
        [InlineData("yml", @"name: Eric Mill")]
        [InlineData("json", @"{ name: 'Eric Mill' }")]
        [InlineData("csv", @"name
""Eric Mill""
")]
        [InlineData("tsv", @"name
""Eric Mill""
")]
        public void renders_nested_object(string ext, string fileContent)
        {
            fileSystem.AddFile(Path.Combine(dataDirectory, $"person.{ext}"), new MockFileData(fileContent));

            var template = Template.Parse(@"{{ data.person.name }}");

            var hash = Hash.FromAnonymousObject(new
            {
                Data = data
            });

            var result = template.Render(hash);

            Assert.Equal("Eric Mill", result.Trim());
        }

        [Theory]
        [InlineData("yml", @"name: Eric Mill
address:
  street: Some Street
  postalcode: 1234")]
        [InlineData("json", @"{
  name: 'Eric Mill',
  address: {
  street: 'Some Street',
  postalcode: 1234
  }
}")]
        [InlineData("csv", @"name,address.street,address.postalcode
""Eric Mill"",""Some Street"",1234")]
        [InlineData("tsv", @"name	address.street	address.postalcode
""Eric Mill""	""Some Street""	1234")]
        public void renders_deep_nested_object(string ext, string fileContent)
        {
            fileSystem.AddFile(Path.Combine(dataDirectory, $"person.{ext}"), new MockFileData(fileContent));

            var template = Template.Parse(@"{{ data.person.address.postalcode }}");

            var hash = Hash.FromAnonymousObject(new
            {
                Data = data
            });

            var result = template.Render(hash);

            Assert.Equal("1234", result.Trim());
        }

        [Theory]
        [InlineData("yml", @"- name: Eric Mill
  github: konklone

- name: Parker Moore
  github: parkr

- name: Liu Fengyun
  github: liufengyun")]
        [InlineData("json", @"[{
    name: 'Eric Mill',
    github: 'konklone',
},{
    name: 'Parker Moore',
    github: 'parkr'
},{
    name: 'Liu Fengyun',
    github: 'liufengyun'
}]")]
        [InlineData("csv", @"name,github
""Eric Mill"",""konklone""
""Parker Moore"",""parkr""
""Liu Fengyun"",""liufengyun""")]
        [InlineData("tsv", @"name	github
""Eric Mill""	""konklone""
""Parker Moore""	""parkr""
""Liu Fengyun""	""liufengyun""")]
        public void renders_nested_lists(string ext, string fileContent)
        {
            fileSystem.AddFile(Path.Combine(dataDirectory, $"members.{ext}"), new MockFileData(fileContent));

            var template = Template.Parse(@"{{ data.members | size }}");

            var hash = Hash.FromAnonymousObject(new
            {
                Data = data
            });

            var result = template.Render(hash);

            Assert.Equal("3", result.Trim());
        }

        [Theory]
        [InlineData("yml", @"dave:
    name: David Smith
    twitter: DavidSilvaSmith")]
        [InlineData("json", @"{
    dave: {
        name: 'David Smith',
        twitter: 'DavidSilvaSmith'
    }
}")]
        //TODO: This is currently not supported. See https://jekyllrb.com/docs/datafiles/#example-accessing-a-specific-author
        //            [InlineData("csv", @"dave.name,dave.twitter
        //""David Smith"",""DavidSilvaSmith""")]
        //            [InlineData("tsv", @"dave.name	dave.twitter
        //""David Smith""	""DavidSilvaSmith""")]
        public void renders_dictionary_accessors(string ext, string fileContent)
        {
            fileSystem.AddFile(Path.Combine(dataDirectory, $"people.{ext}"), new MockFileData(fileContent));

            var template = Template.Parse(@"{{ data.people['dave'].name }}");

            var hash = Hash.FromAnonymousObject(new
            {
                Data = data
            });

            var result = template.Render(hash);

            Assert.Equal("David Smith", result.Trim());
        }

        [Theory]
        [InlineData("yml", @"name: Eric Mill")]
        [InlineData("json", @"{
    name: 'Eric Mill'
}")]
        [InlineData("csv", @"name
""Eric Mill""")]
        [InlineData("tsv", @"name
""Eric Mill""")]
        public void renders_nested_folder_object(string ext, string fileContent)
        {
            fileSystem.AddFile(Path.Combine(dataDirectory, $@"users\person.{ext}"), new MockFileData(fileContent));

            var template = Template.Parse(@"{{ data.users.person.name }}");

            var hash = Hash.FromAnonymousObject(new
            {
                Data = data
            });

            var result = template.Render(hash);

            Assert.Equal("Eric Mill", result.Trim());
        }

        [Theory]
        [InlineData("yml", @"name: Eric Mill
email: eric@example.com")]
        [InlineData("json", @"{
    name: 'Eric Mill',
    email: 'eric@example.com'
}")]
        [InlineData("csv", @"name,email
""Eric Mill"",""eric@example.com""")]
        [InlineData("tsv", @"name	email
""Eric Mill""	""eric@example.com""")]
        public void caches_result(string ext, string fileContent)
        {
            var fileSystem = Substitute.For<IFileSystem>();
            var data = new Data(fileSystem, dataDirectory);

            var directory = Substitute.For<DirectoryBase>();
            fileSystem.Directory.Returns(directory);
            directory.Exists(dataDirectory).Returns(true);

            var file = Substitute.For<FileBase>();
            fileSystem.File.Returns(file);

            var fileName = Path.Combine(dataDirectory, $"person.{ext}");
            file.Exists(fileName).Returns(true);
            file.ReadAllText(fileName).Returns(fileContent);

            var template = Template.Parse(@"{{ data.person.name }} {{ data.person.email }}");

            var hash = Hash.FromAnonymousObject(new
            {
                Data = data
            });

            var result = template.Render(hash);

            Assert.Equal("Eric Mill eric@example.com", result.Trim());

            file.Received(1).ReadAllText(fileName);
        }

        [Fact]
        public void renders_multiple_files_with_nested_objects()
        {
            fileSystem.AddFile(Path.Combine(dataDirectory, $"eric.yml"), new MockFileData(@"name: Eric Mill"));
            fileSystem.AddFile(Path.Combine(dataDirectory, $"manuel.yml"), new MockFileData(@"name: Manuel Grundner"));

            var hash = Hash.FromAnonymousObject(new
            {
                Data = data
            });

            var templateEric = Template.Parse(@"{{ data.eric.name }}");
            var resultEric = templateEric.Render(hash);
            Assert.Equal("Eric Mill", resultEric.Trim());

            var templateManuel = Template.Parse(@"{{ data.manuel.name }}");
            var resultManuel = templateManuel.Render(hash);
            Assert.Equal("Manuel Grundner", resultManuel.Trim());
        }
    }
}
