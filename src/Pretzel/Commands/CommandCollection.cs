using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Composition;
using System.Linq;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensibility;

namespace Pretzel.Commands
{
    [Export]
    [Shared]
    public sealed class CommandCollection
    {
        [ImportMany]
        public ExportFactory<ICommand, CommandInfoAttribute>[] Commands { get; set; }

        [ImportMany]
        public ExportFactory<ICommandParameters, CommandArgumentsAttribute>[] CommandArguments { get; set; }

        [Import]
        public Lazy<CommandParameters> Parameters { get; set; }

        private Dictionary<string, ICommand> commandMap;

        public ICommand this[string name]
        {
            get
            {
                ICommand command;
                commandMap.TryGetValue(name.ToLower(System.Globalization.CultureInfo.InvariantCulture), out command);
                return command;
            }
        }

        [Export]
        public RootCommand RootCommand { get; set; }

        [OnImportsSatisfied]
        public void OnImportsSatisfied()
        {
            commandMap = new Dictionary<string, ICommand>(Commands.Length);

            RootCommand = new RootCommand();

            foreach (var command in Commands)
            {
                if (!commandMap.ContainsKey(command.Metadata.CommandName))
                {
                    var subCommand = new Command(command.Metadata.CommandName, command.Metadata.CommandDescription);

                    foreach (var commandArguments in CommandArguments.Where(a => a.Metadata.CommandName == command.Metadata.CommandName))
                    {
                        foreach(var option in commandArguments.CreateExport().Value.Options)
                        {
                            subCommand.AddOption(option);
                        }
                    }

                    RootCommand.AddCommand(subCommand);
                    //var export = command.CreateExport();
                    //commandMap.Add(command.Metadata.CommandName, export.Value);
                }
            }
        }

        public void WriteHelp(List<Option> defaultSet)
        {
            Console.WriteLine(@"Usage:");
            Console.WriteLine(@"Pretzel.exe command [options]");
            Console.WriteLine();
            //defaultSet.WriteOptionDescriptions(Console.Out);
            Console.WriteLine();

            foreach (var command in commandMap)
            {
                Console.WriteLine(@"Command: " + command.Key);
                command.Value.WriteHelp(Console.Out);
                //var extraArgs = CommandLineExtensions.SelectMany(e => e.Value.GetArguments(command.Key)).ToArray();
                //if (extraArgs.Any())
                //    Parameters.Value.WriteOptions(Console.Out, extraArgs);
                Console.WriteLine();
            }
        }
    }
}
