using Pretzel.Logic.Extensions;
using System.Collections.Generic;
using System.Composition;
using System.Reflection;

namespace Pretzel.Commands
{
    [Shared]
    [CommandInfo(CommandName = "version")]
    public sealed class VersionCommand : ICommand
    {
        public void Execute(IEnumerable<string> arguments)
        {
            Tracing.Info("V{0}", Assembly.GetExecutingAssembly().GetName().Version);
        }

        public void WriteHelp(System.IO.TextWriter writer)
        {
            writer.WriteLine("  Display current Pretzel version.");
        }
    }
}
