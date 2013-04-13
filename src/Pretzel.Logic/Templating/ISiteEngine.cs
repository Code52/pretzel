using System.ComponentModel.Composition;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Logic.Templating
{
    [InheritedExport]
    public interface ISiteEngine
    {
        void Initialize();
        bool CanProcess(SiteContext context);
        void Process(SiteContext context, bool skipFileOnError = false);
        string GetOutputDirectory(string path);
    }
}