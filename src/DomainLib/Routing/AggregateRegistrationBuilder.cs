namespace DomainLib.Routing
{
    public class AggregateRegistrationBuilder<TAggregate, TCommandBase, TEventBase>
    {
        private readonly MessageRegistry<TCommandBase, TEventBase> _messageRegistry;

        public AggregateRegistrationBuilder(MessageRegistry<TCommandBase, TEventBase> messageRegistry)
        {
            _messageRegistry = messageRegistry;
        }

        public CommandRegistrationBuilder<TAggregate, TCommandBase, TCommand, TEventBase> Command<TCommand>() where TCommand : TCommandBase
        {
            return new CommandRegistrationBuilder<TAggregate, TCommandBase, TCommand, TEventBase>(_messageRegistry);
        }

        public EventRegistrationBuilder<TAggregate, TCommandBase, TEventBase, TEvent> Event<TEvent>() where TEvent : TEventBase
        {
            return new EventRegistrationBuilder<TAggregate, TCommandBase, TEventBase, TEvent>(_messageRegistry);
        }
    }
}