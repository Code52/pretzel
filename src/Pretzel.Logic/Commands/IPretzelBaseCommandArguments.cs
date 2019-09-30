using System;
using System.Linq;

namespace Pretzel.Logic.Commands
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
    public interface IPretzelBaseCommandArguments
    {
        string Source { get; }
        bool Debug { get; }
        bool Safe { get; }
        string Template { get; }
        string Destination { get; }
        bool Drafts { get; }
    }
}
