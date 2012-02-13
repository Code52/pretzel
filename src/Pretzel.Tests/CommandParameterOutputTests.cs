using System.IO;
using Pretzel.Logic.Commands;
using Xunit;

namespace Pretzel.Tests
{
    public class CommandParameterOutputTests
    {
        readonly CommandParameters subject;
        readonly StringWriter writer;

        public CommandParameterOutputTests()
        {
            subject = new CommandParameters();
            writer = new StringWriter();
        }

        [Fact]
        public void WriteOptions_WithNoParametersSpecified_DisplaysAll()
        {
            subject.WriteOptions(writer);

            var output = writer.ToString();

            Assert.True(output.Contains("-t"));
            Assert.True(output.Contains("--template="));
            Assert.True(output.Contains("-d"));
            Assert.True(output.Contains("--directory="));
            Assert.True(output.Contains("-p"));
            Assert.True(output.Contains("--port="));
            Assert.True(output.Contains("-i"));
            Assert.True(output.Contains("--import="));
            Assert.True(output.Contains("-f"));
            Assert.True(output.Contains("--file="));
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

            Assert.False(output.Contains("-d"));
            Assert.False(output.Contains("--directory="));
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
            subject.WriteOptions(writer, "-t", "-d");

            var output = writer.ToString();

            Assert.True(output.Contains("-t"));
            Assert.True(output.Contains("--template="));
            Assert.True(output.Contains("-d"));
            Assert.True(output.Contains("--directory="));
        }

        [Fact]
        public void WriteOptions_WithTwoParameterSpecified_IgnoresOthers()
        {
            subject.WriteOptions(writer, "-t", "-d");

            var output = writer.ToString();

            Assert.False(output.Contains("-p"));
            Assert.False(output.Contains("--port="));
            Assert.False(output.Contains("-i"));
            Assert.False(output.Contains("--import="));
            Assert.False(output.Contains("-f"));
            Assert.False(output.Contains("--file="));
        }
    }
}
