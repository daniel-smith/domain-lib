using System;
using System.Collections.Generic;
using DomainLib.Aggregates;

namespace DomainLib.Routing
{
    public class MessageRegistry
    {
        public static MessageRegistry<TCommandBase, TEventBase> Create<TCommandBase, TEventBase>()
        {
            return new MessageRegistry<TCommandBase, TEventBase>();
        }

        public static MessageRegistry<object, object> Create()
        {
            return Create<object, object>();
        }
    }

    public class MessageRegistry<TCommandBase, TEventBase>
    {
        private readonly CommandRegistrations<TCommandBase, TEventBase> _commandRegistrations = new CommandRegistrations<TCommandBase, TEventBase>();
        private readonly EventRoutes<TEventBase> _eventRoutes = new EventRoutes<TEventBase>();
        private readonly EventNameMap _eventNameMap = new EventNameMap();

        public IEventNameMap EventNameMap => _eventNameMap;

        public CommandDispatcher<TCommandBase, TEventBase> BuildCommandDispatcher()
        {
            return new CommandDispatcher<TCommandBase, TEventBase>(_commandRegistrations, BuildEventDispatcher());
        }

        public EventDispatcher<TEventBase> BuildEventDispatcher()
        {
            return new EventDispatcher<TEventBase>(_eventRoutes);
        }

        public void RegisterAggregate<TAggregate>(Action<AggregateRegistrationBuilder<TAggregate, TCommandBase, TEventBase>> buildAggregateRegistration)
        {
            if (buildAggregateRegistration == null) throw new ArgumentNullException(nameof(buildAggregateRegistration));
            buildAggregateRegistration(new AggregateRegistrationBuilder<TAggregate, TCommandBase, TEventBase>(this));
        }

        public void RegisterPreCommandHook(Action<TCommandBase> hook)
        {
            _commandRegistrations.PreCommandHook = hook;
        }

        public void RegisterPostCommandHook(Action<TCommandBase> hook)
        {
            _commandRegistrations.PostCommandHook = hook;
        }

        internal void RegisterCommandRoute<TAggregate, TCommand, TEvent>(ApplyCommand<TAggregate, TCommand, TEvent> applyCommand) where TCommand : TCommandBase
        {
            _commandRegistrations .Routes.Add((typeof(TAggregate), typeof(TCommand)), (agg, cmd) => (IEnumerable<TEventBase>) applyCommand(() => (TAggregate)agg(), (TCommand)cmd));
        }

        internal void RegisterEventRoute<TAggregate, TEvent>(ApplyEvent<TAggregate, TEvent> applyEvent) where TEvent: TEventBase
        {
            _eventRoutes.Add((typeof(TAggregate), typeof(TEvent)), (agg, e) => applyEvent((TAggregate)agg, (TEvent)e));
        }

        internal void RegisterEventName<TEvent>(string eventName)
        {
            _eventNameMap.RegisterEvent<TEvent>(eventName);
        }
    }
}