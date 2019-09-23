using NSubstitute;
using Pretzel.Commands;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Pretzel.Tests.Commands
{
    public class BakeCommandParametersTests
    {
        readonly IConsole Console = Substitute.For<IConsole>();

        BakeCommandParameters BuildParameters(params string[] args)
        {
            var rootCommand = new RootCommand();

            var parameters = new BakeCommandParameters(new MockFileSystem());
            parameters.OnImportsSatisfied();

            foreach (var option in parameters.Options)
                rootCommand.AddOption(option);
            
            var context = new InvocationContext(new Parser(rootCommand).Parse(args), Console);

            new ModelBinder(typeof(BakeCommandParameters))
                .UpdateInstance(parameters, context.BindingContext);

            return parameters;
        }

        [Theory]
        [InlineData("-c")]
        [InlineData("--cleantarget")]
        public void CleanTarget(string argument)
        {
            var sut = BuildParameters(argument);

            Assert.True(sut.CleanTarget);
        }
    }
}
