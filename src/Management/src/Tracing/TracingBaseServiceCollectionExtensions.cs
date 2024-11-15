// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.Http;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Steeltoe.Common;
using Steeltoe.Common.Extensions;
using Steeltoe.Logging;
using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Wavefront.Exporters;
using B3Propagator = OpenTelemetry.Extensions.Propagators.B3Propagator;
using Sdk = OpenTelemetry.Sdk;

namespace Steeltoe.Management.Tracing;

public static class TracingBaseServiceCollectionExtensions
{
    private static readonly AssemblyLoader AssemblyLoader = new();

    /// <summary>
    /// Configure distributed tracing via OpenTelemetry with HttpClient Instrumentation.
    /// </summary>
    /// <param name="services">
    /// The <see cref="IServiceCollection" /> to add services to.
    /// </param>
    /// <returns>
    /// The incoming <paramref name="services" /> so that additional calls can be chained.
    /// </returns>
    public static IServiceCollection AddDistributedTracing(this IServiceCollection services)
    {
        return services.AddDistributedTracing(null);
    }

    /// <summary>
    /// Configure distributed tracing via OpenTelemetry with HttpClient Instrumentation.
    /// </summary>
    /// <param name="services">
    /// The <see cref="IServiceCollection" /> to add services to.
    /// </param>
    /// <param name="action">
    /// Customize the <see cref="TracerProviderBuilder" />.
    /// </param>
    /// <returns>
    /// The incoming <paramref name="services" /> so that additional calls can be chained.
    /// </returns>
    public static IServiceCollection AddDistributedTracing(this IServiceCollection services, Action<TracerProviderBuilder>? action)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddApplicationInstanceInfo();

        services.AddOptions();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<TracingOptions>, ConfigureTracingOptions>());

        services.ConfigureOptionsWithChangeTokenSource<WavefrontExporterOptions, ConfigureWavefrontExporterOptions>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IDynamicMessageProcessor, TracingLogProcessor>());

        bool exportToZipkin = AssemblyLoader.IsAssemblyLoaded("OpenTelemetry.Exporter.Zipkin");
        bool exportToOpenTelemetryProtocol = AssemblyLoader.IsAssemblyLoaded("OpenTelemetry.Exporter.OpenTelemetryProtocol");

        if (exportToZipkin)
        {
            ConfigureZipkinOptions(services);
        }

        if (exportToOpenTelemetryProtocol)
        {
            ConfigureOpenTelemetryProtocolOptions(services);
        }

        services.AddOpenTelemetry().WithTracing(tracerProviderBuilder =>
        {
            tracerProviderBuilder.AddHttpClientInstrumentation();

            if (exportToZipkin)
            {
                AddZipkinExporter(tracerProviderBuilder);
            }

            if (exportToOpenTelemetryProtocol)
            {
                AddOpenTelemetryProtocolExporter(tracerProviderBuilder);
            }

            action?.Invoke(tracerProviderBuilder);
        });

        services.AddOptions<HttpClientTraceInstrumentationOptions>().Configure<IOptionsMonitor<TracingOptions>>(
            (instrumentationOptions, tracingOptionsMonitor) =>
            {
                TracingOptions tracingOptions = tracingOptionsMonitor.CurrentValue;

                if (tracingOptions.EgressIgnorePattern != null)
                {
                    var pathMatcher = new Regex(tracingOptions.EgressIgnorePattern, RegexOptions.None, TimeSpan.FromSeconds(1));

                    instrumentationOptions.FilterHttpRequestMessage += requestMessage =>
                        !pathMatcher.IsMatch(requestMessage.RequestUri?.PathAndQuery ?? string.Empty);
                }
            });

        services.ConfigureOpenTelemetryTracerProvider((serviceProvider, tracerProviderBuilder) =>
        {
            var tracingOptionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<TracingOptions>>();
            TracingOptions tracingOptions = tracingOptionsMonitor.CurrentValue;

            ILogger logger = serviceProvider.GetRequiredService<ILoggerFactory>()
                .CreateLogger($"{typeof(TracingBaseServiceCollectionExtensions).Namespace}.Setup");

            logger.LogTrace("Found Zipkin exporter: {ExportToZipkin}. Found OTLP exporter: {ExportToOtlp}.", exportToZipkin, exportToOpenTelemetryProtocol);

            tracerProviderBuilder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(tracingOptions.Name!));

            if (string.Equals(tracingOptions.PropagationType, "B3", StringComparison.OrdinalIgnoreCase))
            {
                List<TextMapPropagator> propagators =
                [
                    new B3Propagator(tracingOptions.SingleB3Header),
                    new BaggagePropagator()
                ];

                Sdk.SetDefaultTextMapPropagator(new CompositeTextMapPropagator(propagators));
            }

            if (tracingOptions.NeverSample)
            {
                tracerProviderBuilder.SetSampler(new AlwaysOffSampler());
            }
            else if (tracingOptions.AlwaysSample)
            {
                tracerProviderBuilder.SetSampler(new AlwaysOnSampler());
            }

            AddWavefrontExporter(tracerProviderBuilder, serviceProvider);
        });

        return services;
    }

    private static void ConfigureZipkinOptions(IServiceCollection services)
    {
        services.AddOptions<ZipkinExporterOptions>().PostConfigure<IOptionsMonitor<TracingOptions>>((zipkinExporterOptions, tracingOptionsMonitor) =>
        {
            TracingOptions tracingOptions = tracingOptionsMonitor.CurrentValue;

            zipkinExporterOptions.UseShortTraceIds = tracingOptions.UseShortTraceIds;
            zipkinExporterOptions.MaxPayloadSizeInBytes = tracingOptions.MaxPayloadSizeInBytes;

            if (tracingOptions.ExporterEndpoint != null)
            {
                zipkinExporterOptions.Endpoint = tracingOptions.ExporterEndpoint;
            }
        });
    }

    private static void AddZipkinExporter(TracerProviderBuilder builder)
    {
        builder.AddZipkinExporter();
    }

    private static void ConfigureOpenTelemetryProtocolOptions(IServiceCollection services)
    {
        services.AddOptions<OtlpExporterOptions>().PostConfigure<IOptionsMonitor<TracingOptions>>((otlpExporterOptions, tracingOptionsMonitor) =>
        {
            TracingOptions tracingOptions = tracingOptionsMonitor.CurrentValue;

            if (tracingOptions.ExporterEndpoint != null)
            {
                otlpExporterOptions.Endpoint = tracingOptions.ExporterEndpoint;
            }
        });
    }

    private static void AddOpenTelemetryProtocolExporter(TracerProviderBuilder builder)
    {
        builder.AddOtlpExporter();
    }

    private static void AddWavefrontExporter(TracerProviderBuilder tracerProviderBuilder, IServiceProvider serviceProvider)
    {
        var wavefrontOptions = serviceProvider.GetRequiredService<IOptions<WavefrontExporterOptions>>();

        // Only add if wavefront is configured
        if (!string.IsNullOrEmpty(wavefrontOptions.Value.Uri))
        {
            var logger = serviceProvider.GetRequiredService<ILogger<WavefrontTraceExporter>>();
            tracerProviderBuilder.AddWavefrontTraceExporter(wavefrontOptions.Value, logger);
        }
    }
}
