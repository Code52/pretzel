using Pretzel.Logic.Extensions;
using Pretzel.Logic.Templating.Context;
using System.ComponentModel.Composition;

namespace Pretzel.Logic.Extensibility
{
    [InheritedExport]
    public abstract class TagFactoryBase : DotLiquid.ITagFactory
    {
        private readonly string _tageName;
        
        protected SiteContext SiteContext { get; private set; }

        public TagFactoryBase(string tagName)
        {
            _tageName = tagName.ToUnderscoreCase();
        }

        public string TagName
        {
            get
            {
                return _tageName;
            }
        }

        public DotLiquid.Tag Create()
        {
            return (DotLiquid.Tag)CreateTag();
        }

        public abstract ITag CreateTag();

        internal void Initialize(SiteContext siteContext)
        {
            SiteContext = siteContext;
        }
    }
}
