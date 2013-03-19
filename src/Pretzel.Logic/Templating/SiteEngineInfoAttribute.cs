using System;
using System.ComponentModel.Composition;

namespace Pretzel.Logic.Templating
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class SiteEngineInfoAttribute : ExportAttribute
    {
        public string Engine { get; set; }

        public SiteEngineInfoAttribute() : base(typeof(ISiteEngine))
        {
        }
    }
}
