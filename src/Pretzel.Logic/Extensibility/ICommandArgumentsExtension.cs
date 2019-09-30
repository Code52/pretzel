using System;
using System.Collections.Generic;
using System.CommandLine;

namespace Pretzel.Logic.Extensibility
{
    public interface ICommandArgumentsExtension
    {
        void UpdateOptions(IList<Option> options);

        void BindingCompleted();
    }
}
