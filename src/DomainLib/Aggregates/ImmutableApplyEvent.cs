namespace DomainLib.Aggregates
{
    /// <summary>
    /// Applies a domain event to an aggregate root, and returns the updated state.
    /// </summary>
    public delegate TAggregate ImmutableApplyEvent<TAggregate, in TEvent>(TAggregate aggregate, TEvent @event);
}