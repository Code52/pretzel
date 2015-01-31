using System.ComponentModel.Composition;
using System.Reflection;

namespace Pretzel.Logic.Extensions
{
    public interface IAssembly
    {
        string GetEntryAssemblyLocation();
    }

    [Export(typeof(IAssembly))]
    internal class AssemblyBase : IAssembly
    {
        public string GetEntryAssemblyLocation()
        {
            return Assembly.GetEntryAssembly().Location;
        }
    }
}
