using System;
using System.Collections.Generic;
using System.CommandLine;

namespace Pretzel.Logic.Extensibility
{
    /// <summary>
    /// Specifies a command arguments extention. Implementors should be decorated with the <see cref="CommandArgumentsExtensionAttribute"/>
    /// </summary>
    public interface ICommandArgumentsExtension
    {
        /// <summary>
        /// A list of option that will be used as command line options
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        IList<Option> Options { get; }

        /// <summary>
        /// Once the command arguments are bound with values from the command line, this method gets called.
        /// Useful for validation or argument customization before being used by a command
        /// </summary>
        void BindingCompleted();
    }
}
