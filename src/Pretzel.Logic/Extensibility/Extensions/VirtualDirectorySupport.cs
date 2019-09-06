using System.Composition;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using NDesk.Options;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Logic.Extensibility.Extensions
{
    [Export(typeof(IHaveCommandLineArgs))]
    [Export(typeof(ITransform))]
    public class VirtualDirectorySupport : ITransform, IHaveCommandLineArgs
    {
        readonly IFileSystem fileSystem;

        [ImportingConstructor]
        public VirtualDirectorySupport(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public void Transform(SiteContext siteContext)
        {
            if (string.IsNullOrEmpty(VirtualDirectory)) return;

            var href = new Regex("href=\"(?<url>/.*?)\"", RegexOptions.Compiled);
            var src = new Regex("src=\"(?<url>/.*?)\"", RegexOptions.Compiled);
            var hrefReplacement = string.Format("href=\"/{0}${{url}}\"", VirtualDirectory);
            var srcReplacement = string.Format("src=\"/{0}${{url}}\"", VirtualDirectory);

            foreach (var page in siteContext.Pages.Where(p => p.OutputFile.EndsWith(".html") || p.OutputFile.EndsWith(".htm") || p.OutputFile.EndsWith(".css")))
            {
                var fileContents = fileSystem.File.ReadAllText(page.OutputFile);

                var processedContents = href.Replace(fileContents, hrefReplacement);
                processedContents = src.Replace(processedContents, srcReplacement);

                if (fileContents != processedContents)
                {
                    fileSystem.File.WriteAllText(page.OutputFile, processedContents);
                }
            }
        }

        public void UpdateOptions(OptionSet options)
        {
            options.Add("vDir=", "Rewrite url's to work inside the specified virtual directory", v => VirtualDirectory = v);
        }

        public string[] GetArguments(string command)
        {
            if (command == "bake" || command == "taste")
                return new[] { "-vDir" };

            return new string[0];
        }

        public string VirtualDirectory { get; set; }
    }
}
