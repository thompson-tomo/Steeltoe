// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Steeltoe.Management.Endpoint.Actuators.CloudFoundry;

public static class EndpointApplicationBuilderExtensions
{
    /// <summary>
    /// Adds a middleware that provides Cloud Foundry security.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="IApplicationBuilder" /> to configure.
    /// </param>
    /// <returns>
    /// The incoming <paramref name="builder" /> so that additional calls can be chained.
    /// </returns>
    public static IApplicationBuilder UseCloudFoundrySecurity(this IApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (builder.ApplicationServices.GetService<PermissionsProvider>() == null)
        {
            throw new InvalidOperationException(
                $"Please call {nameof(IServiceCollection)}.{nameof(EndpointServiceCollectionExtensions.AddCloudFoundryActuator)} first.");
        }

        builder.UseMiddleware<CloudFoundrySecurityMiddleware>();

        var marker = builder.ApplicationServices.GetRequiredService<HasCloudFoundrySecurityMiddlewareMarker>();
        marker.Value = true;

        return builder;
    }
}
