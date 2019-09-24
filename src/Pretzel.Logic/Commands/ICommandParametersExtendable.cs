using System;
using System.Composition;
using System.Linq;
using Pretzel.Logic.Extensibility;

namespace Pretzel.Logic.Commands
{
    public interface ICommandParametersExtendable
    {
        ExportFactory<IHaveCommandLineArgs, CommandArgumentsExtentionAttribute>[] ArgumentExtenders
        {
            get;
        }
    }
}
