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

        private readonly IEventNameMapping _eventNameMappings = new EventNameMapping();

        public void Add<TDomainEvent>(Func<TAggregateRoot, TDomainEvent, TAggregateRoot> eventApplier)
            where TDomainEvent : TDomainEventBase
        {
            var route = KeyValuePair.Create<Type, ApplyEvent<TAggregateRoot, TDomainEventBase>>(
                typeof(TDomainEvent), (agg, e) => eventApplier(agg, (TDomainEvent) e));

            _routes.Add(route);
            _eventNameMappings.RegisterEvent<TDomainEvent>();
        }

        public ApplyEventRouter<TAggregateRoot, TDomainEventBase> Build()
        {
            return new ApplyEventRouter<TAggregateRoot, TDomainEventBase>(this, _eventNameMappings);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<Type, ApplyEvent<TAggregateRoot, TDomainEventBase>>> GetEnumerator()
        {
            return _routes.GetEnumerator();
        }
    }
}