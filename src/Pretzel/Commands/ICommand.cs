using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;

namespace Pretzel.Commands
{
    [InheritedExport]
    public interface ICommand
    {
        void Execute(IEnumerable<string> arguments);
        void WriteHelp(TextWriter writer); // TODO: obsolete this?
    }
}
