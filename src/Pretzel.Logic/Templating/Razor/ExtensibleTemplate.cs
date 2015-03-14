using Pretzel.Logic.Extensibility;
using RazorEngine.Templating;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Pretzel.Logic.Templating.Razor
{
    public interface IExtensibleTemplate
    {
        dynamic Filter { get; set; }

        dynamic Tag { get; set; }
    }

    public class ExtensibleTemplate<T> : TemplateBase<T>, IExtensibleTemplate
    {
        public dynamic Filter { get; set; }

        public dynamic Tag { get; set; }
    }

    public class ExtensibleActivator : IActivator
    {
        private readonly IActivator defaultActivator;
        private readonly ExtensibleProxy<IFilter> filters;
        private readonly ExtensibleProxy<ITag> tags;

        public ExtensibleActivator(IActivator defaultActivator, IEnumerable<IFilter> filters, IEnumerable<ITag> tags)
        {
            this.defaultActivator = defaultActivator;
            this.filters = new ExtensibleProxy<IFilter>(filters);
            this.tags = new ExtensibleProxy<ITag>(tags);
        }

        public ITemplate CreateInstance(InstanceContext context)
        {
            var template = defaultActivator.CreateInstance(context);

            var extTemplate = template as IExtensibleTemplate;
            if (extTemplate != null)
            {
                extTemplate.Filter = filters;
                extTemplate.Tag = tags;
            }

            return template;
        }
    }

    public class ExtensibleProxy<T> : DynamicObject where T : IName
    {
        private readonly Dictionary<string, MethodInfo> extensibleMethods;

        public ExtensibleProxy(IEnumerable<T> extensibleMethods)
        {
            this.extensibleMethods = extensibleMethods == null ?
                new Dictionary<string, MethodInfo>() :
                extensibleMethods.ToDictionary(
                    x => x.Name,
                    x => x.GetType().GetMethod(x.Name)
                );
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return extensibleMethods.Keys;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            MethodInfo extensibleMethod;
            if (extensibleMethods.TryGetValue(binder.Name, out extensibleMethod))
            {
                result = extensibleMethod.Invoke(null, args);
                return true;
            }

            result = null;
            return false;
        }
    }
}
