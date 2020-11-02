using System;
using System.Collections.Generic;
using System.Linq;
using DomainLib.Aggregates;

namespace DomainLib
{
    public delegate IEnumerable<TEvent> ApplyCommand<in TAggregate, out TEvent, in TCommand>(TAggregate aggregate, TCommand command);

    public class CommandRegistry
    {
        private readonly Dictionary<(Type, Type), ApplyCommand<object, object, object>> _commandRoutes =
            new Dictionary<(Type, Type), ApplyCommand<object, object, object>>();

        public static CommandRegistry Instance { get; } = new CommandRegistry();

        public static void ForAggregate<TAggregate>(Action<AggregateCommandRegistryBuilder<TAggregate>> buildAggregateRegistration)
        {
            buildAggregateRegistration(new AggregateCommandRegistryBuilder<TAggregate>(Instance));
        }

        public static ICommandResult<TAggregate, TEvent> ExecuteCommand<TAggregate, TCommand, TEvent>(TAggregate aggregateRoot, TCommand command)
        {
            var commandType = command.GetType();
            var aggregateRootType = aggregateRoot.GetType();
            var aggregateAndCommandTypes = (aggregateRootType, commandType);

            if (Instance._commandRoutes.TryGetValue(aggregateAndCommandTypes, out var applyCommand))
            {
                var initialContext = CommandExecutionContext.Create<TAggregate, TEvent>(aggregateRoot);
                var result = applyCommand(aggregateRoot, command)
                    .Aggregate(initialContext, (context, @event) => context.ApplyEvent((TEvent) @event));

                return result.Result;
            }

            var message = $"No route found when attempting to apply command " +
                          $"{commandType.Name} to {aggregateRootType.Name}";
            throw new InvalidOperationException(message);
        }

        internal void RegisterCommandRoute<TAggregate, TCommand, TEvent>(ApplyCommand<TAggregate, TEvent, TCommand> applyCommand)
        {
            _commandRoutes.Add((typeof(TAggregate), typeof(TCommand)), (agg, cmd) => (IEnumerable<object>)applyCommand((TAggregate)agg, (TCommand)cmd));
        }
    }

    public class AggregateCommandRegistryBuilder<TAggregate>
    {
        private readonly CommandRegistry _commandRegistry;

        public AggregateCommandRegistryBuilder(CommandRegistry commandRegistry)
        {
            _commandRegistry = commandRegistry;
        }

        public CommandRegistrationBuilder<TAggregate, TCommand> Command<TCommand>()
        {
            return new CommandRegistrationBuilder<TAggregate, TCommand>(_commandRegistry);
        }
    }

    public class CommandRegistrationBuilder<TAggregate, TCommand>
    {
        private readonly CommandRegistry _commandRegistry;

        public CommandRegistrationBuilder(CommandRegistry commandRegistry)
        {
            _commandRegistry = commandRegistry;
        }

        public CommandRegistrationBuilder<TAggregate, TCommand> Executes<TEvent>(ApplyCommand<TAggregate, TEvent, TCommand> applyCommand)
        {
            _commandRegistry.RegisterCommandRoute(applyCommand);
            return this;
        }
    }
}