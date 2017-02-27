namespace Inforigami.Regalo.Messaging
{
    public interface INoHandlerFoundStrategyFactory
    {
        INoHandlerFoundStrategy Create<TMessage>(TMessage message);
    }
}