using System.Collections.Generic;
using System.CommandLine;
using System.Composition;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Logic.Extensibility.Extensions
{
    [Export]
    [Shared]
    [CommandArgumentsExtension(CommandNames = new[] { BuiltInCommands.Bake, BuiltInCommands.Taste })]
    public class VirtualDirectorySupportArguments : ICommandArgumentsExtension
    {
        public void UpdateOptions(IList<Option> options)
        {
            options.Add(new Option(new [] { "-vDir", "--virtualdirectory"}, "Rewrite url's to work inside the specified virtual directory")
            {
                Argument = new Argument<string>()
            });
        }

        public void BindingCompleted()
        {
            //Not used
        }

        public string VirtualDirectory { get; set; }
    }

    [Export(typeof(ITransform))]
    public class VirtualDirectorySupport : ITransform
    {
        readonly IFileSystem fileSystem;

        [ImportingConstructor]
        public VirtualDirectorySupport(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        [Import]
        public VirtualDirectorySupportArguments Arguments { get; set; }

        public void Transform(SiteContext siteContext)
        {
            if (string.IsNullOrEmpty(Arguments.VirtualDirectory)) return;

            var href = new Regex("href=\"(?<url>/.*?)\"", RegexOptions.Compiled);
            var src = new Regex("src=\"(?<url>/.*?)\"", RegexOptions.Compiled);
            var hrefReplacement = string.Format("href=\"/{0}${{url}}\"", Arguments.VirtualDirectory);
            var srcReplacement = string.Format("src=\"/{0}${{url}}\"", Arguments.VirtualDirectory);

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
    }
}
