using System;
using System.Collections.Generic;
using DomainLib.Aggregates;

namespace DomainLib.Routing
{
    public class AggregateRegistryBuilder
    {
        public static AggregateRegistryBuilder<TCommandBase, TEventBase> Create<TCommandBase, TEventBase>()
        {
            return new AggregateRegistryBuilder<TCommandBase, TEventBase>();
        }

        public static AggregateRegistryBuilder<object, object> Create()
        {
            return Create<object, object>();
        }
    }

    public class AggregateMetadataMap : Dictionary<Type, AggregateMetadata>
    {
        public AggregateMetadata GetForInstance<TAggregate>(TAggregate instance)
        {
            var aggregateType = instance.GetType();
            return GetForType(aggregateType);
        }

        public AggregateMetadata GetForType<TAggregate>()
        {
            return GetForType(typeof(TAggregate));
        }

        private AggregateMetadata GetForType(Type aggregateType)
        {
            if (TryGetValue(aggregateType, out var metadata))
            {
                return metadata;
            }

            var message = $"No metadata found for aggregate {aggregateType.Name}";
            throw new InvalidOperationException(message);
        }
    }

    public class AggregateRegistry<TCommandBase, TEventBase>
    {
        public CommandDispatcher<TCommandBase, TEventBase> CommandDispatcher { get; }
        public EventDispatcher<TEventBase> EventDispatcher;
        public IEventNameMap EventNameMap;
        public AggregateMetadataMap AggregateMetadataMap;

        public AggregateRegistry(CommandRegistrations<TCommandBase, TEventBase> commandRegistrations,
                                 EventRoutes<TEventBase> eventRoutes,
                                 IEventNameMap eventNameMap,
                                 AggregateMetadataMap aggregateMetadataMap)
        {
            EventDispatcher = new EventDispatcher<TEventBase>(eventRoutes);
            CommandDispatcher = new CommandDispatcher<TCommandBase, TEventBase>(commandRegistrations, EventDispatcher);
            EventNameMap = eventNameMap;
            AggregateMetadataMap = aggregateMetadataMap;
        }
    }
    
    public class AggregateRegistryBuilder<TCommandBase, TEventBase>
    {
        private readonly CommandRegistrations<TCommandBase, TEventBase> _commandRegistrations = new CommandRegistrations<TCommandBase, TEventBase>();
        private readonly EventRoutes<TEventBase> _eventRoutes = new EventRoutes<TEventBase>();
        private readonly EventNameMap _eventNameMap = new EventNameMap();
        private readonly AggregateMetadataMap _aggregateMetadataMap = new AggregateMetadataMap();

        public AggregateRegistry<TCommandBase, TEventBase> Build()
        {
            return new AggregateRegistry<TCommandBase, TEventBase>(_commandRegistrations, _eventRoutes, _eventNameMap, _aggregateMetadataMap);
        }
        
        public void Register<TAggregate>(Action<AggregateRegistrationBuilder<TAggregate, TCommandBase, TEventBase>> buildAggregateRegistration)
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
            _commandRegistrations.Routes.Add((typeof(TAggregate), typeof(TCommand)), (agg, cmd) => (IEnumerable<TEventBase>) applyCommand(() => (TAggregate)agg(), (TCommand)cmd));
        }

        internal void RegisterEventRoute<TAggregate, TEvent>(ApplyEvent<TAggregate, TEvent> applyEvent) where TEvent: TEventBase
        {
            _eventRoutes.Add((typeof(TAggregate), typeof(TEvent)), (agg, e) => applyEvent((TAggregate)agg, (TEvent)e));
        }

        internal void RegisterEventName<TEvent>(string eventName)
        {
            _eventNameMap.RegisterEvent<TEvent>(eventName);
        }

        internal void RegisterAggregateStreamName<TAggregate>(Func<string, string> getStreamName)
        {
            var aggregateType = typeof(TAggregate);
            if (!_aggregateMetadataMap.TryGetValue(aggregateType, out var aggregateMetadata))
            {
                aggregateMetadata = new AggregateMetadata();
                _aggregateMetadataMap.Add(aggregateType, aggregateMetadata);
            }

            aggregateMetadata.GetKeyFromIdentifier = getStreamName;
        }
    }
}