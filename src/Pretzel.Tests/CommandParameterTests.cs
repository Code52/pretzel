using System.Collections.Generic;
using System.IO;
using NSubstitute;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Templating;
using Pretzel.Logic.Templating.Context;
using Xunit;

namespace Pretzel.Tests
{
    public class CommandParameterTests
    {
        readonly CommandParameters subject;
        const string ExpectedPath = @"D:\Code";
        const string ExpectedImportFile = @"D:\Code\import.xml";
        const string ExpectedImportType = "wordpress";
        const string ExpectedTemplate = @"jekyll";
        const string ExpectedPort = "8000";
        const decimal ExpectedPortDecimal = 8000;

        public CommandParameterTests()
        {
            subject = new CommandParameters();
        }

        [Fact]
        public void Parse_WhenNoParametersSet_MapsPathToCurrentDirectory()
        {
            var args = new List<string>();
            subject.Parse(args);
            Assert.Equal(Directory.GetCurrentDirectory(), subject.Path);
        }

        [Fact]
        public void Parse_WhenOneParameterSet_MapsToPath()
        {
            var args = new List<string> { ExpectedPath };
            subject.Parse(args);
            Assert.Equal(ExpectedPath, subject.Path);
        }

        [Fact]
        public void Parse_WhenSpecifyingTemplateUsingShortParameter_MapsToPath()
        {
            var args = new List<string> { "--t", ExpectedTemplate };
            subject.Parse(args);
            Assert.Equal(ExpectedTemplate, subject.Template);
        }

        [Fact]
        public void Parse_WhenSpecifyingTemplateUsingFullParameter_MapsToPath()
        {
            var args = new List<string> { "--template", ExpectedTemplate };
            subject.Parse(args);
            Assert.Equal(ExpectedTemplate, subject.Template);
        }

        [Fact]
        public void Parse_WhenSpecifyingTemplateUsingShortParameterSingleDash_MapsToPath()
        {
            var args = new List<string> { "-t", ExpectedTemplate };
            subject.Parse(args);
            Assert.Equal(ExpectedTemplate, subject.Template);
        }

        [Fact]
        public void Parse_WhenSpecifyingTemplateUsingFullParameterSingleDash_MapsToPath()
        {
            var args = new List<string> { "-template", ExpectedTemplate };
            subject.Parse(args);
            Assert.Equal(ExpectedTemplate, subject.Template);
        }

        [Fact]
        public void Parse_WhenSpecifyingPathUsingShortParameter_MapsToPath()
        {
            var args = new List<string> { "--d", ExpectedPath };
            subject.Parse(args);
            Assert.Equal(ExpectedPath, subject.Path);
        }

        [Fact]
        public void Parse_WhenSpecifyingPathUsingFullParameter_MapsToPath()
        {
            var args = new List<string> { "--directory", ExpectedPath };
            subject.Parse(args);
            Assert.Equal(ExpectedPath, subject.Path);
        }

        [Fact]
        public void Parse_WhenSpecifyingPathUsingShortParameterSingleDash_MapsToPath()
        {
            var args = new List<string> { "-d", ExpectedPath };
            subject.Parse(args);
            Assert.Equal(ExpectedPath, subject.Path);
        }

        [Fact]
        public void Parse_WhenSpecifyingPathUsingFullParameterSingleDash_MapsToPath()
        {
            var args = new List<string> { "-directory", ExpectedPath };
            subject.Parse(args);
            Assert.Equal(ExpectedPath, subject.Path);
        }

        [Fact]
        public void Parse_WhenSpecifyingPortWithoutParamerers_IsGreaterThanZero()
        {
            Assert.True(subject.Port > 0);
        }

        [Fact]
        public void Parse_WhenSpecifyingPortUsingShortParameter_MapsToPath()
        {
            var args = new List<string> { "--p", ExpectedPort };
            subject.Parse(args);
            Assert.Equal(ExpectedPortDecimal, subject.Port);
        }

