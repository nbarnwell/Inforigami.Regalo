namespace Inforigami.Regalo.Interfaces
{
    public class EventHandlingSucceededEvent<TEvent> : Event, IEventHandlingSucceededEvent<TEvent>
    {
        public TEvent Evt { get; private set; }

        public EventHandlingSucceededEvent(TEvent evt)
        {
            Evt = evt;
        }
    }
}