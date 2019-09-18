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

            Assert.Contains("-t", output);
            Assert.Contains("--template=", output);
            Assert.Contains("--port=", output);
            Assert.Contains("-i", output);
            Assert.Contains("--import=", output);
            Assert.Contains("-f", output);
            Assert.Contains("--file=", output);
            Assert.Contains("--cleantarget", output);
            Assert.Contains("--destination", output);
        }

        [Fact]
        public void WriteOptions_WithOneParameterSpecified_DisplaysSelection()
        {
            subject.WriteOptions(writer, "-t");

            var output = writer.ToString();

            Assert.Contains("-t", output);
            Assert.Contains("--template=", output);
        }

        [Fact]
        public void WriteOptions_WithOneParameterSpecified_IgnoresOthers()
        {
            subject.WriteOptions(writer, "-t");

            var output = writer.ToString();

            Assert.DoesNotContain("-p", output);
            Assert.DoesNotContain("--port=", output);
            Assert.DoesNotContain("-i", output);
            Assert.DoesNotContain("--import=", output);
            Assert.DoesNotContain("-f", output);
            Assert.DoesNotContain("--file=", output);
        }

        [Fact]
        public void WriteOptions_WithTwoParameterSpecified_DisplaysSelection()
        {
            subject.WriteOptions(writer, "-t", "-p");

            var output = writer.ToString();

            Assert.Contains("-t", output);
            Assert.Contains("--template=", output);
            Assert.Contains("-p", output);
            Assert.Contains("--port=", output);
        }

        [Fact]
        public void WriteOptions_WithTwoParameterSpecified_IgnoresOthers()
        {
            subject.WriteOptions(writer, "-t", "-p");

            var output = writer.ToString();

            Assert.DoesNotContain("-i", output);
            Assert.DoesNotContain("--import=", output);
            Assert.DoesNotContain("-f", output);
            Assert.DoesNotContain("--file=", output);
        }
    }
}
