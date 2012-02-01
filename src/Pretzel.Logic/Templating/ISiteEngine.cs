using System.IO.Abstractions;
using Pretzel.Logic.Templating.Liquid;

namespace Pretzel.Logic.Templating
{
    public interface ISiteEngine
    {
        void Initialize(IFileSystem fileSystem, SiteContext context);
        void Process();
    }
}