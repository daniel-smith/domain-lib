using System.Collections.Generic;

namespace DomainLib.Aggregates
{
    public delegate IEnumerable<TEvent> ExecuteCommand<in TAggregate, in TCommand, out TEvent>(
        TAggregate aggregate, TCommand command);
}