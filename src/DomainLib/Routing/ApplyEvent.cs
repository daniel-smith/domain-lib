namespace DomainLib.Routing
{
    public delegate TAggregate ApplyEvent<TAggregate, in TEvent>(TAggregate aggregate, TEvent @event);
}