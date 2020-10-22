namespace DomainLib.Persistence
{
    public interface IEventSerializer
    {
        IEventPersistenceData GetPersistenceData(object @event, string eventName);
        TEvent DeserializeEvent<TEvent>(byte[] eventData, string eventName);
    }
}