        [Fact]
        public void Parse_WhenSpecifyingPortUsingFullParameter_MapsToPath()
        {
            var args = new List<string> { "--port", ExpectedPort };
            subject.Parse(args);
            Assert.Equal(ExpectedPortDecimal, subject.Port);
        }

        [Fact]
        public void Parse_WhenSpecifyingPortUsingShortParameterSingleDash_MapsToPath()
        {
            var args = new List<string> { "-p", ExpectedPort };
            subject.Parse(args);
            Assert.Equal(ExpectedPortDecimal, subject.Port);
        }

        [Fact]
        public void Parse_WhenSpecifyingPortUsingFullParameterSingleDash_MapsToPath()
        {
            var args = new List<string> { "-port", ExpectedPort };
            subject.Parse(args);
            Assert.Equal(ExpectedPortDecimal, subject.Port);
        }

        [Fact]
        public void Parse_WhenSpecifyingImportFileUsingShortParameter_MapsToPath()
        {
            var args = new List<string> { "--f", ExpectedImportFile };
            subject.Parse(args);
            Assert.Equal(ExpectedImportFile, subject.ImportPath);
        }

        [Fact]
        public void Parse_WhenSpecifyingImportFileUsingFullParameter_MapsToPath()
        {
            var args = new List<string> { "--file", ExpectedImportFile };
            subject.Parse(args);
            Assert.Equal(ExpectedImportFile, subject.ImportPath);
        }

        [Fact]
        public void Parse_WhenSpecifyingImportFileUsingShortParameterSingleDash_MapsToPath()
        {
            var args = new List<string> { "-f", ExpectedImportFile };
            subject.Parse(args);
            Assert.Equal(ExpectedImportFile, subject.ImportPath);
        }

        [Fact]
        public void Parse_WhenSpecifyingImportFileUsingFullParameterSingleDash_MapsToPath()
        {
            var args = new List<string> { "-file", ExpectedImportFile };
            subject.Parse(args);
            Assert.Equal(ExpectedImportFile, subject.ImportPath);
        }

        [Fact]
        public void Parse_WhenSpecifyingImportTypeUsingShortParameter_MapsToPath()
        {
            var args = new List<string> { "--i", ExpectedImportType };
            subject.Parse(args);
            Assert.Equal(ExpectedImportType, subject.ImportType);
        }

        [Fact]
        public void Parse_WhenSpecifyingImportTypeUsingFullParameter_MapsToPath()
        {
            var args = new List<string> { "--import", ExpectedImportType };
            subject.Parse(args);
            Assert.Equal(ExpectedImportType, subject.ImportType);
        }

        [Fact]
        public void Parse_WhenSpecifyingImportTypeUsingShortParameterSingleDash_MapsToPath()
        {
            var args = new List<string> { "-i", ExpectedImportType };
            subject.Parse(args);
            Assert.Equal(ExpectedImportType, subject.ImportType);
        }

        [Fact]
        public void Parse_WhenSpecifyingImportTypeUsingFullParameterSingleDash_MapsToPath()
        {
            var args = new List<string> { "-import", ExpectedImportType };
            subject.Parse(args);
            Assert.Equal(ExpectedImportType, subject.ImportType);
        }

        [Fact]
        public void DetectFromDirectory_WithNoEngines_SetsTemplateToLiquid()
        {
            subject.DetectFromDirectory(new Dictionary<string, ISiteEngine>(), new SiteContext());
            Assert.Equal("liquid", subject.Template);
        }

        [Fact]
        public void DetectFromDirectory_WithMatchingEngines_SetsTemplateToCustomValue()
        {
            // arrange
            const string key = "foo";
            var engine = Substitute.For<ISiteEngine>();
            engine.CanProcess(Arg.Any<SiteContext>()).Returns(true);
            var engines = new Dictionary<string, ISiteEngine> { { key, engine } };

            // act 
            subject.DetectFromDirectory(engines, new SiteContext());

            Assert.Equal(key, subject.Template);
        }
    }
}
