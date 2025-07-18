// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Builder;

namespace Steeltoe.Security.Authorization.Certificate;

public static class CertificateApplicationBuilderExtensions
{
    /// <summary>
    /// Enables certificate and header forwarding, along with ASP.NET Core authentication and authorization middlewares.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="IApplicationBuilder" /> to configure.
    /// </param>
    /// <returns>
    /// The incoming <paramref name="builder" /> so that additional calls can be chained.
    /// </returns>
    public static IApplicationBuilder UseCertificateAuthorization(this IApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.UseForwardedHeaders();
        builder.UseCertificateForwarding();
        builder.UseAuthentication();
        builder.UseAuthorization();

        return builder;
    }
}
