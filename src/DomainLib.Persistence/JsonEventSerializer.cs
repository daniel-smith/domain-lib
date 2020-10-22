using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using DomainLib.Aggregates;
using DomainLib.Persistence;

namespace DomainLib.Serialization
{
    public class JsonEventSerializer : IEventSerializer
    {
        private readonly JsonSerializerOptions _options;
        private readonly IEventTypeMapping _eventTypeMappings = new EventTypeMapping();

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

        public void RegisterEventTypeMappings(IEventTypeMapping typeMapping)
        {
            _eventTypeMappings.Merge(typeMapping);
        }

        public IEventPersistenceData GetPersistenceData(object @event, string eventName)
        {
            return new JsonEventPersistenceData(Guid.NewGuid(), eventName, JsonSerializer.SerializeToUtf8Bytes(@event, _options), null);
        }

        public TEvent DeserializeEvent<TEvent>(byte[] eventData, string eventName)
        {
            var clrType = _eventTypeMappings.GetClrTypeForEvent(eventName);

            var evt = JsonSerializer.Deserialize(eventData, clrType, _options);

            if (evt is TEvent @event)
            {
                return @event;
            }

            var runtTimeType = typeof(TEvent);
            throw new InvalidEventTypeException($"Cannot cast event of type {eventName} to {runtTimeType}", eventName, runtTimeType);
        }
    }
}