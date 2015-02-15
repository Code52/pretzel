using Pretzel.Logic.Extensibility;
using RazorEngine.Templating;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Pretzel.Logic.Templating.Razor
{
    public interface IExtensibleTemplate
    {
        dynamic Filter { get; set; }
    }

    public class ExtensibleTemplate<T> : TemplateBase<T>, IExtensibleTemplate
    {
        public dynamic Filter { get; set; }
    }

    public class ExtensibleActivator : IActivator
    {
        private readonly IActivator defaultActivator;
        private readonly IEnumerable<IFilter> filters;

        public ExtensibleActivator(IActivator defaultActivator, IEnumerable<IFilter> filters)
        {
            this.defaultActivator = defaultActivator;
            this.filters = filters;
        }

        public ITemplate CreateInstance(InstanceContext context)
        {
            var template = defaultActivator.CreateInstance(context);

            var extTemplate = template as IExtensibleTemplate;
            if (extTemplate != null)
            {
                extTemplate.Filter = new FilterProxy(filters);
            }

            return template;
        }
    }

    public class FilterProxy : DynamicObject
    {
        private readonly Dictionary<string, IFilter> filters;

        public FilterProxy(IEnumerable<IFilter> filters)
        {
            this.filters = filters == null ? new Dictionary<string, IFilter>() : filters.ToDictionary(x => x.Name);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return filters.Keys;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            IFilter filter;
            if (filters.TryGetValue(binder.Name, out filter))
            {
                var methodInfo = filter.GetType().GetMethod(filter.Name);
                result = methodInfo.Invoke(filter, args);
                return true;
            }

            result = null;
            return false;
        }
    }
}
