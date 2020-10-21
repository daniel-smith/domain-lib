namespace DomainLib.Aggregates
{
    public interface INamedEvent
    {
        string EventName { get; }
    }
}