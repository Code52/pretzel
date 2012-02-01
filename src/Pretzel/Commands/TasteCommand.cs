using System;
using System.ComponentModel.Composition;
using System.IO;
using NDesk.Options;

namespace Pretzel.Commands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [CommandInfo(CommandName = "taste")]
    public sealed class TasteCommand : ICommand
    {
        public int Port { get; private set; }
        public bool Debug { get; private set; }

        private OptionSet Settings
        {
            get
            {
                return new OptionSet
                           {
                               {"p|port=", "The server port number.", v => Port = int.Parse(v)},
                               {"debug", "Enable debugging", v => Debug = true}
                           };
            }
        }

        public void Execute(string[] arguments)
        {
            Settings.Parse(arguments);
            Console.WriteLine("Port: " + Port);
            Console.WriteLine("Debug: " + Debug);
        }

        public void WriteHelp(TextWriter writer)
        {
            Settings.WriteOptionDescriptions(writer);
        }
    }
}
