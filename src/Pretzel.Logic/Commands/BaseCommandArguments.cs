using Pretzel.Logic.Extensibility;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Composition;
using System.Linq;

namespace Pretzel.Logic.Commands
{
    public abstract class BaseCommandArguments : ICommandArguments
    {
        [Export]
        public IList<Option> Options { get; set; }
        public IList<ICommandArgumentsExtension> Extensions { get; } = new List<ICommandArgumentsExtension>();

        [OnImportsSatisfied]
        internal void OnImportsSatisfied()
        {
            var options = new List<Option>();
            Options = options;
            options.AddRange(CreateOptions());
        }

        protected abstract IEnumerable<Option> CreateOptions();

        public virtual void BindingCompleted()
        {
        }
    }
}
