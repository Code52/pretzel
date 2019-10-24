using System;
using System.Composition;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using NSubstitute;
using Xunit;

namespace Pretzel.Tests
{
    public class ProgramTests
    {
        IDirectory directory;
        public ProgramTests()
        {
            Program.fileSystem = Substitute.For<IFileSystem>();
            directory = Substitute.For<IDirectory>();
            Program.fileSystem.Directory.Returns(directory);
        }

        [Theory]
        [InlineData("source", new[] { "s", "source" })]
        [InlineData("debug", new[] { "debug" })]
        [InlineData("safe", new[] { "safe" })]
        public void GlobalOptionsArePresent(string optionName, string[] argumentNames)
        {
            Assert.Contains(Program.GlobalOptions, o => o.Name == optionName);

            Assert.Contains(Program.GlobalOptions.First(o => o.Name == optionName).Aliases,
                alias => argumentNames.Contains(alias)
            );
        }

        [Fact]
        public void SourceDefaultsToCurrentDirectory()
        {
            var expectedDirectory = @"C:\foo";
            directory.GetCurrentDirectory().Returns(expectedDirectory);

            var sourceOption = Program.GlobalOptions.First(o => o.Name == "source");

            Assert.True(sourceOption.Argument.HasDefaultValue);
            Assert.Equal(expectedDirectory, sourceOption.Argument.GetDefaultValue());
        }

        [Fact]
        public void CompositionDoesNotThrow()
        {
            using (var compositionHost = Program.Compose(true, true, null))
            {
                var program = new Program();
                compositionHost.SatisfyImports(program);
            }
        }

        public class PatchSourcePath
        {
            public PatchSourcePath()
            {
                Program.fileSystem = new MockFileSystem();
            }

            private const string ExpectedPath = @"D:\Code";

            [Fact]
            public void PatchSourcePath_WhenNoParametersSet_MapsPathToCurrentDirectory()
            {
                var result = Program.PatchSourcePath(new[] { "bake" });
                Assert.Equal(new[] { "bake" }, result);
            }

            [Fact]
            public void PatchSourcePath_WhenOneParameterSet_MapsToPath()
            {
                var result = Program.PatchSourcePath(new[] { "bake", ExpectedPath });
                Assert.Equal(new[] { "bake", "-s", ExpectedPath }, result);
            }

            [Fact]
            public void PatchSourcePath_WhenOneParameterSet_MapsToPath_RelativePath()
            {
                var result = Program.PatchSourcePath(new[] { "bake", "mySite" });

                Assert.Equal(new[]
                {
                    "bake",
                    "-s",
                    Program.fileSystem.Path.Combine(
                        Program.fileSystem.Directory.GetCurrentDirectory(),
                        "mySite"
                        )
                }, result);
            }

            [Fact]
            public void PatchSourcePath_WhenSpecifyingSourcePathUsingShortParameter_MapsToPath()
            {
                var result = Program.PatchSourcePath(new[] { "bake", "--s", ExpectedPath });
                Assert.Equal(new[]
                {
                    "bake",
                    "--s",
                    ExpectedPath
                }, result);
            }

            [Fact]
            public void PatchSourcePath_WhenSpecifyingSourcePathUsingFullParameter_MapsToPath()
            {
                var result = Program.PatchSourcePath(new[] { "bake", "--source", ExpectedPath });
                Assert.Equal(new[]
                {
                    "bake",
                    "--source",
                    ExpectedPath
                }, result);
            }

            [Fact]
            public void PatchSourcePath_WhenSpecifyingSourcePathUsingShortParameterSingleDash_MapsToPath()
            {
                var result = Program.PatchSourcePath(new[] { "bake", "-s", ExpectedPath });
                Assert.Equal(new[]
                {
                    "bake",
                    "-s",
                    ExpectedPath
                }, result);
            }

            [Fact]
            public void PatchSourcePath_WhenSpecifyingSourcePathUsingFullParameterSingleDash_MapsToPath()
            {
                var result = Program.PatchSourcePath(new[] { "bake", "-source", ExpectedPath });
                Assert.Equal(new[]
                {
                    "bake",
                    "-source",
                    ExpectedPath
                }, result);
            }
        }
    }
}
