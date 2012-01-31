using System.Collections.Generic;
using System.IO;
using NDesk.Options;

namespace Pretzel
{
    public class TasteOptions
    {
        public int Port { get; set; }

        private static OptionSet GetSettings(TasteOptions options)
        {
            var settings = new OptionSet()
            {
                { "p|port=", "The server port number.",
                (int v) => options.Port = v }
            };
            return settings;
        }

        public static TasteOptions Parse(string[] args)
        {
            var options = new TasteOptions();

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

            return options;
        }

        public static void WriteHelp(TextWriter writer)
        {
            OptionSet settings = GetSettings(new TasteOptions());
            settings.WriteOptionDescriptions(writer);
        }
    }
}
