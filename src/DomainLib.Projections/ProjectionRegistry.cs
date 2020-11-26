using System;
using System.Collections.Generic;
using DomainLib.Aggregates;

namespace DomainLib.Projections
{
    public class ProjectionRegistry
    {
        public ProjectionRegistry(EventProjectionMap eventProjectionMap, Dictionary<Type, IContext> eventContextMap, IEventNameMap eventNameMap)
        {
            EventProjectionMap = eventProjectionMap;
            EventContextMap = eventContextMap;
            EventNameMap = eventNameMap;
        }

        public EventProjectionMap EventProjectionMap { get; }
        public IEventNameMap EventNameMap { get; }
        public Dictionary<Type, IContext> EventContextMap { get; }
    }
}