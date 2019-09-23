using Pretzel.Logic.Extensibility;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Composition;
using System.Linq;

namespace Pretzel.Logic.Commands
{
    public interface ICommandParametersExtendable
    {
        ExportFactory<IHaveCommandLineArgs, CommandArgumentsExtentionAttribute>[] ArgumentExtenders
        {
            get;
        }
    }

    public abstract class BaseParameters : ICommandParameters, ICommandParametersExtendable
    {
        [ImportMany]
        public ExportFactory<IHaveCommandLineArgs, CommandArgumentsExtentionAttribute>[] ArgumentExtenders { get; set; }

        [Export]
        public IList<Option> Options { get; set; }

        [OnImportsSatisfied]
        internal void OnImportsSatisfied()
        {
            var options = new List<Option>();
            Options = options;
            WithOptions(options);

            foreach (var factory in this.GetCommandExtentions())
            {
                factory.CreateExport().Value.UpdateOptions(Options);
            }
        }

        protected abstract void WithOptions(List<Option> options);

        public virtual void BindingCompleted()
        {
        }
    }

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
