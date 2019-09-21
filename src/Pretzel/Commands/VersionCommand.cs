using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensions;
using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Composition;
using System.Reflection;
using System.Threading.Tasks;

namespace Pretzel.Commands
{
    [Shared]
    [CommandInfo(CommandName = BuiltInCommands.Version, CommandDescription = "display current Pretzel version")]
    public sealed class VersionCommand : ICommand
    {
        public async Task Execute()
        {
            Tracing.Info("V{0}", Assembly.GetExecutingAssembly().GetName().Version);
        }
    }
}
