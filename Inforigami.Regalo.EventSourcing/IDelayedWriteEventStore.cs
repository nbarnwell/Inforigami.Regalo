namespace Inforigami.Regalo.EventSourcing
{
    public interface IDelayedWriteEventStore : IEventStore
    {
        void Flush();
    }
}
