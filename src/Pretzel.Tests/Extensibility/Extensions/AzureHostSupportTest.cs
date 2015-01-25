using NDesk.Options;
using NSubstitute;
using Pretzel.Logic.Extensibility.Extensions;
using Pretzel.Logic.Templating.Context;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Xunit;
using Xunit.Extensions;
using System.Linq;

namespace Pretzel.Tests.Extensibility.Extensions
{
    public class AzureHostSupportTest
    {
        readonly MockFileSystem fileSystem;
        readonly AzureHostSupport azureHostSupport;

        public AzureHostSupportTest()
        {
            fileSystem = new MockFileSystem();
            azureHostSupport = new AzureHostSupport(fileSystem);
        }

        [InlineData("create")]
        [Theory]
        public void GetArguments_Create_Should_Return_Azure(string command)
        {
            // act
            var args = azureHostSupport.GetArguments(command);

            // assert
            Assert.Equal(1, args.Length);
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
            Assert.Equal(0, args.Length);
        }

        [Fact]
        public void UpdateOptions_Should_Add_Azure()
        {
            var optionSet = new OptionSet();

            // act
            azureHostSupport.UpdateOptions(optionSet);

            // assert
            Assert.Equal(1, optionSet.Count);
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

        [Fact(Skip="Cannot be tested for now because Assembly.GetEntryAssembly() returns null")]
        public void MixIn_AzureActivated_AllFilesAreIncluded()
        {
            // arrange
            fileSystem.AddDirectory(@"c:\website");
            fileSystem.AddFile(@"c:\website\index.md", MockFileData.NullObject);
            fileSystem.AddDirectory(@"c:\website\_posts");

            var optionSet = new OptionSet();
            azureHostSupport.UpdateOptions(optionSet);
            optionSet.Parse(new[] { "--azure" });

            // act
            azureHostSupport.MixIn(@"c:\website");

            // assert
            Assert.True(fileSystem.AllDirectories.Contains(@"c:\website\_source"));
            Assert.True(fileSystem.AllFiles.Contains(@"c:\website\_source\index.md"));
            Assert.True(fileSystem.AllDirectories.Contains(@"c:\website\_source\_posts"));
            Assert.True(fileSystem.AllFiles.Contains(@"c:\website\Shim.cs"));
            Assert.True(fileSystem.AllFiles.Contains(@"c:\website\Shim.csproj"));
            Assert.True(fileSystem.AllFiles.Contains(@"c:\website\Shim.sln"));
            Assert.True(fileSystem.AllFiles.Contains(@"c:\website\Pretzel.exe"));
        }

    }
}