using Inforigami.Regalo.EventSourcing;

namespace Inforigami.Regalo.Testing
{
    public interface IHandlerSetter<TEntity>
            where TEntity : AggregateRoot, new()
    {
        IGivenSetter<TEntity, THandler> HandledBy<THandler>(THandler handler);
    }
}
