namespace Inforigami.Regalo.Core
{
    public abstract class Command : Message, ICommand
    {
    }
    public interface ICommand : IMessage
    {
    }
}
