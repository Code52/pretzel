using System;
using System.ComponentModel.Composition;
using System.IO;
using NDesk.Options;
using Pretzel.Logic.Extensions;

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
			Tracing.Info("taste - testing a site locally");
            Settings.Parse(arguments);
            if (Port == 0)
            {
                Port = 8080;
            }

            Tracing.Info("Port: " + Port);
            Tracing.Info("Debug: " + Debug);

            var f = new FileContentProvider();
            var w = new WebHost(Directory.GetCurrentDirectory(), f);
            w.Start();
			
			Tracing.Info("Press 'Q' to stop the process");
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey();
            } 
            while (key.Key != ConsoleKey.Q);
        }

        public void WriteHelp(TextWriter writer)
        {
            Settings.WriteOptionDescriptions(writer);
        }
    }
}
