using System.ComponentModel.Composition;
using System.IO.Abstractions;
using Pretzel.Logic.Templating.Jekyll;

namespace Pretzel.Logic.Templating
{
    [InheritedExport]
    public interface ISiteEngine
    {
        bool CanProcess(IFileSystem fileSystem, string directory);
        void Initialize(IFileSystem fileSystem, SiteContext context);
        void Process();
    }
}