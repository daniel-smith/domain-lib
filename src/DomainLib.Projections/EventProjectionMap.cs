using System;
using System.Collections.Generic;

namespace DomainLib.Projections
{
    public class EventProjectionMap : Dictionary<Type, IList<(Type, ProjectEvent)>>
    {
        public IEnumerable<IContext> Contexts { get; }

        public void AddProjectionFunc(Type eventType, Type projectionType, ProjectEvent projectionFunc)
        {
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