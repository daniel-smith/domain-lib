using System;
using DomainLib.Aggregates;

namespace Shopping.Domain.Events
{
    public class ShoppingCartCreated : IDomainEvent
    {
        public const string EventName = "ShoppingCartCreated";

        public ShoppingCartCreated(Guid id)
        {
            Id = id;
        }

        string INamedEvent.EventName { get; } = EventName;
        public Guid Id { get; }
        
    }
}