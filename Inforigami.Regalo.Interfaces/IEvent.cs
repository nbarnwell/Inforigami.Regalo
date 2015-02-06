namespace Inforigami.Regalo.Interfaces
{
    public interface IEvent : IMessage
    {
        IEventHeaders Headers { get; }
    }
}
