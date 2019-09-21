using System;
using System.Composition;

namespace Pretzel.Commands
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CommandInfoAttribute : ExportAttribute
    {
        public string CommandName { get; set; }

        public string CommandDescription { get; set; }

        //We need the redundant CommandType cause we have no way to get the concrete type
        //before creating the export. We only want to create the command the user chose.
        //This way we can allow extention authors to specify custom arguments.
        public Type CommandType { get; set; }

        public CommandInfoAttribute() : base(typeof(ICommand))
        {
        }
    }
}
