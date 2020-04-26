namespace Convey.QoS.Violation.Options
{
    public class QoSTimeViolationOptions
    {
        public double CommandExceedingCoefficient { get; set; }
        public double QueryExceedingCoefficient { get; set; }
        public double EventExceedingCoefficient { get; set; }
    }
}
