using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using NDesk.Options;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Logic.Commands
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class CommandParameters
    {
        readonly IEnumerable<IHaveCommandLineArgs> commandLineExtensions;

        [ImportingConstructor]
        public CommandParameters([ImportMany] IEnumerable<IHaveCommandLineArgs> commandLineExtensions)
        {
            this.commandLineExtensions = commandLineExtensions;
            GetDefaultValue("Port", s => decimal.TryParse(s, out port));

            Settings = new OptionSet
                                {
                                    { "t|template=", "The templating engine to use", v => Template = v },
                                    { "d|directory=", "The path to site directory", p => Path = p },
                                    { "p|port=", "The port to test the site locally", p => decimal.TryParse(p, out port) },
                                    { "i|import=", "The import type", v => ImportType = v }, // TODO: necessary?
                                    { "f|file=", "Path to import file", v => ImportPath = v },
                                };
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

        private OptionSet Settings { get; set; }

        public void Parse(IEnumerable<string> arguments)
        {
            // Allow extensions to register command line args
            foreach (var commandLineExtension in commandLineExtensions)
            {
                commandLineExtension.UpdateOptions(Settings);
            }

            var argumentList = arguments.ToArray();

            Settings.Parse(argumentList);

            var firstArgument = argumentList.FirstOrDefault();

            if (firstArgument != null && !firstArgument.StartsWith("-") && !firstArgument.StartsWith("/"))
            {
                Path = System.IO.Path.IsPathRooted(firstArgument) 
                    ? firstArgument
                    : System.IO.Path.Combine(Directory.GetCurrentDirectory(), firstArgument);
            }

            if (string.IsNullOrWhiteSpace(Path))
            {
                Path = Directory.GetCurrentDirectory();
            }
            else
            {
                Path = System.IO.Path.GetFullPath(Path);
            }
        }

        public void DetectFromDirectory(IDictionary<string, ISiteEngine> engines, SiteContext context)
        {
            foreach (var engine in engines)
            {
                if (!engine.Value.CanProcess(context)) continue;

                Tracing.Info(String.Format("Recommended engine for directory: '{0}'", engine.Key));
                Template = engine.Key;
                return;
            }

            if (Template == null)
                Template = "liquid";
        }

        public void WriteOptions(TextWriter writer, params string[] args)
        {
            if (args.Length == 0)
                Settings.WriteOptionDescriptions(writer);
            else
                WriteSubset(writer, args);
        }

        private void WriteSubset(TextWriter writer, string[] args)
        {
            var textWriter = new StringWriter();
            Settings.WriteOptionDescriptions(textWriter);
            var output = textWriter.ToString();

            foreach (var line in output.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries))
            {
                if (args.Any(line.Contains))
                {
                    writer.WriteLine(line);
                }
            }
        }
    }
}
