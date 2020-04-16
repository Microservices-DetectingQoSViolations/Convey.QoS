using Convey.QoS.Violation.Act;
using Convey.QoS.Violation.Cache;
using Convey.Types;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using OpenTracing;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Convey.QoS.Violation.TimeViolation
{
    public class QoSTimeViolationChecker : IQoSTimeViolationChecker
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();

        private readonly IDistributedCache _distributedCache;
        private readonly IMemoryCache _memoryCache;
        private readonly IQoSCacheFormatter _qoSCacheFormatter;
        private readonly IQoSViolateRaiser _qoSViolateRaiser;

        private ISpan _span;

        private readonly int _windowComparerSize;
        private readonly string _appServiceName;
        private string _cacheMsgName;
        private string _msgName;

        private const long LongElapsedMilliseconds = 60000;

        public QoSTimeViolationChecker(IQoSCacheFormatter qoSCacheFormatter, IQoSViolateRaiser qoSViolateRaiser, 
            IDistributedCache distributedCache, IMemoryCache memoryCache, QoSTrackingOptions options, AppOptions appOptions)
        {
            _qoSCacheFormatter = qoSCacheFormatter;
            _qoSViolateRaiser = qoSViolateRaiser;
            _distributedCache = distributedCache;
            _memoryCache = memoryCache;
            _windowComparerSize = options.WindowComparerSize;
            _appServiceName = appOptions.Service;
        }

        public IQoSTimeViolationChecker Build(ISpan span, string msgName)
        {
            _span = span;
            _msgName = msgName;
            _cacheMsgName = $"{_appServiceName.ToLower()}_{_msgName}";

            return this;
        }

        public void Run()
        {
            _stopwatch.Start();
        }

        public async Task Analyze()
        {
            _stopwatch.Stop();
            var handlingTime = _stopwatch.ElapsedMilliseconds;
            try
            {
                var requiredHandlingTime = await GetRequiredTimeFromCache();

                if (RaiseTimeQoSViolationIfNeeded(handlingTime, requiredHandlingTime))
                {
                    // log Message

                    return;
                }

                await SetValuesInCache(handlingTime, requiredHandlingTime);
            }
            catch (Exception e)
            {
                // log msg
            }
        }

        private bool RaiseTimeQoSViolationIfNeeded(long handlingTime, long requiredHandlingTime)
        {
            var shouldRaise = _qoSViolateRaiser.ShouldRaiseTimeViolation(handlingTime, requiredHandlingTime);

            if (shouldRaise)
            {
                _qoSViolateRaiser.Raise(_span, ViolateType.HandlerTimeExceeded);
            }
            return shouldRaise;
        }

        private async Task<long> GetRequiredTimeFromCache()
        {
            var cacheValue = await _distributedCache.GetAsync(_cacheMsgName);

            return _qoSCacheFormatter.DeserializeNumber(cacheValue);

        }

        private Task<int> GetActualIdxFromCache()
            => _memoryCache.GetOrCreateAsync(GetIndexName(), cacheEntry => Task.FromResult(0));

        private Task<long[]> GetActualArrayFromCache()
            => _memoryCache.GetOrCreateAsync(GetArrayName(),
                cacheEntry => Task.FromResult(Enumerable.Repeat(LongElapsedMilliseconds, _windowComparerSize).ToArray()));

        private async Task SetValuesInCache(long handlingTime, long requiredHandlingTime)
        {
            var actualIdx = await GetActualIdxFromCache();
            var cachedArray = await GetActualArrayFromCache();

            var nextIndex = (actualIdx + 1) % _windowComparerSize;
            cachedArray[nextIndex] = handlingTime;

            _memoryCache.Set(GetArrayName(), cachedArray);
            _memoryCache.Set(GetIndexName(), nextIndex);

            if (nextIndex == 0)
            {
                var meanHandlerTime = cachedArray.Average();
                if (meanHandlerTime < requiredHandlingTime)
                {
                    await _distributedCache.SetAsync(_cacheMsgName,
                        _qoSCacheFormatter.SerializeNumber((long)meanHandlerTime));
                }
            }
        }

        private string GetArrayName()
            => _cacheMsgName + "_arr";
        private string GetIndexName()
            => _cacheMsgName + "_idx";
    }
}
