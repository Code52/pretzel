using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Composition;
using System.Composition.Hosting;
using System.Linq;
using Pretzel.Logic.Commands;
using Xunit;

namespace Pretzel.Tests.Commands
{
    public class BaseParametersTests
    {
        [Export]
        [Shared]
        [CommandArguments(CommandName = "test")]
        public class BaseParametersImpl : BaseCommandArguments
        {
            protected override IEnumerable<Option> CreateOptions() => new[]
            {
                new Option("-base")
            };
        }
        
        [Fact]
        public void ExtentionIsPossible()
        {
            var configuration = new ContainerConfiguration();
            configuration.WithPart<BaseParametersImpl>();
            using(var container = configuration.CreateContainer())
            {
                var sut = container.GetExport<BaseParametersImpl>();

                Assert.NotNull(sut.Options);
            }
        }
    }
}
