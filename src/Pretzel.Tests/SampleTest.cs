using Xunit;

namespace Pretzel.Tests
{
    public class SampleTest
    {
        public class SampleClass { }

        public class For_A_Test_That_Does_Nothing : SpecificationFor<SampleClass>
        {
            public override SampleClass Given()
            {
                return new SampleClass();
            }

            public override void When()
            {
                // TODO: do something
            }

            [Fact]
            public void We_Always_Get_True()
            {
                Assert.True(true);
            }
        }
    }
}
