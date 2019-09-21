using System.Collections.Generic;
using System.CommandLine;

namespace Pretzel.Logic.Extensibility
{
    public interface IHaveCommandLineArgs
    {
        void UpdateOptions(IList<Option> options);
    }
}
