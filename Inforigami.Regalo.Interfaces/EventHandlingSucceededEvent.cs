namespace Inforigami.Regalo.Interfaces
{
    public class EventHandlingSucceededEvent<TEvent> : IEventHandlingSucceededEvent<TEvent>
    {
        public IEventHeaders Headers { get; private set; }
        public TEvent Evt { get; private set; }

        public EventHandlingSucceededEvent(TEvent evt)
        {
            Evt = evt;
            Headers = new EventHeaders();
        }
    }
}