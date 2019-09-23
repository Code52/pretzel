using NSubstitute;
using Pretzel.Commands;
using Pretzel.Logic.Commands;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Pretzel.Tests.Commands
{
    public class BakeCommandParametersTests : BakeBaseCommandParametersTests<BakeCommandParameters>
    {
        protected override BakeCommandParameters CreateParameters(IFileSystem fileSystem)
            => new BakeCommandParameters(fileSystem);
    }
}
