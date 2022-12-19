using System;

namespace Inforigami.Regalo.EventSourcing
{
    public interface IFlushStrategy
    {
        void Execute();
    }

    public class FlushOnDemandStrategy : IFlushStrategy
    {
        private readonly IEventStore _eventStore;

        public FlushOnDemandStrategy(IEventStore eventStore)
        {
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        }

        public void Execute()
        {
            // Do nothing
        }

        public void Flush()
        {
            _eventStore.Flush();
        }
    }

    public class AutomaticFlushStrategy : IFlushStrategy
    {
        private readonly IEventStore _eventStore;

        public AutomaticFlushStrategy(IEventStore eventStore)
        {
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        }

        public void Execute()
        {
            _eventStore.Flush();
        }
    }

    public class NoFlushStrategy : IFlushStrategy
    {
        public void Execute()
        {
            // Do nothing; flushing of the event store will be done some other way
        }
    }
}