using System;
using System.Collections.Generic;

namespace DomainLib.Projections
{
    public sealed class EventProjectionMap : Dictionary<Type, IList<(Type, ProjectEvent)>>
    {
        public void AddProjectionFunc(Type eventType, Type projectionType, ProjectEvent projectionFunc)
        {
            if (eventType == null) throw new ArgumentNullException(nameof(eventType));
            if (projectionType == null) throw new ArgumentNullException(nameof(projectionType));
            if (projectionFunc == null) throw new ArgumentNullException(nameof(projectionFunc));

            if (TryGetValue(eventType, out var projectionFuncs))
            {
                projectionFuncs.Add((projectionType, projectionFunc));
            }
            else
            {
                var projectionsList = new List<(Type, ProjectEvent)> {(projectionType, projectionFunc)};
                Add(eventType, projectionsList);
            }
        }
    }
}