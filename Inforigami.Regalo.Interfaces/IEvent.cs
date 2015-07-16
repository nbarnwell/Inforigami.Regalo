namespace Inforigami.Regalo.Interfaces
{
    public interface IEvent : IMessage
    {
        IEventHeaders Headers { get; }
        void OverwriteHeaders(IEventHeaders headers);
    }
}
