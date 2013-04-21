using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using NDesk.Options;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensibility;

namespace Pretzel.Commands
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class CommandCollection : IPartImportsSatisfiedNotification
    {
        [ImportMany]
        private Lazy<ICommand, ICommandInfo>[] Commands { get; set; }
        [ImportMany]
        IEnumerable<Lazy<IHaveCommandLineArgs>> CommandLineExtensions { get; set; }
        [Import]
        Lazy<CommandParameters> Parameters { get; set; }

        private Dictionary<string, ICommand> commandMap;

        public ICommand this[string name]
        {
            get
            {
                ICommand command;
                commandMap.TryGetValue(name.ToLower(), out command);
                return command;
            }
        }

        public void OnImportsSatisfied()
        {
            commandMap = new Dictionary<string, ICommand>(Commands.Length);

            foreach (var command in Commands)
            {
                if (!commandMap.ContainsKey(command.Metadata.CommandName))
                    commandMap.Add(command.Metadata.CommandName, command.Value);
            }
        }

        public void WriteHelp(OptionSet defaultSet)
        {
            Console.WriteLine(@"Usage:");
            Console.WriteLine(@"Pretzel.exe command [options]");
            Console.WriteLine();
            defaultSet.WriteOptionDescriptions(Console.Out);
            Console.WriteLine();

            foreach (var command in commandMap)
            {
                Console.WriteLine(@"Command: " + command.Key);
                command.Value.WriteHelp(Console.Out);
                var extraArgs = CommandLineExtensions.SelectMany(e => e.Value.GetArguments(command.Key)).ToArray();
                if (extraArgs.Any())
                    Parameters.Value.WriteOptions(Console.Out, extraArgs);
                Console.WriteLine();
            }
        }
    }
}
