using System;

namespace Shopping.Domain.Events
{
    public class ShoppingCartCreated : IDomainEvent
    {
        public ShoppingCartCreated(Guid id)
        {
            Id = id;
        }

        public string EventName { get; } = "ShoppingCartCreated";
        public Guid Id { get; }
    }
}