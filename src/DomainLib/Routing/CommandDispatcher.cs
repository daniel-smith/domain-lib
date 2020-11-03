using System;
using System.Linq;
using DomainLib.Aggregates;

namespace DomainLib.Routing
{
    public class CommandDispatcher<TCommandBase, TEventBase>
    {
        private readonly CommandRoutes<TCommandBase, TEventBase> _routes;
        private readonly EventDispatcher<TEventBase> _eventDispatcher;

        public CommandDispatcher(CommandRoutes<TCommandBase, TEventBase> commandRoutes, EventDispatcher<TEventBase> eventDispatcher)
        {
            _routes = commandRoutes ?? throw new ArgumentNullException(nameof(commandRoutes));
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        }

        public ICommandResult<TAggregate, TEventBase> Dispatch<TAggregate>(TAggregate aggregateRoot, TCommandBase command)
        {
            var commandType = command.GetType();
            var aggregateRootType = aggregateRoot.GetType();
            var aggregateAndCommandTypes = (aggregateRootType, commandType);
            var currentState = aggregateRoot;

            if (_routes.TryGetValue(aggregateAndCommandTypes, out var applyCommand))
            {
                var initialContext = CommandExecutionContext.Create(_eventDispatcher, aggregateRoot);

                var result = applyCommand(() => currentState, command)
                    .Aggregate(initialContext, (context, @event) =>
                    {
                        var newContext = context.ApplyEvent(@event);
                        currentState = newContext.Result.NewState;

                        return newContext;
                    });

                return result.Result;
            }

            var message = $"No route found when attempting to apply command " +
                          $"{commandType.Name} to {aggregateRootType.Name}";
            throw new InvalidOperationException(message);
        }
    }
}