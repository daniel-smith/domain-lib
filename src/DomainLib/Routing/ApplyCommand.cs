using System;
using System.Collections.Generic;

namespace DomainLib.Routing
{
    public delegate IEnumerable<TEvent> ApplyCommand<in TAggregate, in TCommand, out TEvent>(Func<TAggregate> aggregate, TCommand command);
}