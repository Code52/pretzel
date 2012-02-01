using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Pretzel.Logic.Templating.Liquid;
using Xunit;

namespace Pretzel.Tests.Templating.Liquid
{
    public static class TestExtensions
    {
        public static string RemoveWhiteSpace(this string s)
        {
            return s.Replace("\r\n", "").Replace("\n", "");
        }
    }
    
    public class LiquidEngineTests
    {
        public class When_Recieving_A_Folder_Containing_One_File : SpecificationFor<LiquidEngine>
        {
            MockFileSystem fileSystem;
            const string fileContents = "<html><head></head><body></body></html>";

            public override LiquidEngine Given()
            {
                return new LiquidEngine();
            }

            public override void When()
            {
                var filepath = @"C:\website\index.html";


                fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
                                                {
                                                    {filepath, new MockFileData(fileContents)}
                                                });

                Subject.Process(fileSystem, @"C:\website\", null);
            }

            [Fact]
            public void That_Site_Folder_Is_Created()
            {
                Assert.True(fileSystem.Directory.Exists(@"C:\website\_site"));
            }

            [Fact]
            public void The_File_Is_Added_At_The_Root()
            {
                Assert.True(fileSystem.File.Exists(@"C:\website\_site\index.html"));
            }

            [Fact]
            public void The_File_Is_Identical()
            {
                Assert.Equal(fileContents, fileSystem.File.ReadAllText(@"C:\website\_site\index.html"));
            }
        }

        public class When_Recieving_A_Folder_Containing_One_File_In_A_Subfolder : SpecificationFor<LiquidEngine>
        {
            MockFileSystem fileSystem;
            const string fileContents = "<html><head></head><body></body></html>";

            public override LiquidEngine Given()
            {
                return new LiquidEngine();
            }

            public override void When()
            {
                var filepath = @"C:\website\content\index.html";


                fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
                                                {
                                                    {filepath, new MockFileData(fileContents)}
                                                });

                Subject.Process(fileSystem, @"C:\website\", null);
            }

            [Fact]
            public void That_Child_Folder_Is_Created()
            {
                Assert.True(fileSystem.Directory.Exists(@"C:\website\_site\content"));
            }

            [Fact]
            public void The_File_Is_Added_At_The_Root()
            {
                Assert.True(fileSystem.File.Exists(@"C:\website\_site\content\index.html"));
            }

            [Fact]
            public void The_File_Is_Identical()
            {
                Assert.Equal(fileContents, fileSystem.File.ReadAllText(@"C:\website\_site\content\index.html"));
            }
        }

        public class When_Recieving_A_File_With_A_Template : SpecificationFor<LiquidEngine>
        {
            MockFileSystem fileSystem;
            const string fileContents = "<html><head><title>{{ page.title }}</title></head><body></body></html>";
            const string expectedfileContents = "<html><head><title>My Web Site</title></head><body></body></html>";

            public override LiquidEngine Given()
            {
                return new LiquidEngine();
            }

            public override void When()
            {
                var filepath = @"C:\website\index.html";


                fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
                                                {
                                                    {filepath, new MockFileData(fileContents)}
                                                });

                Subject.Process(fileSystem, @"C:\website\", new Site { Title = "My Web Site" });
            }

            [Fact]
            public void The_File_Is_Applies_Data_To_The_Template()
            {
                Assert.Equal(expectedfileContents, fileSystem.File.ReadAllText(@"C:\website\_site\index.html"));
            }
        }

        public class When_Recieving_A_Markdown_File : SpecificationFor<LiquidEngine>
        {
            MockFileSystem fileSystem;
            const string templateContents = "<html><head><title>{{ page.title }}</title></head><body>{{ content }}</body></html>";
            const string pageContents = "---\r\n layout: default\r\n---\r\n\r\n# Hello World!";
            const string expectedfileContents = "<html><head><title>My Web Site</title></head><body><h1>Hello World!</h1></body></html>";

            public override LiquidEngine Given()
            {
                return new LiquidEngine();
            }

            public override void When()
            {
                var layoutFile = @"C:\website\_layouts\default.html";
                var contentFile = @"C:\website\index.md";

                fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
                                                {
                                                    {layoutFile, new MockFileData(templateContents)},
                                                    {contentFile, new MockFileData(pageContents)}
                                                });

                Subject.Process(fileSystem, @"C:\website\", new Site { Title = "My Web Site" });
            }

            [Fact]
            public void The_File_Is_Applies_Data_To_The_Template()
            {
                Assert.Equal(expectedfileContents, fileSystem.File.ReadAllText(@"C:\website\_site\index.html").RemoveWhiteSpace());
            }

            [Fact]
            public void Does_Not_Copy_Template_To_Output()
            {
                Assert.False(fileSystem.File.Exists(@"C:\website\_site\_layouts\default.html"));
            }
        }
    }
}
