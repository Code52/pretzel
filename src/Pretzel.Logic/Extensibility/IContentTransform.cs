using System.ComponentModel.Composition;

namespace Pretzel.Logic.Extensibility
{
    [InheritedExport]
    public interface IContentTransform
    {
        string Transform(string content);
    }
}