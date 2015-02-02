namespace Inforigami.Regalo.Interfaces
{
    public interface IEventHandlingSucceededEvent<out TEvent> : IEventHandlingResultEvent
    {
        TEvent Evt { get; }
    }
}
