namespace Inforigami.Regalo.Core
{
    public interface INoHandlerFoundStrategy
    {
        void Invoke(object message);
    }
}