using System.Collections.Generic;
using System.IO;

namespace Pretzel.Commands
{
    public interface ICommand
    {
        void Execute(IEnumerable<string> arguments);
        void WriteHelp(TextWriter writer); // TODO: obsolete this?
    }
}
