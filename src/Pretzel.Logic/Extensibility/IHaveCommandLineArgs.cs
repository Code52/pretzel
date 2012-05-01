using System.ComponentModel.Composition;
using NDesk.Options;

namespace Pretzel.Logic.Extensibility
{
    [InheritedExport]
    public interface IHaveCommandLineArgs
    {
        void UpdateOptions(OptionSet options);
    }
}