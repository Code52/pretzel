using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensibility;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Composition;
using System.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Pretzel.Tests.Commands
{
    public class BaseParametersTests
    {
        [Export]
        [Shared]
        [CommandArguments(CommandName = "test")]
        public class BaseParametersImpl : BaseParameters
        {
            protected override void WithOptions(List<Option> options)
            {
                options.Add(new Option("-base"));
            }
        }

        [Export]
        [Shared]
        [CommandArgumentsExtention(CommandNames = new[] { "test" })]
        public class BaseParametersImplExt : IHaveCommandLineArgs
        {
            public void UpdateOptions(IList<Option> options)
            {
                options.Add(new Option("-ext"));
            }

            public void BindingCompleted()
            {
            }
        }

        [Fact]
        public void ExtentionIsPossible()
        {
            var configuration = new ContainerConfiguration();
            configuration.WithPart<BaseParametersImpl>();
            configuration.WithPart<BaseParametersImplExt>();
            using(var container = configuration.CreateContainer())
            {
                var sut = container.GetExport<BaseParametersImpl>();

                Assert.Contains(sut.Options, o => o.Name == "base");
                Assert.Contains(sut.Options, o => o.Name == "ext");
            }
        }
    }
}
