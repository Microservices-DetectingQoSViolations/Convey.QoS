using System;
using OpenTracing;

namespace Convey.QoS.Violation.Sampling
{
    public class QoSTrackingSampler : IQoSTrackingSampler
    {
        private readonly double _samplingRate;
        private readonly Random _random = new Random();

        private readonly ITracer _tracer;

        public QoSTrackingSampler(QoSTrackingOptions options, ITracer tracer)
        {
            _tracer = tracer;
            _samplingRate = options.SamplingRate;
        }

        public bool DoWork()
        {
            return _tracer.ActiveSpan is {} && _random.NextDouble() <= _samplingRate;
        }
    }
}
