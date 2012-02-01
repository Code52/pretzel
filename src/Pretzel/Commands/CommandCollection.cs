﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Pretzel.Commands
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class CommandCollection : IPartImportsSatisfiedNotification
    {
        [ImportMany]
        private Lazy<ICommand, ICommandInfo>[] Commands { get; set; }

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

        public void WriteHelp()
        {
            Console.WriteLine("Usage:");
            foreach (var command in commandMap)
            {
                Console.WriteLine("Command: " + command.Key);
                command.Value.WriteHelp(Console.Out);
            }
        }
    }
}
