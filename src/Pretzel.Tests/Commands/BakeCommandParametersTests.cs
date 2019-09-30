using System;
using System.IO.Abstractions;
using System.Linq;
using Pretzel.Commands;

namespace Pretzel.Tests.Commands
{
    public class BakeCommandParametersTests : BakeBaseCommandParametersTests<BakeCommandArguments>
    {
        protected override BakeCommandArguments CreateParameters(IFileSystem fileSystem)
            => new BakeCommandArguments(fileSystem);
    }
}
