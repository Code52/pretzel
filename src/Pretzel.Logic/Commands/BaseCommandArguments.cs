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
        readonly List<Option> options = new List<Option>();
        [Export]
        public IList<Option> Options => options;
        public IList<ICommandArgumentsExtension> Extensions { get; } = new List<ICommandArgumentsExtension>();

        internal void BuildOptions()
        {
            options.AddRange(CreateOptions());
        }

        protected abstract IEnumerable<Option> CreateOptions();

        public virtual void BindingCompleted()
        {
        }
    }
}
