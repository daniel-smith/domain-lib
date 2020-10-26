using System;
using DomainLib.Aggregates;

namespace DomainLib.Persistence
{
    public interface IEventSerializer
    {
        IEventPersistenceData GetPersistenceData(object @event, string eventName);
        TEvent DeserializeEvent<TEvent>(byte[] eventData, string eventName);
        void RegisterEventTypeMappings(IEventNameMapping nameMapping);
        Type GetClrTypeForEventName(string eventName);
        string GetEventNameForClrType(Type clrType);
    }
}