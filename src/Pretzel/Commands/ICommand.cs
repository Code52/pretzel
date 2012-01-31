using System.IO;

namespace Pretzel.Commands
{
    public interface ICommand
    {
        void Execute(string[] arguments);
        void WriteHelp(TextWriter writer);
    }
}
