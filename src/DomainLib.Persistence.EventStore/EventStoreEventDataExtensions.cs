using EventStore.ClientAPI;

namespace DomainLib.Persistence.EventStore
{
    public static class EventStoreEventDataExtensions
    {
        public static EventData ToEventData(this IEventSerializer @eventSerializer, object @event, string eventName)
        {
            var eventPersistenceData = eventSerializer.GetPersistenceData(@event, eventName);
            return new EventData(eventPersistenceData.EventId, eventPersistenceData.EventName, eventPersistenceData.IsJsonBytes, eventPersistenceData.EventData, eventPersistenceData.EventMetadata);
        }
    }
}