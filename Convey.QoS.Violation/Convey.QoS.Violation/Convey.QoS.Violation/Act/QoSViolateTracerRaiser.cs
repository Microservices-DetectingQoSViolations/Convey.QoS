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

        public QoSViolateTracerRaiser(ITracer tracer, ILogger<IQoSViolateRaiser> logger)
        {
            _tracer = tracer;
            _logger = logger;
        }

        public void Raise(ViolationType violationType)
        {
            RaiseInLogger(violationType);
            RaiseInTracer(violationType);
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
    }
}
