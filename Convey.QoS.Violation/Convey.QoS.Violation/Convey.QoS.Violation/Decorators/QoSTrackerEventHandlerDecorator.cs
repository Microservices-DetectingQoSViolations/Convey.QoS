using Convey.CQRS.Events;
using Convey.Exceptions;
using Convey.QoS.Violation.Act;
using Convey.QoS.Violation.Extensions;
using Convey.QoS.Violation.Options;
using Convey.QoS.Violation.Sampling;
using Convey.QoS.Violation.TimeViolation;
using OpenTracing;
using System;
using System.Threading.Tasks;

namespace Convey.QoS.Violation.Decorators
{
    public class QoSTrackerEventHandlerDecorator<TEvent> : IEventHandler<TEvent>
        where TEvent : class, IEvent
    {
        private readonly IEventHandler<TEvent> _handler;
        private readonly ITracer _tracer;
        private readonly IQoSTrackingSampler _trackingSampler;
        private readonly IQoSTimeViolationChecker<TEvent> _qoSViolateChecker;
        private readonly IQoSViolateRaiser _qoSViolateRaiser;

        private readonly bool _withTracing;

        public QoSTrackerEventHandlerDecorator(IEventHandler<TEvent> handler, ITracer tracer,
            IQoSTrackingSampler trackingSampler, IQoSTimeViolationChecker<TEvent> qoSViolateChecker, IQoSViolateRaiser qoSViolateRaiser,
            QoSTrackingOptions trackingOptions)
        {
            _handler = handler;
            _tracer = tracer;
            _trackingSampler = trackingSampler;
            _qoSViolateChecker = qoSViolateChecker;
            _qoSViolateRaiser = qoSViolateRaiser;

            _withTracing = trackingOptions.EnabledTracing && tracer is { };
        }

        public async Task HandleAsync(TEvent @event)
        {
            if (!_trackingSampler.DoWork())
            {
                await _handler.HandleAsync(@event);
                return;
            }

            using var scope = _withTracing ? BuildScope(@event.GetEventName()) : null;

            _qoSViolateChecker.Run();

            try
            {
                await _handler.HandleAsync(@event);
            }
            catch (Exception exception)
            {
                switch (exception)
                {
                    case AppException _:
                        _qoSViolateRaiser.Raise(ViolationType.AmongServicesInconsistency);
                        break;
                }
                throw;
            }

            await _qoSViolateChecker.Analyze();
        }

        private IScope BuildScope(string eventName)
        {
            var scope = _tracer
                .BuildSpan($"handling {eventName}")
                .WithTag("message-type", "event");

            if (_tracer.ActiveSpan is { })
            {
                scope.AddReference(References.ChildOf, _tracer.ActiveSpan.Context);
            }

            return scope.StartActive(true);
        }
    }
}
