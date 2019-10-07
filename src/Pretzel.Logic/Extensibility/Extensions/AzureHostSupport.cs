using System.Collections.Generic;
using System.CommandLine;
using System.Composition;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Pretzel.Logic.Commands;
using Pretzel.Logic.Extensions;

namespace Pretzel.Logic.Extensibility.Extensions
{
    [Export]
    [Shared]
    [CommandArgumentsExtension(CommandArgumentTypes = new[] { typeof(RecipeCommandArguments) })]
    public class AzureHostSupportArguments : ICommandArgumentsExtension
    {
        public IList<Option> Options { get; } = new[]
        {
            new Option(new [] { "--azure", "-azure" }, "Enables deploy to azure support")
            {
                Argument = new Argument<bool>()
            }
        };

        public void BindingCompleted()
        {
            //Not used
        }
        public bool Azure { get; set; }
    }

    [Export(typeof(IAdditionalIngredient))]
    public class AzureHostSupport : IAdditionalIngredient
    {
        private readonly IFileSystem fileSystem;
        private readonly IAssembly assembly;

        [Import]
        public AzureHostSupportArguments Arguments { get; set; }

        [ImportingConstructor]
        public AzureHostSupport(IFileSystem fileSystem, IAssembly assembly)
        {
            this.fileSystem = fileSystem;
            this.assembly = assembly;
        }

        public void MixIn(string directory)
        {
            if (!Arguments.Azure) return;
            // Move everything under the _source folder
            var sourceFolder = Path.Combine(directory, "_source");
            if (!fileSystem.Directory.Exists(sourceFolder))
            {
                fileSystem.Directory.CreateDirectory(sourceFolder);
            }

            foreach (var file in fileSystem.Directory.GetFiles(directory))
            {
                var trimStart = file.Replace(directory, string.Empty).TrimStart(Path.DirectorySeparatorChar);
                fileSystem.File.Move(file, Path.Combine(sourceFolder, trimStart));
            }

            foreach (var directoryToMove in fileSystem.Directory.GetDirectories(directory).Where(n => new DirectoryInfo(n).Name != "_source"))
            {
                var trimStart = directoryToMove.Replace(directory, string.Empty).TrimStart(Path.DirectorySeparatorChar);
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
