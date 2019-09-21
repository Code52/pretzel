using System;
using System.Composition;

namespace Pretzel.Logic.Extensibility
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CommandArgumentsExtentionAttribute : ExportAttribute
    {
        public string[] CommandNames { get; set; }

        public CommandArgumentsExtentionAttribute() : base(typeof(IHaveCommandLineArgs))
        {
        }
    }
}
