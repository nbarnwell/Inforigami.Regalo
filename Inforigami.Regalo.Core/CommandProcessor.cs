namespace Inforigami.Regalo.Core
{
    public class CommandProcessor : MessageProcessorBase, ICommandProcessor
    {
        public CommandProcessor(ILogger logger, INoHandlerFoundStrategyFactory noHandlerFoundStrategyFactory) 
            : base(logger, noHandlerFoundStrategyFactory)
        {
        }

        public void Process<TCommand>(TCommand command)
        {
            HandleMessage(command, typeof(ICommandHandler<>));
        }
    }
}
