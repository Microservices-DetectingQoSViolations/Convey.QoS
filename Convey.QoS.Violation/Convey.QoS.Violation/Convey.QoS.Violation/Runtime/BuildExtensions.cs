using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
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
                .WithThreadPoolSchedulingStats()
                .StartCollecting();

            builder.Services.AddPrometheusCounters();
            builder.Services.AddPrometheusAspNetCoreMetrics();
            builder.Services.AddPrometheusHttpClientMetrics();
            builder.Services.AddPrometheusGrpcClientMetrics();

            return builder;
        }

        public static IApplicationBuilder UseRuntimeMetrics(this IApplicationBuilder app)
        {
            return app.UseEndpoints(endpoints =>
                endpoints.MapMetrics("runtime_metrics"));
        }
    }
}
