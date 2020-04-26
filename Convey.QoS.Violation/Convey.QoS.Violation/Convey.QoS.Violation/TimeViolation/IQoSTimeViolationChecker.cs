using System.Threading.Tasks;

namespace Convey.QoS.Violation.TimeViolation
{
    public interface IQoSTimeViolationChecker<TMessage>
    {
        void Run();
        Task Analyze();
    }
}