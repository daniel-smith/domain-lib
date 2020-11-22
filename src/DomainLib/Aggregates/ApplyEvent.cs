namespace DomainLib.Aggregates
{
    public delegate void ApplyEvent<in TAggregate, in TEvent>(TAggregate aggregate, TEvent @event);
}