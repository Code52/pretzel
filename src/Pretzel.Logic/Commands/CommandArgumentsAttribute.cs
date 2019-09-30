using System;
using System.Composition;

namespace Pretzel.Logic.Commands
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CommandArgumentsAttribute : ExportAttribute
    {
        public string CommandName { get; set; }

        public CommandArgumentsAttribute() : base(typeof(ICommandArguments))
        {
        }
    }
}
