namespace DomainLib.Routing
{
    public class CommandRegistrationBuilder<TAggregate, TCommandBase, TCommand, TEventBase> where TCommand : TCommandBase
    {
        private readonly MessageRegistry<TCommandBase, TEventBase> _messageRegistry;

        public CommandRegistrationBuilder(MessageRegistry<TCommandBase, TEventBase> messageRegistry)
        {
            _messageRegistry = messageRegistry;
        }

        public CommandRegistrationBuilder<TAggregate, TCommandBase, TCommand, TEventBase> RoutesTo(ApplyCommand<TAggregate, TCommand, TEventBase> applyCommand)
        {
            _messageRegistry.RegisterCommandRoute(applyCommand);
            return this;
        }
    }
}