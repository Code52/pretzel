using System.ComponentModel.Composition;
using Pretzel.Logic.Templating.Jekyll;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Logic.Templating
{
    [InheritedExport]
    public interface ISiteEngine
    {
        bool CanProcess(string directory);
        void Initialize();
        void Process(SiteContext context);
    }
}