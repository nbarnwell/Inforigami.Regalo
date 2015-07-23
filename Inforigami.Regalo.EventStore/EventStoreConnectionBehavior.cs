namespace Inforigami.Regalo.EventStore
{
    public enum EventStoreConnectionBehavior
    {
        NoClustering,
        ClusterWithDns,
        ClusterWithGossipSeeds
    }
}