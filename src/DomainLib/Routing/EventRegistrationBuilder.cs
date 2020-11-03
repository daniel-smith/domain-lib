namespace DomainLib.Routing
{
    public class EventRegistrationBuilder<TAggregate, TCommandBase, TEventBase, TEvent> where TEvent : TEventBase
    {
        private readonly MessageRegistry<TCommandBase, TEventBase> _messageRegistry;

        public EventRegistrationBuilder(MessageRegistry<TCommandBase, TEventBase> messageRegistry)
        {
            _messageRegistry = messageRegistry;
        }

        public EventRegistrationBuilder<TAggregate, TCommandBase, TEventBase, TEvent> RoutesTo(ApplyEvent<TAggregate, TEvent> applyEvent)
        {
            _messageRegistry.RegisterEventRoute(applyEvent);
            return this;
        }

        public EventRegistrationBuilder<TAggregate, TCommandBase, TEventBase, TEvent> HasName(string name)
        {
            _messageRegistry.RegisterEventName<TEvent>(name);
            return this;
        }
    }
}