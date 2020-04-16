namespace Convey.QoS.Violation
{
    public class QoSTrackingOptions
    {
        public bool Enabled { get; set; }
        public double SamplingRate { get; set; }
        public int WindowComparerSize { get; set; }
    }
}
