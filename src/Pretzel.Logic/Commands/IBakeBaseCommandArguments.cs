using System;
using System.Linq;

namespace Pretzel.Logic.Commands
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public interface IBakeBaseCommandArguments : IPretzelBaseCommandArguments
    {
        bool CleanTarget { get; }
    }
}
