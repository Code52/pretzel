using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Composition;
using System.Reflection;
using System.Threading.Tasks;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensions;

namespace Pretzel.Commands
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
        CommandName = BuiltInCommands.Version,
        CommandDescription = "display current Pretzel version",
        CommandArgumentsType = typeof(VersionCommandArguments)
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
