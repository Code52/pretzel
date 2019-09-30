using System;
using System.Linq;

namespace Pretzel.Logic.Commands
{
    public interface IImportCommandArguments : IPretzelBaseCommandArguments
    {
        string ImportType { get; }

        string ImportFile { get; }
    }
}
