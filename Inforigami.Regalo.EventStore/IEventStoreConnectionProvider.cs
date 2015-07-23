using EventStore.ClientAPI;

namespace Inforigami.Regalo.EventStore
{
    public interface IEventStoreConnectionProvider
    {
        IEventStoreConnection GetConnection();
    }
}