namespace Convey.QoS.Violation.Metrics
{
    public interface IQoSViolationMetricsRegistry
    {
        void IncrementQoSViolation(ViolationType violationType);
    }
}