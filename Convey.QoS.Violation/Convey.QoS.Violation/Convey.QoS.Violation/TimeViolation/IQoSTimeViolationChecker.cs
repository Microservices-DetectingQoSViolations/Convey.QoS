using OpenTracing;
using System.Threading.Tasks;

namespace Convey.QoS.Violation.TimeViolation
{
    public interface IQoSTimeViolationChecker
    {
        IQoSTimeViolationChecker Build(ISpan span, string commandName);
        void Run();
        Task Analyze();
    }
}