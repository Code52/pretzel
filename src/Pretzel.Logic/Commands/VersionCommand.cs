using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Composition;
using System.Reflection;
using System.Threading.Tasks;
using Pretzel.Logic.Extensions;

namespace Pretzel.Logic.Commands
{
    [Shared]
    [Export]
    [CommandArguments]
    public class VersionCommandArguments : BaseCommandArguments
    {
        protected override IEnumerable<Option> CreateOptions() => Array.Empty<Option>();
    }

    [Shared]
    [CommandInfo(
        Name = "version",
        Description = "display current Pretzel version",
        ArgumentsType = typeof(VersionCommandArguments),
        CommandType = typeof(VersionCommand)
        )]
    public sealed class VersionCommand : Command<VersionCommandArguments>
    {
        protected override Task<int> Execute(VersionCommandArguments arguments)
        {
            Tracing.Info("V{0}", Assembly.GetExecutingAssembly().GetName().Version);

            return Task.FromResult(0);
        }
    }
}
