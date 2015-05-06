using Pretzel.Logic.Extensions;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reflection;

namespace Pretzel.Commands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [CommandInfo(CommandName = "version")]
    public sealed class VersionCommand : ICommand
    {
        public void Execute(IEnumerable<string> arguments)
        {
            Tracing.Info(string.Format("V{0}", Assembly.GetExecutingAssembly().GetName().Version));
        }

        public void WriteHelp(System.IO.TextWriter writer)
        {
            writer.WriteLine("  Display current Pretzel version.");
        }
    }
}
