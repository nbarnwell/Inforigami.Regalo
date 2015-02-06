namespace Inforigami.Regalo.Interfaces
{
    public abstract class Event : IEvent
    {
        public IEventHeaders Headers { get; private set; }

        protected Event()
        {
            Headers = new EventHeaders();
        }
    }
}