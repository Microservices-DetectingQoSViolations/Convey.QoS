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

        public static string GetMessageName<TMessage>(this IQoSTimeViolationChecker<TMessage> violationChecker)
        {
            var message = Activator.CreateInstance<TMessage>();
            return message switch
            {
                ICommand command => command.GetCommandName(),
                IQuery query => query.GetQueryName(),
                IEvent @event => @event.GetEventName(),
                _ => throw new ArgumentException($"Invalid message type {typeof(TMessage)}.")
            };
        }

        public static string ToUnderscoreCase(this string str)
            => string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString()))
                .ToLowerInvariant();
    }
}
