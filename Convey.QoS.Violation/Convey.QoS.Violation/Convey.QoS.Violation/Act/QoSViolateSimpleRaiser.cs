using Microsoft.Extensions.Logging;

namespace Convey.QoS.Violation.Act
{
    public class QoSViolateSimpleRaiser : IQoSViolateRaiser
    {
        private readonly ILogger<IQoSViolateRaiser> _logger;

        public QoSViolateSimpleRaiser(ILogger<IQoSViolateRaiser> logger)
        {
            _logger = logger;
        }

        public void Raise(ViolationType violationType)
        {
            _logger.LogWarning($"QoSViolation {violationType} raised.");
        }
    }
}
