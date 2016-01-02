using System.ComponentModel.Composition;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.Logic.Extensibility {
    [InheritedExport]
    public interface IBeforeProcessingTransform {
        void Transform(SiteContext context);
    }
}
