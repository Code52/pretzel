using System;
using System.Composition;

namespace Pretzel.Logic.Extensibility
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CommandArgumentsExtensionAttribute : ExportAttribute
    {
        public Type[] CommandTypes { get; set; }

        public CommandArgumentsExtensionAttribute() : base(typeof(ICommandArgumentsExtension))
        {
        }
    }
}
