using Pretzel.Logic.Templating.Context;
using System.ComponentModel.Composition;

namespace Pretzel.Logic.Templating
{
    [InheritedExport]
    public interface ISiteEngine
    {
        void Initialize();

        bool CanProcess(SiteContext context);

        void Process(SiteContext context, bool skipFileOnError = false);
    }
}
