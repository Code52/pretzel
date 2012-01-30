using Xunit;

namespace Pretzel.Tests
{
    public class SampleClass { }

    public class SampleTest : SpecificationFor<SampleClass>
    {
        public override SampleClass Given()
        {
            return new SampleClass();
        }

        public override void When()
        {
            Assert.True(true);
        }
    }
}
