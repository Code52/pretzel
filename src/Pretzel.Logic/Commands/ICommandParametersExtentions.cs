using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Pretzel.Logic.Extensibility;

namespace Pretzel.Logic.Commands
{
    public static class ICommandParametersExtentions
    {
        public static IEnumerable<ExportFactory<IHaveCommandLineArgs, CommandArgumentsExtentionAttribute>> GetCommandExtentions(this ICommandParametersExtendable commandParametersExtendable)
        {
            var attr = commandParametersExtendable.GetType().GetCustomAttributes(typeof(CommandArgumentsAttribute), true).FirstOrDefault();

            if (attr is CommandArgumentsAttribute commandArgumentAttribute && commandParametersExtendable.ArgumentExtenders != null)
            {
                foreach (var factory in commandParametersExtendable.ArgumentExtenders)
                {
                    if (factory.Metadata.CommandNames.Contains(commandArgumentAttribute.CommandName))
                    {
                        yield return factory;
                    }
                }
            }
        }
    }
}
