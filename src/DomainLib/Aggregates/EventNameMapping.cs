using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DomainLib.Aggregates
{
    // ReSharper disable once InconsistentNaming
    public class EventNameMapping : IEventNameMapping
    {
        private readonly Dictionary<string, Type> _eventNameToTypeMappings;
        private readonly Dictionary<Type, string> _eventTypeToNameMappings;

        public EventNameMapping()
        {
            _eventNameToTypeMappings = new Dictionary<string, Type>();
            _eventTypeToNameMappings = new Dictionary<Type, string>();
        }
        
        public void RegisterEvent<TEvent>(bool throwOnConflict = true)
        {
            RegisterEvent(typeof(TEvent));
        }

        public void RegisterEvent(Type clrType, bool throwOnConflict = true)
        {
            var eventName = DetermineEventNameForClrType(clrType);
            RegisterEventName(eventName, clrType);
        }

        public Type GetClrTypeForEventName(string eventName)
        {
            if (_eventNameToTypeMappings.TryGetValue(eventName, out var clrType))
            {
                return clrType;
            }

            throw new UnknownEventNameException($"{eventName} could not be mapped to a CLR type", eventName);
        }

        public string GetEventNameForClrType(Type clrType)
        {
            return _eventTypeToNameMappings.TryGetValue(clrType, out var eventName)
                ? eventName
                : DetermineEventNameForClrType(clrType);
        }

        IEnumerable<KeyValuePair<string, Type>> IEventNameMapping.GetNameToTypeMappings()
        {
            return _eventNameToTypeMappings.ToImmutableDictionary();
        }

        public void Merge(IEventNameMapping other, bool throwOnConflict = true)
        {
            foreach (var (key, value) in other.GetNameToTypeMappings())
            {
                RegisterEventName(key, value, throwOnConflict);
            }
        }

        private static string DetermineEventNameForClrType(Type clrType)
        {
            // When determining the event name for a given type, we follow a three step process.
            // 1) If there is an EventNameAttribute on the type, we use the event name from that.
            // 2) If there is a public static field with the name 'EventName' on the type, use that
            // 3) Use the name of the type

            var eventNameAttribute = clrType
                                     .GetCustomAttributes(typeof(EventNameAttribute), true)
                                     .Cast<EventNameAttribute>()
                                     .ToList();

            // We shouldn't be able to get here as the attribute is restricted to prevent 
            // multiple uses. Defensive check to avoid any ambiguity 
            if (eventNameAttribute.Count > 1)
            {
                throw new InvalidOperationException($"Found more than one EventNameAttribute for type {clrType.FullName}");
            }

            if (eventNameAttribute.Count == 1)
            {
                return eventNameAttribute[0].EventName;
            }
            
            var eventName = clrType.GetField("EventName")?.GetValue(null) as string;
            
            return !string.IsNullOrEmpty(eventName) ? eventName : clrType.Name;
        }

        private void RegisterEventName(string eventName, Type clrType, bool throwOnConflict = true)
        {
            if (throwOnConflict && 
                _eventNameToTypeMappings.ContainsKey(eventName) &&
                _eventNameToTypeMappings[eventName] != clrType)
            {
                throw new InvalidOperationException($"Event {eventName} is already mapped " +
                                                    $"to type {_eventNameToTypeMappings[eventName].FullName}. " +
                                                    $"Cannot map to {clrType.FullName}");
            }

            if (throwOnConflict && 
                _eventTypeToNameMappings.ContainsKey(clrType) &&
                _eventTypeToNameMappings[clrType] != eventName)
            {
                throw new InvalidOperationException($"Type {clrType.FullName} is already mapped " +
                                                    $"to event {_eventTypeToNameMappings[clrType]}. " +
                                                    $"Cannot map to {_eventTypeToNameMappings[clrType]}");
            }

            _eventNameToTypeMappings[eventName] = clrType;
            _eventTypeToNameMappings[clrType] = eventName;
        }
    }
}