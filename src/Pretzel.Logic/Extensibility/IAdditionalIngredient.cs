using System.ComponentModel.Composition;

namespace Pretzel.Logic.Extensibility
{
    [InheritedExport]
    public interface IAdditionalIngredient
    {
        void MixIn(string directory);
    }
}