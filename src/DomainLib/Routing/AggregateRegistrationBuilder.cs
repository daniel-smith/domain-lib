using System;

namespace DomainLib.Routing
{
    public class AggregateRegistrationBuilder<TAggregate, TCommandBase, TEventBase>
    {
        private readonly AggregateRegistryBuilder<TCommandBase, TEventBase> _aggregateRegistryBuilder;

        public AggregateRegistrationBuilder(AggregateRegistryBuilder<TCommandBase, TEventBase> aggregateRegistryBuilder)
        {
            _aggregateRegistryBuilder = aggregateRegistryBuilder;
        }

        public CommandRegistrationBuilder<TAggregate, TCommandBase, TCommand, TEventBase> Command<TCommand>() where TCommand : TCommandBase
        {
            return new CommandRegistrationBuilder<TAggregate, TCommandBase, TCommand, TEventBase>(_aggregateRegistryBuilder);
        }

        public EventRegistrationBuilder<TAggregate, TCommandBase, TEventBase, TEvent> Event<TEvent>() where TEvent : TEventBase
        {
            return new EventRegistrationBuilder<TAggregate, TCommandBase, TEventBase, TEvent>(_aggregateRegistryBuilder);
        }

        public AggregateRegistrationBuilder<TAggregate, TCommandBase, TEventBase> PersistenceKey(Func<string, string> getPersistenceKey)
        {
            _aggregateRegistryBuilder.RegisterAggregateStreamName<TAggregate>(getPersistenceKey);
            return this;
        }

        public AggregateRegistrationBuilder<TAggregate, TCommandBase, TEventBase> SnapshotKey(Func<TAggregate, string> getSnapshotKey)
        {
            return this;
        }
    }
}