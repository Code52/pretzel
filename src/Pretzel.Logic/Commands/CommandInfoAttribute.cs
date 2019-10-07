using System;
using System.Composition;

namespace Pretzel.Logic.Commands
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CommandInfoAttribute : ExportAttribute
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public Type ArgumentsType { get; set; }

        public CommandInfoAttribute() : base(typeof(ICommand))
        {
        }
    }
}
