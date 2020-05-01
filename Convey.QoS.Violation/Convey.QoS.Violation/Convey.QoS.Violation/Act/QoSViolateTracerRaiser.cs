using Convey.QoS.Violation.Metrics;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Tag;

namespace Convey.QoS.Violation.Act
{
    public class QoSViolateTracerRaiser : IQoSViolateRaiser
    {
        public static readonly StringTag Violation = new StringTag("violation");

        private readonly ITracer _tracer;
        private readonly ILogger<IQoSViolateRaiser> _logger;
        private readonly IQoSViolationMetricsRegistry _qoSViolationMetricsRegistry;

        public QoSViolateTracerRaiser(ITracer tracer, ILogger<IQoSViolateRaiser> logger, IQoSViolationMetricsRegistry qoSViolationMetricsRegistry)
        {
            _tracer = tracer;
            _logger = logger;
            _qoSViolationMetricsRegistry = qoSViolationMetricsRegistry;
        }

        public void Raise(ViolationType violationType)
        {
            RaiseInLogger(violationType);
            RaiseInTracer(violationType);
            RaiseAsMetric(violationType);
        }

        private void RaiseInLogger(ViolationType violationType)
        {
            _logger.LogWarning($"QoSViolation {violationType} raised.");
        }

        private void RaiseInTracer(ViolationType violationType)
        {
            var span = _tracer.ActiveSpan;
            if (span is null)
            {
                _logger.LogDebug("There is no active span in tracer.");
                return;
            }

            span.Log($"QoSViolation {violationType} raised.");
            span.SetTag(Violation, violationType.ToString());
        }

        private void RaiseAsMetric(ViolationType violationType)
        {
            _qoSViolationMetricsRegistry.IncrementQoSViolation(violationType);
        }
    }
}
