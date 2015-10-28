namespace Inforigami.Regalo.Interfaces
{
    public interface ICommand : IMessage
    {
        ICommandHeaders Headers { get; }
        void OverwriteHeaders(ICommandHeaders headers);
    }
}
