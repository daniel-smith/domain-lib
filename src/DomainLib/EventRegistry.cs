using DomainLib.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DomainLib
{
    public delegate TAggregate ApplyEvent<TAggregate, in TEvent>(TAggregate aggregate, TEvent @event);
    

    public class EventRegistry
    {
        private readonly Dictionary<(Type, Type), ApplyEvent<object, object>> _eventRoutes = new Dictionary<(Type, Type), ApplyEvent<object, object>>();
        private readonly EventNameMap _eventNameMap = new EventNameMap();

        public static EventRegistry Instance { get; } = new EventRegistry();
        public IEventNameMap EventNameMap => _eventNameMap;

        public static void ForAggregate<TAggregate>(Action<AggregateEventRegistryBuilder<TAggregate>> buildAggregateRegistration)
        {
            buildAggregateRegistration(new AggregateEventRegistryBuilder<TAggregate>(Instance));
        }

        public static TAggregate RouteEvents<TAggregate, TEventBase>(TAggregate aggregateRoot, IEnumerable<TEventBase> events)
        {
            return events.Aggregate(aggregateRoot, RouteEvent);
        }

        public static TAggregate RouteEvent<TAggregate, TEvent>(TAggregate aggregateRoot, TEvent @event)
        {
            var eventType = @event.GetType();
            var aggregateRootType = aggregateRoot.GetType();
            var aggregateAndEventTypes = (aggregateRootType, eventType);

            if (Instance._eventRoutes.TryGetValue(aggregateAndEventTypes, out var applyEvent))
            {
                return (TAggregate)applyEvent(aggregateRoot, @event);
            }

            // If we get here, there is no explicit route specified for this event type.
            // Try and get a route to the event base type, i.e. a default route.
            //if (_routes.TryGetValue(typeof(TDomainEventBase), out var defaultApplyEvent))
            //{
            //    return defaultApplyEvent(aggregateRoot, @event);
            //}

            //// No default route specified.
            var message = $"No route or default route found when attempting to apply event " +
                          $"{eventType.Name} to {aggregateRootType.Name}";
            throw new InvalidOperationException(message);
        }
        
        internal void RegisterEventRoute<TAggregate, TEvent>(ApplyEvent<TAggregate, TEvent> applyEvent)
        {
            _eventRoutes.Add((typeof(TAggregate), typeof(TEvent)), (agg, e) => applyEvent((TAggregate)agg, (TEvent)e));
        }
        
        internal void RegisterEventName<TEvent>(string eventName)
        {
            _eventNameMap.RegisterEvent<TEvent>(eventName);
        }
    }

    public class AggregateEventRegistryBuilder<TAggregate>
    {
        private readonly EventRegistry _eventRegistry;

        public AggregateEventRegistryBuilder(EventRegistry eventRegistry)
        {
            _eventRegistry = eventRegistry;
        }

        public EventRegistrationBuilder<TAggregate, TEvent> Event<TEvent>()
        {
            return new EventRegistrationBuilder<TAggregate, TEvent>(_eventRegistry);
        }
    }

    public class EventRegistrationBuilder<TAggregate, TEvent>
    {
        private readonly EventRegistry _eventRegistry;

        public EventRegistrationBuilder(EventRegistry eventRegistry)
        {
            _eventRegistry = eventRegistry;
        }

        public EventRegistrationBuilder<TAggregate, TEvent> RouteTo(ApplyEvent<TAggregate, TEvent> applyEvent)
        {
            _eventRegistry.RegisterEventRoute(applyEvent);
            return this;
        }

        public EventRegistrationBuilder<TAggregate, TEvent> WithName(string name)
        {
            _eventRegistry.RegisterEventName<TEvent>(name);
            return this;
        }
    }

}