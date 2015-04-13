using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensibility;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Xunit;

namespace Pretzel.Tests
{
    public class CommandParameterOutputTests
    {
        private readonly CommandParameters subject;
        private readonly StringWriter writer;

        public CommandParameterOutputTests()
        {
            subject = new CommandParameters(Enumerable.Empty<IHaveCommandLineArgs>(), new MockFileSystem());
            writer = new StringWriter();
        }

        [Fact]
        public void WriteOptions_WithNoParametersSpecified_DisplaysAll()
        {
            subject.WriteOptions(writer);

            var output = writer.ToString();

            Assert.True(output.Contains("-t"));
            Assert.True(output.Contains("--template="));
            Assert.True(output.Contains("--port="));
            Assert.True(output.Contains("-i"));
            Assert.True(output.Contains("--import="));
            Assert.True(output.Contains("-f"));
            Assert.True(output.Contains("--file="));
            Assert.True(output.Contains("--cleantarget"));
            Assert.True(output.Contains("--destination"));
        }

        [Fact]
        public void WriteOptions_WithOneParameterSpecified_DisplaysSelection()
        {
            subject.WriteOptions(writer, "-t");

            var output = writer.ToString();

            Assert.True(output.Contains("-t"));
            Assert.True(output.Contains("--template="));
        }

        [Fact]
        public void WriteOptions_WithOneParameterSpecified_IgnoresOthers()
        {
            subject.WriteOptions(writer, "-t");

            var output = writer.ToString();

            Assert.False(output.Contains("-p"));
            Assert.False(output.Contains("--port="));
            Assert.False(output.Contains("-i"));
            Assert.False(output.Contains("--import="));
            Assert.False(output.Contains("-f"));
            Assert.False(output.Contains("--file="));
        }

        [Fact]
        public void WriteOptions_WithTwoParameterSpecified_DisplaysSelection()
        {
            subject.WriteOptions(writer, "-t", "-p");

            var output = writer.ToString();

            Assert.True(output.Contains("-t"));
            Assert.True(output.Contains("--template="));
            Assert.True(output.Contains("-p"));
            Assert.True(output.Contains("--port="));
        }

        [Fact]
        public void WriteOptions_WithTwoParameterSpecified_IgnoresOthers()
        {
            subject.WriteOptions(writer, "-t", "-p");

            var output = writer.ToString();

            Assert.False(output.Contains("-i"));
            Assert.False(output.Contains("--import="));
            Assert.False(output.Contains("-f"));
            Assert.False(output.Contains("--file="));
        }
    }
}
