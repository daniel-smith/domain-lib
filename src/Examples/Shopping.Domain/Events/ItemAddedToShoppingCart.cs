using System;
using DomainLib.Aggregates;

namespace Shopping.Domain.Events
{
    public class ItemAddedToShoppingCart : IDomainEvent
    {
        public const string EventName = "ItemAddedToShoppingCart";

        public ItemAddedToShoppingCart(Guid id, string item)
        {
            Id = id;
            Item = item;
        }

        string INamedEvent.EventName { get; } = EventName;
        public Guid Id { get; }
        public string Item { get; }
    }
}