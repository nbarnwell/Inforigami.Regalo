namespace Inforigami.Regalo.Messaging
{
    public interface ICommandProcessor
    {
        void Process<TCommand>(TCommand command);
    }
}
