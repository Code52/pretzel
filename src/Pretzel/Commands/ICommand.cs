using System.ComponentModel.Composition;
using System.IO;

namespace Pretzel.Commands
{
    [InheritedExport]
    public interface ICommand
    {
        void Execute(string[] arguments);
        void WriteHelp(TextWriter writer);
    }
}
