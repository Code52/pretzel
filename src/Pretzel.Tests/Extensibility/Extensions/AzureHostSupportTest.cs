using NDesk.Options;
using Pretzel.Logic.Extensibility.Extensions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Xunit;
using Xunit.Extensions;

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

        [InlineData("create")]
        [Theory]
        public void GetArguments_Create_Should_Return_Azure(string command)
        {
            // act
            var args = azureHostSupport.GetArguments(command);

            // assert
            Assert.Single(args);
            Assert.Equal("-azure", args[0]);
        }

        [InlineData(null)]
        [InlineData("")]
        [InlineData("bake")]
        [InlineData("taste")]
        [Theory]
        public void GetArguments_Default_Should_Return_Empty_Array(string command)
        {
            // act
            var args = azureHostSupport.GetArguments(command);

            // assert
            Assert.Empty(args);
        }

        [Fact]
        public void UpdateOptions_Should_Add_Azure()
        {
            var optionSet = new OptionSet();

            // act
            azureHostSupport.UpdateOptions(optionSet);

            // assert
            Assert.Single(optionSet);
            Assert.Equal("azure", optionSet[0].Prototype);
            Assert.NotNull(optionSet[0].Description);
        }

        [Fact]
        public void MixIn_AzureNotActivated_NothingHappens()
        {
            // arrange
            fileSystem.AddDirectory(@"c:\website");
            var optionSet = new OptionSet();
            azureHostSupport.UpdateOptions(optionSet);

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

            var optionSet = new OptionSet();
            azureHostSupport.UpdateOptions(optionSet);
            optionSet.Parse(new[] { "--azure" });

            // act
            azureHostSupport.MixIn(@"c:\website");

            // assert
            Assert.Contains(@"c:\website\_source\", fileSystem.AllDirectories);
            Assert.Contains(@"c:\website\_source\index.md", fileSystem.AllFiles);
            Assert.Contains(@"c:\website\_source\_posts\", fileSystem.AllDirectories);
            Assert.Contains(@"c:\website\Shim.cs", fileSystem.AllFiles);
            Assert.Contains(@"c:\website\Shim.csproj", fileSystem.AllFiles);
            Assert.Contains(@"c:\website\Shim.sln", fileSystem.AllFiles);
            Assert.Contains(@"c:\website\Pretzel.exe", fileSystem.AllFiles);
        }

    }
}
