namespace Pretzel.Tests
{
    public abstract class SpecificationFor<T>
    {
        public T Subject;

        public abstract T Given();
        public abstract void When();

        protected SpecificationFor()
        {
#pragma warning disable S1699 // Constructors should only call non-overridable methods
            Subject = Given();
            When();
#pragma warning restore S1699 // Constructors should only call non-overridable methods
        }
    }
}
