using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DomainLib.Aggregates.Registration;

namespace DomainLib.Aggregates
{
    /// <summary>
    /// Dispatches commands to an aggregate root, where they are executed and produce an updated aggregate state
    /// along with one or more domain events representing the changes that have occurred
    /// </summary>
    public sealed class CommandDispatcher<TCommandBase, TEventBase>
    {
        private readonly CommandRegistrations<TCommandBase, TEventBase> _registrations;
        private readonly EventDispatcher<TEventBase> _eventDispatcher;

        internal CommandDispatcher(
            CommandRegistrations<TCommandBase, TEventBase> registrations, EventDispatcher<TEventBase> eventDispatcher)
        {
            _registrations = registrations ?? throw new ArgumentNullException(nameof(registrations));
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        }

        public IEnumerable<TEventBase> Dispatch<TAggregate>(TAggregate aggregateRoot, TCommandBase command)
        {
            var commandType = command.GetType();
            var aggregateRootType = aggregateRoot.GetType();
            var routeKey = (aggregateRootType, commandType);

            if (_registrations.Routes.TryGetValue(routeKey, out var executeCommand))
            {
                _registrations.PreCommandHook?.Invoke(command);

                var events = executeCommand(aggregateRoot, command).ToList();
                _eventDispatcher.Dispatch(aggregateRoot, events);
                _registrations.PostCommandHook?.Invoke(command);
                return events;
            }

            var message = $"No route found when attempting to apply command " +
                          $"{commandType.Name} to {aggregateRootType.Name}";
            throw new InvalidOperationException(message);
        }

        public (TAggregate, IReadOnlyList<TEventBase>) ImmutableDispatch<TAggregate>(
            TAggregate aggregateRoot, TCommandBase command)
        {
            var commandType = command.GetType();
            var aggregateRootType = aggregateRoot.GetType();
            var routeKey = (aggregateRootType, commandType);
            
            if (_registrations.ImmutableRoutes.TryGetValue(routeKey, out var executeCommand))
            {
                _registrations.PreCommandHook?.Invoke(command);

                var initialResult = (newState: aggregateRoot, events: ImmutableList<TEventBase>.Empty);
                var currentState = aggregateRoot;

                var finalResult = executeCommand(() => currentState, command)
                    .Aggregate(initialResult, (result, @event) =>
                    {
                        var (state, events) = result;
                        var newState = _eventDispatcher.Dispatch(state, @event);
                        var newEvents = events.Concat(Enumerable.Repeat(@event, 1)).ToImmutableList();
                        var newResult = (newState, events: newEvents);
                        currentState = newState;

                        return newResult;
                    });

                _registrations.PostCommandHook?.Invoke(command);

                return finalResult;
            }

            var message = $"No route found when attempting to apply command " +
                          $"{commandType.Name} to {aggregateRootType.Name}";
            throw new InvalidOperationException(message);
        }
    }
}