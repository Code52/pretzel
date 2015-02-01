using NDesk.Options;
using Pretzel.Logic.Extensions;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;

namespace Pretzel.Logic.Extensibility.Extensions
{
    public class AzureHostSupport : IAdditionalIngredient, IHaveCommandLineArgs
    {
        private readonly IFileSystem fileSystem;
        private readonly IAssembly assembly;
        private bool performAzureWorkaround;

        [ImportingConstructor]
        public AzureHostSupport(IFileSystem fileSystem, IAssembly assembly)
        {
            this.fileSystem = fileSystem;
            this.assembly = assembly;
        }

        public void UpdateOptions(OptionSet options)
        {
            options.Add("azure", "Enables deploy to azure support", v => performAzureWorkaround = (v != null));
        }

        public string[] GetArguments(string command)
        {
            return command == "create" ? new[] { "-azure" } : new string[0];
        }

        public void MixIn(string directory)
        {
            if (!performAzureWorkaround) return;
            // Move everything under the _source folder
            var sourceFolder = Path.Combine(directory, "_source");
            if (!fileSystem.Directory.Exists(sourceFolder))
            {
                fileSystem.Directory.CreateDirectory(sourceFolder);
            }

            foreach (var file in fileSystem.Directory.GetFiles(directory))
            {
                var trimStart = file.Replace(directory, string.Empty).TrimStart('/', '\\');
                fileSystem.File.Move(file, Path.Combine(sourceFolder, trimStart));
            }

            foreach (var directoryToMove in fileSystem.Directory.GetDirectories(directory).Where(n => new DirectoryInfo(n).Name != "_source"))
            {
                var trimStart = directoryToMove.Replace(directory, string.Empty).TrimStart('/', '\\');
                fileSystem.Directory.Move(directoryToMove, Path.Combine(sourceFolder, trimStart));
            }
            
            fileSystem.File.WriteAllText(Path.Combine(directory, @"Shim.cs"), Properties.RazorAzure.Shim);
            fileSystem.File.WriteAllText(Path.Combine(directory, @"Shim.csproj"), Properties.RazorAzure.ShimProject);
            fileSystem.File.WriteAllText(Path.Combine(directory, @"Shim.sln"), Properties.RazorAzure.ShimSolution);

            var currentPath = assembly.GetEntryAssemblyLocation();
            var destination = Path.Combine(directory, "Pretzel.exe");
            if (!fileSystem.File.Exists(destination))
            { 
                fileSystem.File.Copy(currentPath, destination);
            }

            Tracing.Info("Shim project added to allow deployment to azure websites");
        }
    }
}