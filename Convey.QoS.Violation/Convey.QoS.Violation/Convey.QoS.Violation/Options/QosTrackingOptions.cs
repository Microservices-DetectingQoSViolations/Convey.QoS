namespace Convey.QoS.Violation.Options
{
    public class QoSTrackingOptions
    {
        public bool Enabled { get; set; }
        public bool EnabledTracing { get; set; }
        public double SamplingRate { get; set; }
        public int WindowComparerSize { get; set; }

        public QoSTimeViolationOptions QoSTimeViolationOptions { get; set; }
    }
}
