using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Prometheus;
using Prometheus.DotNetRuntime;

namespace Convey.QoS.Violation.Runtime
{
    public static class BuildExtensions
    {
        public static IConveyBuilder AddRuntimeMetrics(this IConveyBuilder builder)
        {
            DotNetRuntimeStatsBuilder
                .Customize()
                .WithGcStats()
                .WithThreadPoolStats()
                .StartCollecting();

            return builder;
        }

        public static IApplicationBuilder UseRuntimeMetrics(this IApplicationBuilder app)
        {
            return app.UseEndpoints(endpoints =>
                endpoints.MapMetrics("runtime_metrics"));
        }
    }
}
