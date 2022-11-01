// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Steeltoe.Common;
using Steeltoe.Common.Kubernetes;
using Steeltoe.Configuration.Kubernetes;
using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Info;

namespace Steeltoe.Management.Kubernetes;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add an IInfoContributor that reports basic Kubernetes pod and host information.
    /// </summary>
    /// <param name="services">
    /// <see cref="IServiceCollection" />.
    /// </param>
    /// <param name="podUtilities">
    /// Bring your own <see cref="IPodUtilities" />. Defaults to <see cref="StandardPodUtilities" />.
    /// </param>
    public static IServiceCollection AddKubernetesInfoContributor(this IServiceCollection services, IPodUtilities podUtilities = null)
    {
        ArgumentGuard.NotNull(services);

        if (!services.Any(srv => srv.ServiceType.IsAssignableFrom(typeof(IPodUtilities))))
        {
            if (podUtilities == null)
            {
                services.AddKubernetesClient();
                services.AddSingleton<IPodUtilities, StandardPodUtilities>();
            }
            else
            {
                services.Add(new ServiceDescriptor(typeof(IPodUtilities), podUtilities));
            }
        }

        services.AddSingleton<IInfoContributor, KubernetesInfoContributor>();
        return services;
    }

    /// <summary>
    /// Add actuators that are useful when running in Kubernetes.
    /// </summary>
    /// <param name="services">
    /// <see cref="IServiceCollection" />.
    /// </param>
    /// <param name="configuration">
    /// Application configuration. Retrieved from the <see cref="IServiceCollection" /> if not provided.
    /// </param>
    /// <param name="podUtilities">
    /// Bring your own <see cref="IPodUtilities" />. Defaults to <see cref="StandardPodUtilities" />.
    /// </param>
    /// <param name="version">
    /// Set response type version.
    /// </param>
    public static IServiceCollection AddKubernetesActuators(this IServiceCollection services, IConfiguration configuration = null,
        IPodUtilities podUtilities = null, MediaTypeVersion version = MediaTypeVersion.V2)
    {
        ArgumentGuard.NotNull(services);

        services.AddKubernetesInfoContributor(podUtilities);
        services.AddAllActuators(configuration, version);
        return services;
    }
}