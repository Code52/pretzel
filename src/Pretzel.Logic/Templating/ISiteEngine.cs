using Pretzel.Logic.Templating.Context;

namespace Pretzel.Logic.Templating
{
    public interface ISiteEngine
    {
        void Initialize();

        bool CanProcess(SiteContext context);

        void Process(SiteContext context, bool skipFileOnError = false);
    }
}
