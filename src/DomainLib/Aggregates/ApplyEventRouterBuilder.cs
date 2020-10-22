using System;
using System.Collections;
using System.Collections.Generic;

namespace DomainLib.Aggregates
{
    /// <summary>
    /// Mutable class for building an EventRouter.
    /// </summary>
    public class ApplyEventRouterBuilder<TAggregateRoot, TDomainEventBase> :
        IEnumerable<KeyValuePair<Type, ApplyEvent<TAggregateRoot, TDomainEventBase>>>
    {
        private readonly List<KeyValuePair<Type, ApplyEvent<TAggregateRoot, TDomainEventBase>>> _routes =
            new List<KeyValuePair<Type, ApplyEvent<TAggregateRoot, TDomainEventBase>>>();

        private readonly List<KeyValuePair<string, Type>> _eventTypeMappings = new List<KeyValuePair<string, Type>>();

        public void Add<TDomainEvent>(Func<TAggregateRoot, TDomainEvent, TAggregateRoot> eventApplier)
            where TDomainEvent : TDomainEventBase
        {
            var route = KeyValuePair.Create<Type, ApplyEvent<TAggregateRoot, TDomainEventBase>>(
                typeof(TDomainEvent), (agg, e) => eventApplier(agg, (TDomainEvent) e));

            _routes.Add(route);

            var eventName = GetEventName<TDomainEvent>();
            _eventTypeMappings.Add(KeyValuePair.Create(eventName, typeof(TDomainEvent)));
        }

        public ApplyEventRouter<TAggregateRoot, TDomainEventBase> Build()
        {
            return new ApplyEventRouter<TAggregateRoot, TDomainEventBase>(this, _eventTypeMappings);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<Type, ApplyEvent<TAggregateRoot, TDomainEventBase>>> GetEnumerator()
        {
            return _routes.GetEnumerator();
        }

        private static string GetEventName<TDomainEvent>()
        {
            // TODO: This is kind of hacky. Have a think if there are better ways to do this
            var eventName = typeof(TDomainEvent).GetField("EventName").GetValue(null) as string;

            if (!string.IsNullOrEmpty(eventName))
            {
                return eventName;
            }

            return typeof(TDomainEvent).Name;
        }
    }
}