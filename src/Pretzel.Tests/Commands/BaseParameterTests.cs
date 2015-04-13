using Pretzel.Logic.Commands;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace Pretzel.Tests
{
    public class BaseParameterTests
    {
        private const string ExpectedPath = @"D:\Code";

        private readonly IFileSystem FileSystem = new MockFileSystem();

        public BaseParameters GetBaseParameter(string[] args)
        {
            return BaseParameters.Parse(args, FileSystem);
        }

        [Fact]
        public void WriteOptions_WithNoParametersSpecified_DisplaysAll()
        {
            var writer = new StringWriter();

            var subject = GetBaseParameter(new string[0]);

            subject.Options.WriteOptionDescriptions(writer);

            var output = writer.ToString();

            Assert.True(output.Contains("--directory="));
            Assert.True(output.Contains("--source="));
            Assert.True(output.Contains("--debug"));
            Assert.True(output.Contains("--help"));
            Assert.True(output.Contains("--safe"));
        }

        [Fact]
        public void Parse_CommandName_IsTheFirstParameter()
        {
            var subject = GetBaseParameter(new[] { "bake" });
            Assert.Equal("bake", subject.CommandName);
            Assert.Empty(subject.CommandArgs);
        }

        [Fact]
        public void Parse_WhenNoParametersSet_MapsPathToCurrentDirectory()
        {
            var subject = GetBaseParameter(new[] { "bake" });
            Assert.Equal(FileSystem.Directory.GetCurrentDirectory(), subject.Path);
        }

        [Fact]
        public void Parse_WhenOneParameterSet_MapsToPath()
        {
            var subject = GetBaseParameter(new[] { "bake", ExpectedPath });
            Assert.Equal(ExpectedPath, subject.Path);
        }

        [Fact]
        public void Parse_WhenSpecifyingPathUsingShortParameter_MapsToPath()
        {
            var subject = GetBaseParameter(new[] { "bake", "--d", ExpectedPath });
            Assert.Equal(ExpectedPath, subject.Path);
        }

        [Fact]
        public void Parse_WhenSpecifyingPathUsingFullParameter_MapsToPath()
        {
            var subject = GetBaseParameter(new[] { "bake", "--directory", ExpectedPath });
            Assert.Equal(ExpectedPath, subject.Path);
        }

        [Fact]
        public void Parse_WhenSpecifyingPathUsingShortParameterSingleDash_MapsToPath()
        {
            var subject = GetBaseParameter(new[] { "bake", "-d", ExpectedPath });
            Assert.Equal(ExpectedPath, subject.Path);
        }

        [Fact]
        public void Parse_WhenSpecifyingPathUsingFullParameterSingleDash_MapsToPath()
        {
            var subject = GetBaseParameter(new[] { "bake", "-directory", ExpectedPath });
            Assert.Equal(ExpectedPath, subject.Path);
        }

        [Fact]
        public void Parse_WhenSpecifyingAllParameters_ResultIsCorrect()
        {
            var subject = GetBaseParameter(new[] { "bake", @"-directory=c:\mysite", "-safe", "-help", "-debug" });

            Assert.Equal(@"c:\mysite", subject.Path);
            Assert.True(subject.Safe);
            Assert.True(subject.Help);
            Assert.True(subject.Debug);
            Assert.Equal("bake", subject.CommandName);
            Assert.Empty(subject.CommandArgs);
        }

        [Fact]
        public void Parse_WhenSpecifyingNoParameters_DefaultVeluesResultIsCorrect()
        {
            var args = new List<string>();

            var subject = GetBaseParameter(new[] { "bake" });

            Assert.Equal(FileSystem.Directory.GetCurrentDirectory(), subject.Path);
            Assert.False(subject.Safe);
            Assert.False(subject.Help);
            Assert.False(subject.Debug); Assert.Equal("bake", subject.CommandName);
            Assert.Empty(subject.CommandArgs);
        }

        [Fact]
        public void Parse_WhenOneParameterSet_MapsToPath_RelativePath()
        {
            var subject = GetBaseParameter(new[] { "bake", "mySite" });

            Assert.Equal(FileSystem.Path.Combine(FileSystem.Directory.GetCurrentDirectory(), "mySite"), subject.Path);
        }

        [Fact]
        public void Parse_WhenSpecifyingSourcePathUsingShortParameter_MapsToPath()
        {
            var subject = GetBaseParameter(new[] { "bake", "--s", ExpectedPath });
            Assert.Equal(ExpectedPath, subject.Path);
        }

        [Fact]
        public void Parse_WhenSpecifyingSourcePathUsingFullParameter_MapsToPath()
        {
            var subject = GetBaseParameter(new[] { "bake", "--source", ExpectedPath });
            Assert.Equal(ExpectedPath, subject.Path);
        }

        [Fact]
        public void Parse_WhenSpecifyingSourcePathUsingShortParameterSingleDash_MapsToPath()
        {
            var subject = GetBaseParameter(new[] { "bake", "-s", ExpectedPath });
            Assert.Equal(ExpectedPath, subject.Path);
        }

        [Fact]
        public void Parse_WhenSpecifyingSourcePathUsingFullParameterSingleDash_MapsToPath()
        {
            var subject = GetBaseParameter(new[] { "bake", "-source", ExpectedPath });
            Assert.Equal(ExpectedPath, subject.Path);
        }

        [Fact]
        public void Parse_WhenSpecifyingExtraParams_ThereShouldBeInCommandArgs()
        {
            var subject = GetBaseParameter(new[] { "bake", "-cleantarget", "-safe", "-nobrowser", "-p=8888" });
            Assert.NotNull(subject.CommandArgs);
            Assert.Equal(3, subject.CommandArgs.Count);
            Assert.Equal("-cleantarget", subject.CommandArgs[0]);
            Assert.Equal("-nobrowser", subject.CommandArgs[1]);
            Assert.Equal("-p=8888", subject.CommandArgs[2]);
        }
    }
}
