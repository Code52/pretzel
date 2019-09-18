using Pretzel.Logic.Templating.Context;

namespace Pretzel.Logic.Extensibility
{
    public interface IBeforeProcessingTransform
    {
        void Transform(SiteContext context);
    }
}
