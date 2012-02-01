using System.IO.Abstractions;
using Pretzel.Logic.Templating.Jekyll;

namespace Pretzel.Logic.Templating
{
    public interface ISiteEngine
    {
        void Initialize(IFileSystem fileSystem, SiteContext context);
        void Process();
    }
}