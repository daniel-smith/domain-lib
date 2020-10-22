using System;
using System.Collections.Generic;

namespace DomainLib.Aggregates
{
    public interface IEventTypeMapping
    {
        void RegisterEventType(string eventType, Type clrType);
        Type GetClrTypeForEvent(string eventType);
        IEnumerable<KeyValuePair<string, Type>> GetMappings();
        void Merge(IEventTypeMapping other, bool overwriteOnConflict = false);
    }
}