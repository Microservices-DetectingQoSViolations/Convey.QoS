using App.Metrics;
using App.Metrics.Counter;

namespace Convey.QoS.Violation.Metrics
{
    public class QoSViolationMetricsRegistry : IQoSViolationMetricsRegistry
    {
        private readonly IMetrics _metrics;

        private static readonly CounterOptions CounterViolationOptions = new CounterOptions
        {
            Name = "QoS Violation",
            MeasurementUnit = Unit.Custom("QoSViolation")
        };

        public QoSViolationMetricsRegistry(IMetrics metrics)
        {
            _metrics = metrics;
        }

        public void IncrementQoSViolation(ViolationType violationType)
        {
            _metrics.Measure.Counter.Increment(CounterViolationOptions, violationType.ToString());
        }
    }
}
