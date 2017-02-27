namespace Inforigami.Regalo.Messaging
{
    public interface INoHandlerFoundStrategy
    {
        void Invoke(object message);
    }
}