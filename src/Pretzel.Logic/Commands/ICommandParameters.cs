using System;
using System.Collections.Generic;
using System.CommandLine;

namespace Pretzel.Logic.Commands
{
    public interface ICommandParameters
    {
        IList<Option> Options { get; }
    }
}
