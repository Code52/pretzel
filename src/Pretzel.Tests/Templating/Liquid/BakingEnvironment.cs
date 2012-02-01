using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;

namespace Pretzel.Tests.Templating.Liquid
{
    public abstract class BakingEnvironment<T> : SpecificationFor<T>
    {
        private MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
        public MockFileSystem FileSystem
        {
            get { return fileSystem; }
            set { fileSystem = value; }
        }
    }
}
