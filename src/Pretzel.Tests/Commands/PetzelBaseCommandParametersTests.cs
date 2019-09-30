using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using Pretzel.Logic;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Templating;
using Pretzel.Logic.Templating.Context;
using Xunit;

namespace Pretzel.Tests.Commands
{
    public abstract class PretzelBaseCommandParametersTests<T> : ParametersTests<T>
        where T : PretzelBaseCommandParameters
    {
        [Theory]
        [InlineData("--drafts")]
        public void Drafts(string argument)
        {
            var sut = BuildParameters(argument);

            Assert.True(sut.Drafts);
        }

        [Theory]
        [InlineData("-t", "liquid")]
        [InlineData("--template", "razor")]
        public void Template(string argument, string value)
        {
            var sut = BuildParameters(argument, value);

            Assert.Equal(value, sut.Template);
        }

        [Theory]
        [InlineData("-d", "foo/_site")]
        [InlineData("--destination", "bar/mySite")]
        public void Destination(string argument, string value)
        {
            var sut = BuildParameters(argument, value);

            Assert.Equal(fileSystem.Path.Combine(sut.Source, value), sut.Destination);
        }

        [Theory]
        [InlineData("-d", @"c:\tmp\_site")]
        public void DestinationRooted(string argument, string value)
        {
            var sut = BuildParameters(argument, value);

            Assert.Equal(value, sut.Destination);
        }

        [Fact]
        public void DestinationEmpty()
        {
            var sut = BuildParameters("-d");

            Assert.Equal(fileSystem.Path.Combine(sut.Source, "_site"), sut.Destination);
        }

        [Fact]
        public void DestinationDefaultValue()
        {
            var sut = BuildParameters();

            Assert.Equal(fileSystem.Path.Combine(sut.Source, "_site"), sut.Destination);
        }

        [Fact]
        public void DetectFromDirectory_WhenSpecifyingNoSiteEngines_DefaultValueIsLiquid()
        {
            var sut = BuildParameters();

            var siteContext = new SiteContext();

            sut.DetectFromDirectory(new Dictionary<string, ISiteEngine>(), siteContext);

            Assert.Equal("liquid", sut.Template);
        }

        [Fact]
        public void DetectFromDirectory_WhenSpecifyingTwoSiteEngines_CorrectValueIsPicked()
        {
            var sut = BuildParameters();

            var siteContext = new SiteContext { Config = new ConfigurationMock(new Dictionary<string, object> { { "pretzel", new Dictionary<string, object> { { "engine", "engine2" } } } }) };

            var siteEngine1 = Substitute.For<ISiteEngine>();
            siteEngine1.CanProcess(Arg.Any<SiteContext>())
                .Returns(ci => ci.Arg<SiteContext>().Engine == "engine1");

            var siteEngine2 = Substitute.For<ISiteEngine>();
            siteEngine2.CanProcess(Arg.Any<SiteContext>())
                .Returns(ci => ci.Arg<SiteContext>().Engine == "engine2");

            var siteEngines = new Dictionary<string, ISiteEngine>
            {
                { "engine1", siteEngine1 },
                { "engine2", siteEngine2 },
            };

            sut.DetectFromDirectory(siteEngines, siteContext);

            Assert.Equal("engine2", sut.Template);
        }

        [Fact]
        public void DetectFromDirectory_WhenSpecifyingNoPretzelConfig_DefaultValueIsLiquid()
        {
            var sut = BuildParameters();

            var siteContext = new SiteContext { Config = new Configuration() };

            var siteEngine1 = Substitute.For<ISiteEngine>();
            siteEngine1.CanProcess(Arg.Any<SiteContext>())
                .Returns(ci => ci.Arg<SiteContext>().Engine == "engine1");

            var siteEngines = new Dictionary<string, ISiteEngine>
            {
                { "engine1", siteEngine1 }
            };

            sut.DetectFromDirectory(siteEngines, siteContext);

            Assert.Equal("liquid", sut.Template);
        }

        [Fact]
        public void DetectFromDirectory_WhenSpecifyingNoEnginInPretzelConfig_DefaultValueIsLiquid()
        {
            var sut = BuildParameters();

            var siteContext = new SiteContext { Config = new ConfigurationMock(new Dictionary<string, object> { { "pretzel", new Dictionary<string, object> { } } }) };

            var siteEngine1 = Substitute.For<ISiteEngine>();
            siteEngine1.CanProcess(Arg.Any<SiteContext>())
                .Returns(ci => ci.Arg<SiteContext>().Engine == "engine1");

            var siteEngines = new Dictionary<string, ISiteEngine>
            {
                { "engine1", siteEngine1 }
            };

            sut.DetectFromDirectory(siteEngines, siteContext);

            Assert.Equal("liquid", sut.Template);
        }

        [Fact]
        public void DetectFromDirectory_WhenSpecifyingPretzelConfigSimpleValue_DefaultValueIsLiquid()
        {
            var sut = BuildParameters();

            var siteContext = new SiteContext { Config = new ConfigurationMock(new Dictionary<string, object> { { "pretzel", 42 } }) };

            var siteEngine1 = Substitute.For<ISiteEngine>();
            siteEngine1.CanProcess(Arg.Any<SiteContext>())
                .Returns(ci => ci.Arg<SiteContext>().Engine == "engine1");

            var siteEngines = new Dictionary<string, ISiteEngine>
            {
                { "engine1", siteEngine1 }
            };

            sut.DetectFromDirectory(siteEngines, siteContext);

            Assert.Equal("liquid", sut.Template);
        }
    }
}
