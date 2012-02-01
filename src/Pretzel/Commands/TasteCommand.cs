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
        public string Path { get; private set; }

        private OptionSet Settings
        {
            get
            {
                return new OptionSet
                           {
                               {"p|port=", "The server port number.", v => Port = int.Parse(v)},
                               {"path=", "The path to site directory", p => Path = p },
                               {"debug", "Enable debugging", v => Debug = true}
                           };
            }
        }

        public void Execute(string[] arguments)
        {
            Settings.Parse(arguments);
            Console.WriteLine("Port: " + Port);
            Console.WriteLine("Debug: " + Debug);

            Path = Path ?? Directory.GetCurrentDirectory();
            Console.WriteLine("Path: " + Path);

            var f = new FileContentProvider();
            var w = new WebHost(Path, f);
            w.Start();
            Console.ReadLine();
        }

        public void WriteHelp(TextWriter writer)
        {
            Settings.WriteOptionDescriptions(writer);
        }
    }
}
