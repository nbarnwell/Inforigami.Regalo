namespace Inforigami.Regalo.Interfaces
{
    public interface ICommand : IMessage
    {
        int TargetVersion { get; set; }
    }
}
