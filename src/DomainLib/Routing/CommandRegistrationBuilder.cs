namespace DomainLib.Routing
{
    public class CommandRegistrationBuilder<TAggregate, TCommandBase, TCommand, TEventBase> where TCommand : TCommandBase
    {
        private readonly AggregateRegistryBuilder<TCommandBase, TEventBase> _aggregateRegistryBuilder;

        public CommandRegistrationBuilder(AggregateRegistryBuilder<TCommandBase, TEventBase> aggregateRegistryBuilder)
        {
            _aggregateRegistryBuilder = aggregateRegistryBuilder;
        }

        public CommandRegistrationBuilder<TAggregate, TCommandBase, TCommand, TEventBase> RoutesTo(ApplyCommand<TAggregate, TCommand, TEventBase> applyCommand)
        {
            _aggregateRegistryBuilder.RegisterCommandRoute(applyCommand);
            return this;
        }
    }
}