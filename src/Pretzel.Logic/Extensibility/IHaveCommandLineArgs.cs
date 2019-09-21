using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Composition;

namespace Pretzel.Logic.Extensibility
{
    public interface IHaveCommandLineArgs
    {
        void UpdateOptions(IList<Option> options);

        void BindingCompleted();
    }
}
