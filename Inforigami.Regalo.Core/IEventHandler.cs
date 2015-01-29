namespace Inforigami.Regalo.Core
{
    public interface IEventHandler<TEvent>
    {
        void Handle(TEvent evt);
    }
}
