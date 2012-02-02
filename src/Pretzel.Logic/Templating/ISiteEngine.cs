using System.ComponentModel.Composition;
using Pretzel.Logic.Templating.Jekyll;

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