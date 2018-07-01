using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;

namespace Pretzel.Tests.Templating.Jekyll
{
    public abstract class BakingEnvironment<T> : SpecificationFor<T>
    {
        public MockFileSystem FileSystem { get; set; } = new MockFileSystem(new Dictionary<string, MockFileData>());
    }
}
