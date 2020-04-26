using Convey.QoS.Violation.Options;
using System;

namespace Convey.QoS.Violation.Sampling
{
    public class QoSTrackingSampler : IQoSTrackingSampler
    {
        private readonly double _samplingRate;
        private readonly Random _random = new Random();

        public QoSTrackingSampler(QoSTrackingOptions options)
        {
            _samplingRate = options.SamplingRate;
        }

        public bool DoWork()
        {
            return _random.NextDouble() <= _samplingRate;
        }
    }
}
