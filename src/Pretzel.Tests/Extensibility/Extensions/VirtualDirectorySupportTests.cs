using NDesk.Options;
using NSubstitute;
using Pretzel.Logic.Extensibility.Extensions;
using Pretzel.Logic.Templating.Context;
using System.Collections.Generic;
using System.IO.Abstractions;
using Xunit;
using Xunit.Extensions;

namespace Pretzel.Tests.Extensibility.Extensions
{
    public class VirtualDirectorySupportTests
    {
        readonly IFileSystem fileSystem;
        readonly VirtualDirectorySupport vdirSupport;

        public VirtualDirectorySupportTests()
        {
            fileSystem = Substitute.For<IFileSystem>();
            vdirSupport = new VirtualDirectorySupport(fileSystem);
        }

        [Fact]
        public void not_processed_when_no_argument_is_passed_in()
        {
            // arrange
            var returnThis = Substitute.For<FileBase>();
            fileSystem.File.Returns(returnThis);
            var optionSet = new OptionSet();
            vdirSupport.UpdateOptions(optionSet);
            optionSet.Parse(new string[0]);

            // act
            vdirSupport.Transform(new SiteContext
            {
                Pages = new List<Page>
                                                      {
                                                          new NonProcessedPage
                                                              {
                                                                  OutputFile = "Test.html"
                                                              },
                                                      }
            });

            // assert
            returnThis.DidNotReceive().ReadAllText("Test.html");
        }

        [Fact]
        public void processes_only_when_vdir_argument_is_passed()
        {
            // arrange
            var returnThis = Substitute.For<FileBase>();
            fileSystem.File.Returns(returnThis);
            var optionSet = new OptionSet();
            vdirSupport.UpdateOptions(optionSet);
            optionSet.Parse(new[] { "--vDir", "something" });

            // act
            vdirSupport.Transform(new SiteContext
                                      {
                                          Pages = new List<Page>
                                                      {
                                                          new NonProcessedPage
                                                              {
                                                                  OutputFile = "Test.html"
                                                              },
                                                      }
                                      });

            // assert
            returnThis.Received().ReadAllText("Test.html");
        }

        [Fact]
        public void processes_only_css_and_html_files() 
        {
            // arrange
            var returnThis = Substitute.For<FileBase>();
            fileSystem.File.Returns(returnThis);
            var optionSet = new OptionSet();
            vdirSupport.UpdateOptions(optionSet);
            optionSet.Parse(new[] { "--vDir", "something" });

            // act
            vdirSupport.Transform(new SiteContext
                                      {
                                          Pages = new List<Page>
                                                      {
                                                          new NonProcessedPage
                                                              {
                                                                  OutputFile = "Test.bin"
                                                              },
                                                              new NonProcessedPage
                                                              {
                                                                  OutputFile = "Test.html"
                                                              },
                                                              new NonProcessedPage
                                                              {
                                                                  OutputFile = "Test.htm"
                                                              },
                                                              new NonProcessedPage
                                                              {
                                                                  OutputFile = "Test.css"
                                                              },
                                                      }
                                      });

            // assert
            returnThis.DidNotReceive().ReadAllText("Test.bin");
            returnThis.Received().ReadAllText("Test.html");
            returnThis.Received().ReadAllText("Test.htm");
            returnThis.Received().ReadAllText("Test.css");
        }

        [Fact]
        public void includes_virtual_directory_in_href()
        {
            // arrange
            var returnThis = Substitute.For<FileBase>();
            fileSystem.File.Returns(returnThis);
            var optionSet = new OptionSet();
            vdirSupport.UpdateOptions(optionSet);
            optionSet.Parse(new[] { "--vDir", "something" });

            const string body = @"<body>
<a href=""/dir/file.html"" />
<img src=""/img/something.png"" />
</body>";
            returnThis.ReadAllText("Test.html").Returns(body);

            // act
            vdirSupport.Transform(new SiteContext
            {
                Pages = new List<Page>
                                                      {
                                                              new NonProcessedPage
                                                              {
                                                                  OutputFile = "Test.html"
                                                              },
                                                      }
            });

            // assert
            const string newBody = @"<body>
<a href=""/something/dir/file.html"" />
<img src=""/something/img/something.png"" />
</body>";
            returnThis.Received().WriteAllText("Test.html", newBody);
        }

        [InlineData("bake")]
        [InlineData("taste")]
        [Theory]
        public void GetArguments_Bake_Or_Taste_Should_Return_VDir(string command)
        {
            // act
            var args = vdirSupport.GetArguments(command);

            // assert
            Assert.Equal(1, args.Length);
            Assert.Equal("-vDir", args[0]);
        }

        [InlineData(null)]
        [InlineData("")]
        [InlineData("create")]
        [Theory]
        public void GetArguments_Default_Should_Return_Empty_Array(string command)
        {
            // act
            var args = vdirSupport.GetArguments(command);

            // assert
            Assert.Equal(0, args.Length);
        }
    }
}