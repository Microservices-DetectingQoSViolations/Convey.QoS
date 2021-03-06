﻿using Convey.CQRS.Commands;
using Convey.CQRS.Events;
using Convey.CQRS.Queries;
using Convey.QoS.Violation.Act;
using Convey.QoS.Violation.Cache;
using Convey.QoS.Violation.Decorators;
using Convey.QoS.Violation.Extensions;
using Convey.QoS.Violation.Options;
using Convey.QoS.Violation.Sampling;
using Convey.QoS.Violation.TimeViolation;
using Convey.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Convey.QoS.Violation.Metrics;
using Convey.QoS.Violation.Runtime;
using Jaeger;
using Jaeger.Reporters;
using Jaeger.Samplers;
using OpenTracing;

namespace Convey.QoS.Violation
{
    public static class BuildExtensions
    {
        private const string SectionName = "qoSTracking";

        private const long LongElapsedMilliseconds = 60000;

        public static IConveyBuilder AddQoSViolation(this IConveyBuilder builder)
        {
            return builder
                .AddQoSViolationHelpers()
                .AddQoSTrackingDecorators()
                .AddRuntimeMetrics();
        }

        public static IConveyBuilder AddQoSViolationHelpers(this IConveyBuilder builder)
        {
            var qoSTrackingOptions = builder.GetOptions<QoSTrackingOptions>(SectionName);

            if (!qoSTrackingOptions.Enabled)
            {
                return builder;
            }

            builder.Services.AddSingleton(qoSTrackingOptions);

            if (qoSTrackingOptions.EnabledTracing)
            {
                builder.Services.AddTransient<IQoSViolationMetricsRegistry, QoSViolationMetricsRegistry>();
                builder.Services.AddSingleton<IQoSViolateRaiser, QoSViolateTracerRaiser>();
            }
            else
            {
                builder.Services.AddSingleton<IQoSViolateRaiser, QoSViolateSimpleRaiser>();

                ITracer dummyTracer = new Tracer.Builder(Assembly.GetEntryAssembly().FullName)
                    .WithReporter(new NoopReporter())
                    .WithSampler(new ConstSampler(false))
                    .Build();
                builder.Services.AddSingleton(dummyTracer);
            }

            return builder;
        }

        public static IConveyBuilder AddQoSTrackingDecorators(this IConveyBuilder builder)
        {
            builder.Services.AddSingleton<IQoSTrackingSampler, QoSTrackingSampler>();
            builder.Services.AddSingleton<IQoSCacheFormatter, QoSCacheFormatter>();

            builder.Services.AddTransient(typeof(IQoSTimeViolationChecker<>), typeof(QoSTimeViolationChecker<>));

            builder.Services.TryDecorate(typeof(ICommandHandler<>), typeof(QoSTrackerCommandHandlerDecorator<>));
            builder.Services.TryDecorate(typeof(IEventHandler<>), typeof(QoSTrackerEventHandlerDecorator<>));
            builder.Services.TryDecorate(typeof(IQueryHandler<,>), typeof(QoSTrackerQueryHandlerDecorator<,>));

            return builder;
        }

        public static IApplicationBuilder UseQoS(this IApplicationBuilder app)
        {
            return app
                .UseQoSCache();
        }

        public static IApplicationBuilder UseQoSCache(
            this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var serviceName = scope.ServiceProvider.GetService<AppOptions>().Service;

            var commandHandlerType = typeof(ICommandHandler<>);
            var eventHandlerType = typeof(IEventHandler<>);
            var queryHandlerType = typeof(IQueryHandler<,>);

            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var allHandlerClasses = allAssemblies
                .SelectMany(s => s.GetTypes())
                .Where(type => !type.Name.Contains("Decorator")) // Remove Decorators for preventing duplicates
                .Select(type =>
                    type.GetInterfaces()
                        .FirstOrDefault(i =>
                            i.IsGenericType &&
                            (i.GetGenericTypeDefinition() == commandHandlerType
                            || i.GetGenericTypeDefinition() == eventHandlerType
                            || i.GetGenericTypeDefinition() == queryHandlerType)))
                .Where(type => type is { })
                .Select(type => new { HandlerType = type.Name[1], GenericTypeName = type.GenericTypeArguments[0].Name }) // Get first generic type of handlers (command/event/query type)
                .Select(type => serviceName.ToLower() + type.HandlerType + type.GenericTypeName) // Select command/event/query name
                .Distinct()
                .Select(name => name.ToUnderscoreCase());

            SetDefaultCacheValues(scope, allHandlerClasses);

            return app;
        }

        private static void SetDefaultCacheValues(IServiceScope scope, IEnumerable<string> allHandlersClasses)
        {
            var cache = scope.ServiceProvider.GetService<IDistributedCache>();
            var formatter = scope.ServiceProvider.GetService<IQoSCacheFormatter>();

            var serializedLongElapsedMilliseconds = formatter.SerializeNumber(LongElapsedMilliseconds);

            allHandlersClasses
                .ToList()
                .ForEach(key =>
                {
                    if (cache.Get(key) is { }) return;
                    cache.Set(key, serializedLongElapsedMilliseconds);
                });
        }
    }
}
