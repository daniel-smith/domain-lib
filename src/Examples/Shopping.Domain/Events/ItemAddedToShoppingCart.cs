using System;

namespace Shopping.Domain.Events
{
    public class ItemAddedToShoppingCart : IDomainEvent
    {
        public ItemAddedToShoppingCart(Guid id, string item)
        {
            Id = id;
            Item = item;
        }

        public string EventName { get; } = "ItemAddedToShoppingCart";
        public Guid Id { get; }
        public string Item { get; }
    }
}