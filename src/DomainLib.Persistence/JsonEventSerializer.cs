using DomainLib.Aggregates;
using DomainLib.Persistence;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DomainLib.Serialization
{
    public class JsonEventSerializer : IEventSerializer
    {
        private readonly JsonSerializerOptions _options;
        private readonly IEventNameMapping _eventNameMappings = new EventNameMapping();

        public JsonEventSerializer()
        {
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                AllowTrailingCommas = false,
            };
        }

        public JsonEventSerializer(JsonSerializerOptions options)
        {
            _options = options;
        }

        public void RegisterConverter(JsonConverter customConverter)
        {
            _options.Converters.Add(customConverter);
        }

        public void RegisterEventTypeMappings(IEventNameMapping nameMapping)
        {
            _eventNameMappings.Merge(nameMapping);
        }

        public IEventPersistenceData GetPersistenceData(object @event)
        {
            var eventName = _eventNameMappings.GetEventNameForClrType(@event.GetType());
            return new JsonEventPersistenceData(Guid.NewGuid(), eventName, JsonSerializer.SerializeToUtf8Bytes(@event, _options), null);
        }

        public TEvent DeserializeEvent<TEvent>(byte[] eventData, string eventName)
        {
            var clrType = _eventNameMappings.GetClrTypeForEventName(eventName);

            var evt = JsonSerializer.Deserialize(eventData, clrType, _options);

            if (evt is TEvent @event)
            {
                return @event;
            }

            var runtTimeType = typeof(TEvent);
            throw new InvalidEventTypeException($"Cannot cast event of type {eventName} to {runtTimeType.FullName}", eventName, runtTimeType.FullName);
        }
    }
}