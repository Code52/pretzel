using System;
using System.ComponentModel.Composition;
using System.IO;
using NDesk.Options;

namespace Pretzel.Commands
{
    [Export(typeof(ICommand))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [CommandInfo(CommandName = "taste")]
    public sealed class TasteCommand : ICommand
    {
        public int Port { get; private set; }

        private OptionSet GetSettings()
        {
            var settings = new OptionSet
            {
                { "p|port=", "The server port number.",
                v => Port = int.Parse(v) }
            };
            return settings;
        }

        public void Execute(string[] arguments)
        {
            GetSettings().Parse(arguments);
            Console.WriteLine("Port: " + Port);
        }

        public void WriteHelp(TextWriter writer)
        {
            var settings = GetSettings();
            settings.WriteOptionDescriptions(writer);
        }
    }
}
