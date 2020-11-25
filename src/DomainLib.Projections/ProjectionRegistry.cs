using System;
using DomainLib.Aggregates;

namespace DomainLib.Projections
{
    public class ProjectionRegistry
    {
        public IEventNameMap EventNameMap { get; }
    }
}