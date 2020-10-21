using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using DomainLib.Persistence;

namespace DomainLib.Serialization
{
    public class JsonEventSerializer : IEventSerializer
    {
        private readonly JsonSerializerOptions _options;

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

        public IEventPersistenceData GetPersistenceData(object @event, string eventName)
        {
            return new JsonEventPersistenceData(Guid.NewGuid(), eventName, JsonSerializer.SerializeToUtf8Bytes(@event, _options), null);
        }
    }
}