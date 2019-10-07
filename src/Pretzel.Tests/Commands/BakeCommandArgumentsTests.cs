using System;
using System.IO.Abstractions;
using System.Linq;
using Pretzel.Logic.Commands;

namespace Pretzel.Tests.Commands
{
    public class BakeCommandArgumentsTests : BakeBaseCommandArgumentsTests<BakeCommandArguments>
    {
        protected override BakeCommandArguments CreateArguments(IFileSystem fileSystem)
            => new BakeCommandArguments(fileSystem);
    }
}
