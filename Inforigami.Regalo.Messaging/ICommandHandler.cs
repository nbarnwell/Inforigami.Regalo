namespace Inforigami.Regalo.Messaging
{
    public interface ICommandHandler<TCommand>
    {
        void Handle(TCommand command);
    }
}
