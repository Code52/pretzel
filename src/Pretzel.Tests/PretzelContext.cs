namespace Pretzel.Tests
{
    public abstract class SpecificationFor<T>
    {
        public T Subject;

        public abstract T Given();
#pragma warning disable xUnit1013 //This is part of design, analyzer does not check for abstract methods correctly
        public abstract void When();
#pragma warning restore xUnit1013

        protected SpecificationFor()
        {
            Subject = Given();
            When();
        }
    }
}
