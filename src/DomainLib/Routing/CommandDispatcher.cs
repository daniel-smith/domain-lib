using System;
using System.Linq;
using DomainLib.Aggregates;

namespace DomainLib.Routing
{
    public class CommandDispatcher<TCommandBase, TEventBase>
    {
        private readonly CommandRegistrations<TCommandBase, TEventBase> _registrations;
        private readonly EventDispatcher<TEventBase> _eventDispatcher;

        public CommandDispatcher(CommandRegistrations<TCommandBase, TEventBase> registrations, EventDispatcher<TEventBase> eventDispatcher)
        {
            _registrations = registrations ?? throw new ArgumentNullException(nameof(registrations));
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        }

        public ICommandResult<TAggregate, TEventBase> Dispatch<TAggregate>(TAggregate aggregateRoot, TCommandBase command)
        {
            var commandType = command.GetType();
            var aggregateRootType = aggregateRoot.GetType();
            var aggregateAndCommandTypes = (aggregateRootType, commandType);
            var currentState = aggregateRoot;

            if (_registrations.Routes.TryGetValue(aggregateAndCommandTypes, out var applyCommand))
            {
                _registrations.PreCommandHook?.Invoke(command);

                var initialContext = CommandExecutionContext.Create(_eventDispatcher, aggregateRoot);

                var result = applyCommand(() => currentState, command)
                    .Aggregate(initialContext, (context, @event) =>
                    {
                        var newContext = context.ApplyEvent(@event);
                        currentState = newContext.Result.NewState;

                        return newContext;
                    });

                _registrations.PostCommandHook?.Invoke(command);

                return result.Result;
            }

            var message = $"No route found when attempting to apply command " +
                          $"{commandType.Name} to {aggregateRootType.Name}";
            throw new InvalidOperationException(message);
        }
    }
}