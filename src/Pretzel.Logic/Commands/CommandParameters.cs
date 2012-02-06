using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using NDesk.Options;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating;

namespace Pretzel.Logic.Commands
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CommandParameters
    {
        public CommandParameters()
        {
            GetDefaultValue("Port", s => decimal.TryParse(s, out port));
        }

        private void GetDefaultValue(string propertyName, Action<string> converter)
        {
            var attributes = TypeDescriptor.GetProperties(this)[propertyName].Attributes;
            var myAttribute = (DefaultValueAttribute) attributes[typeof (DefaultValueAttribute)];
            converter(myAttribute.Value.ToString());
        }

        public string Path { get; private set; }
        public string Template { get; set; }
        public string ImportPath { get; private set; }
        public string ImportType { get; set; }

        private decimal port;

        [DefaultValue(8080)]
        public decimal Port
        {
            get { return port; }
            set { port = value; }
        }

        private OptionSet Settings
        {
            get
            {
                return new OptionSet
                           {
                               { "t|template=", "The templating engine to use", v => Template = v },
                               { "d|directory=", "The path to site directory", p => Path = p },
                               { "p|port=", "The path to site directory", p => decimal.TryParse(p, out port) },
                               { "i|import=", "The import type", v => ImportType = v }, // TODO: necessary?
                               { "f|file=", "Path to import file", v => ImportPath = v },
                           };
            }
        }

        public void Parse(IEnumerable<string> arguments)
        {
            Settings.Parse(arguments);

            if (arguments.Count() == 1)
            {
                Path = arguments.First();
            }

            if (string.IsNullOrWhiteSpace(Path))
            {
                Path = Directory.GetCurrentDirectory();
            }
        }

        public void DetectFromDirectory(IDictionary<string, ISiteEngine> engines)
        {
            foreach (var engine in engines)
            {
                if (!engine.Value.CanProcess(Path)) continue;

                Tracing.Info(String.Format("Recommended engine for directory: '{0}'", engine.Key));
                Template = engine.Key;
                return;
            }
        }

        public void WriteOptions(TextWriter writer)
        {
            Settings.WriteOptionDescriptions(writer);
        }
    }
}
