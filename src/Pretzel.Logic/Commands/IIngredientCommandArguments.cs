using System;
using System.Linq;

namespace Pretzel.Logic.Commands
{
    public interface IIngredientCommandArguments : IPretzelBaseCommandArguments
    {
        string NewPostTitle { get; }
    }
}
