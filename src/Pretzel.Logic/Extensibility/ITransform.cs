using Pretzel.Logic.Templating.Context;

namespace Pretzel.Logic.Extensibility
{
    public interface ITransform
    {
        void Transform(SiteContext siteContext);
    }
}
