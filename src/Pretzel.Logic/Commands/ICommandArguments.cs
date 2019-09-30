using Pretzel.Logic.Extensibility;
using System.Collections.Generic;
using System.CommandLine;

namespace Pretzel.Logic.Commands
{
    /// <summary>
    /// Specifies a command argument for a single command. Implementors should be decorated with the <see cref="CommandArgumentsAttribute"/>
    /// </summary>
    public interface ICommandArguments
    {
        /// <summary>
        /// A list of option that will be used as command line options
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        IList<Option> Options { get; }

        /// <summary>
        /// A list of extentions the command argument is extended with.
        /// </summary>
        /// <value>
        /// The extensions.
        /// </value>
        IList<ICommandArgumentsExtension> Extensions { get; }

        /// <summary>
        /// Once the command arguments are bound with values from the command line, this method gets called.
        /// Useful for validation or argument customization before being used by a command
        /// </summary>
        void BindingCompleted();
    }
}
