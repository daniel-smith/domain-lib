using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DomainLib.Aggregates
{
    public class EventTypeMapping : IEventTypeMapping
    {
        private readonly Dictionary<string, Type> _eventTypeMappings;

        public EventTypeMapping()
        {
            _eventTypeMappings = new Dictionary<string, Type>();
        }

        public EventTypeMapping(IEnumerable<KeyValuePair<string, Type>> mappings)
        {
            _eventTypeMappings = new Dictionary<string, Type>(mappings);
        }

        public void RegisterEventType(string eventType, Type clrType)
        {
            _eventTypeMappings[eventType] = clrType;
        }

        public Type GetClrTypeForEvent(string eventType)
        {
            // TODO throw a better exception if the event type isn't found in the mappings
            return _eventTypeMappings[eventType];
        }

        public IEnumerable<KeyValuePair<string, Type>> GetMappings()
        {
            return _eventTypeMappings.ToImmutableDictionary();
        }

        public void Merge(IEventTypeMapping other, bool overwriteOnConflict = false)
        {
            foreach (var (key, value) in other.GetMappings())
            {
                if (!overwriteOnConflict)
                {
                    if (_eventTypeMappings.ContainsKey(key) && _eventTypeMappings[key] != value)
                    {
                        // TODO: Better exception
                        throw new InvalidOperationException("Conflict detected in event mappings");
                    }
                }

                RegisterEventType(key, value);
            }
        }
    }
}