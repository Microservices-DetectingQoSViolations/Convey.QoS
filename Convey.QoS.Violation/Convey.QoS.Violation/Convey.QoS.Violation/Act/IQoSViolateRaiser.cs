using OpenTracing;

namespace Convey.QoS.Violation.Act
{
    public interface IQoSViolateRaiser
    {
        bool ShouldRaiseTimeViolation(long handlingTime, long requiredHandlingTime);
        void Raise(ISpan span, ViolateType violateType);
    }
}
