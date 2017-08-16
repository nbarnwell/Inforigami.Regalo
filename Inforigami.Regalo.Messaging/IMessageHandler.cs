namespace Inforigami.Regalo.Messaging
{
    public interface IMessageHandler<TMessage>
    {
        void Handle(TMessage message);
    }
}