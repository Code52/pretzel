using System;
using System.Composition;

namespace Pretzel.Logic.Commands
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CommandInfoAttribute : ExportAttribute
    {
        public string CommandName { get; set; }

        public string CommandDescription { get; set; }

        public Type CommandArgumentsType { get; set; }

        public CommandInfoAttribute() : base(typeof(ICommand))
        {
        }
    }
}
