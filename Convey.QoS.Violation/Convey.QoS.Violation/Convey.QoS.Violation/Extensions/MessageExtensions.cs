using System;
using Convey.CQRS.Commands;
using Convey.CQRS.Events;
using Convey.CQRS.Queries;
using System.Linq;
using Convey.QoS.Violation.TimeViolation;

namespace Convey.QoS.Violation.Extensions
{
    internal static class MessageExtensions
    {
        public static string GetCommandName<TCommand>(this TCommand command) where TCommand : class, ICommand
        {
            return ToUnderscoreCase("C" + command.GetType().Name);
        }

        public static string GetQueryName<TQuery>(this TQuery query) where TQuery : class, IQuery
        {
            return ToUnderscoreCase("Q" + query.GetType().Name);
        }

        public static string GetEventName<TEvent>(this TEvent @event) where TEvent : class, IEvent
        {
            return ToUnderscoreCase("E" + @event.GetType().Name);
        }

        public static string GetCommandName(Type command)
        {
            return ToUnderscoreCase("C" + command.Name);
        }

        public static string GetQueryName(Type query)
        {
            return ToUnderscoreCase("Q" + query.Name);
        }

        public static string GetEventName(Type @event)
        {
            return ToUnderscoreCase("E" + @event.Name);
        }

        public static string GetMessageName<TMessage>(this IQoSTimeViolationChecker<TMessage> violationChecker)
        {
            var type = typeof(TMessage);
            if (typeof(ICommand).IsAssignableFrom(type))
            {
                return GetCommandName(type);
            }
            if (typeof(IQuery).IsAssignableFrom(type))
            {
                return GetQueryName(type);
            }
            if (typeof(IEvent).IsAssignableFrom(type))
            {
                return GetEventName(type);
            }

            throw new ArgumentException($"Invalid message type {type}.");
        }

        public static string ToUnderscoreCase(this string str)
            => string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString()))
                .ToLowerInvariant();
    }
}
