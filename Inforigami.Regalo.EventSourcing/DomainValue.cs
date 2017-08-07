namespace Inforigami.Regalo.EventSourcing
{
    public abstract class DomainValue<T>
    {
        public T Value { get; }

        protected DomainValue(T value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"{Value}";
        }
    }
}