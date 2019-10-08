using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Pretzel.Logic.Extensibility.Extensions;
using Xunit;

namespace Pretzel.Tests.Extensibility.Extensions
{
    public class AzureHostSupportTest
    {
        readonly MockFileSystem fileSystem;
        readonly AzureHostSupport azureHostSupport;
        readonly MockAssembly assembly;

        public AzureHostSupportTest()
        {
            fileSystem = new MockFileSystem();
            assembly = new MockAssembly();
            azureHostSupport = new AzureHostSupport(fileSystem, assembly);
        }

        [Fact]
        public void MixIn_AzureNotActivated_NothingHappens()
        {
            // arrange
            fileSystem.AddDirectory(@"c:\website");

            azureHostSupport.Arguments = new AzureHostSupportArguments();

            // act
            azureHostSupport.MixIn(@"c:\website");

            // assert
            Assert.Equal(2, fileSystem.AllPaths.Count());
        }

        [Fact]
        public void MixIn_AzureActivated_AllFilesAreIncluded()
        {
            // arrange
            fileSystem.AddDirectory(@"c:\website");
            fileSystem.AddFile(@"c:\website\index.md", MockFileData.NullObject);
            fileSystem.AddDirectory(@"c:\website\_posts");

            assembly.EntryAssemblyLocation = @"c:\tools\Pretzel.exe";
            fileSystem.AddFile(@"c:\tools\Pretzel.exe", MockFileData.NullObject);

            azureHostSupport.Arguments = new AzureHostSupportArguments
            {
                Azure = true
            };

            // act
            azureHostSupport.MixIn(@"c:\website");

            // assert
            Assert.Contains(@"c:\website\_source", fileSystem.AllDirectories);
            Assert.Contains(@"c:\website\_source\index.md", fileSystem.AllFiles);
            Assert.Contains(@"c:\website\_source\_posts", fileSystem.AllDirectories);
            Assert.Contains(@"c:\website\Shim.cs", fileSystem.AllFiles);
            Assert.Contains(@"c:\website\Shim.csproj", fileSystem.AllFiles);
            Assert.Contains(@"c:\website\Shim.sln", fileSystem.AllFiles);
            Assert.Contains(@"c:\website\Pretzel.exe", fileSystem.AllFiles);
        }

    }
}
