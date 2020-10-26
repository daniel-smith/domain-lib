using System;
using System.Collections.Generic;

namespace DomainLib.Aggregates
{
    public interface IEventNameMapping
    {
        void RegisterEvent<TEvent>(bool throwOnConflict = true);
        void RegisterEvent(Type clrType, bool throwOnConflict = true);
        Type GetClrTypeForEventName(string eventName);
        string GetEventNameForClrType(Type clrType);
        IEnumerable<KeyValuePair<string, Type>> GetNameToTypeMappings();
        void Merge(IEventNameMapping other, bool throwOnConflict = true);
    }
}