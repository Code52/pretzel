using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Pretzel.Logic.Templating.Liquid;
using Xunit;

namespace Pretzel.Tests.Templating.Liquid
{
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

                Subject.Process(fileSystem, @"C:\website\");
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
    }
}
