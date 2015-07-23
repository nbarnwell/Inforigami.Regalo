namespace Inforigami.Regalo.EventStore
{
    public interface IEventStoreConfiguration
    {
        EventStoreConnectionBehavior ConnectionBehavior { get; }
        string EventStoreEndpoints { get; }
    }
}