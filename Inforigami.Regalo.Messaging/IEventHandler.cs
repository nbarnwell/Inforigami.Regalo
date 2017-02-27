namespace Inforigami.Regalo.Messaging
{
    public interface IEventHandler<TEvent>
    {
        void Handle(TEvent evt);
    }
}
