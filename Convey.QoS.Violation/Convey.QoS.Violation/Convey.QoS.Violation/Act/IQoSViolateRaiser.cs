namespace Convey.QoS.Violation.Act
{
    public interface IQoSViolateRaiser
    {
        void Raise(ViolationType violateType);
    }
}
