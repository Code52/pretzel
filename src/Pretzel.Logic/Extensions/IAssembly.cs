using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Pretzel.Logic.Extensions
{
    public interface IAssembly
    {
        string GetEntryAssemblyLocation();
    }

    [ExcludeFromCodeCoverage]
    [Export(typeof(IAssembly))]
    internal sealed class AssemblyWrapper : IAssembly
    {
        public string GetEntryAssemblyLocation()
        {
            return Assembly.GetEntryAssembly().Location;
        }
    }
}
