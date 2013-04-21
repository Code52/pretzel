using System;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Abstractions;
using NDesk.Options;
using Pretzel.Logic.Extensions;

namespace Pretzel.Logic.Extensibility.Extensions
{
    public class AzureHostSupport : IAdditionalIngredient, IHaveCommandLineArgs
    {
        private readonly IFileSystem fileSystem;
        private bool performAzureWorkaround;

        [ImportingConstructor]
        public AzureHostSupport(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public void UpdateOptions(OptionSet options)
        {
            options.Add("azure", "Enables deploy to azure support", v => performAzureWorkaround = (v!= null));
        }

        public string[] GetArguments(string command)
        {
            return command == "create" ? new[] {"-azure"} : new string[0];
        }

        public void MixIn(string directory)
        {
            if (!performAzureWorkaround) return;
            fileSystem.File.WriteAllText(Path.Combine(directory, @"Shim.cs"), Properties.RazorAzure.Shim);
            fileSystem.File.WriteAllText(Path.Combine(directory, @"Shim.csproj"), Properties.RazorAzure.ShimProject);
            fileSystem.File.WriteAllText(Path.Combine(directory, @"Shim.sln"), Properties.RazorAzure.ShimSolution);

            var currentPath = AppDomain.CurrentDomain.FriendlyName;
            var destination = Path.Combine(directory, "Pretzel.exe");
            if (!File.Exists(destination))
                File.Copy(currentPath, destination);

            Tracing.Info("Shim project added to allow deployment to azure websites");
        }
    }
}