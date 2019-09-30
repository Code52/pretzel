using Pretzel.Logic.Extensibility;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Composition;
using System.Linq;

namespace Pretzel.Logic.Commands
{
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
            options.AddRange(CreateOptions());

            foreach (var factory in this.GetCommandExtentions())
            {
                factory.CreateExport().Value.UpdateOptions(Options);
            }
        }

        protected abstract IEnumerable<Option> CreateOptions();

        public virtual void BindingCompleted()
        {
        }
    }
}
