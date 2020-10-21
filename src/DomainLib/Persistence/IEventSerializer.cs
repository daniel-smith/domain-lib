namespace DomainLib.Persistence
{
    public interface IEventSerializer
    {
        IEventPersistenceData GetPersistenceData(object @event, string eventName);
    }
}