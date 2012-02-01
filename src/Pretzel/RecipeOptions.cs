using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NDesk.Options;

namespace Pretzel
{
    public class RecipeOptions
    {
        public RecipeOptions()
        {
            Path = Environment.CurrentDirectory;
        }

        public string Path { get; set; }
        public string Engine { get; set; }

        private static OptionSet GetSettings(BakeOptions options)
        {
            var settings = new OptionSet()
            {
                { "e|engine=", "The render engine",
                    v => options.Engine = v}
            };
            return settings;
        }

        public static BakeOptions Parse(string[] args)
        {
            var options = new BakeOptions();

            OptionSet settings = GetSettings(options);

            List<string> extra;
            try
            {
                extra = settings.Parse(args);
            }
            catch (OptionException)
            {
                return null;
            }

            if (extra.Any())
                options.Path = extra.First();

            return options;
        }

        public static void WriteHelp(TextWriter writer)
        {
            OptionSet settings = GetSettings(new BakeOptions());
            settings.WriteOptionDescriptions(writer);
        }
    }
}
