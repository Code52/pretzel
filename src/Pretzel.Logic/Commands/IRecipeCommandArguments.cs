using System;
using System.Linq;

namespace Pretzel.Logic.Commands
{
    public interface IRecipeCommandArguments : IPretzelBaseCommandArguments
    {
        bool WithProject { get; }
        bool Wiki { get; }
    }
}
