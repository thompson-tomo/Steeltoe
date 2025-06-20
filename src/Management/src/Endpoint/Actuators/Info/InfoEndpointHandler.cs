// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Steeltoe.Common;
using Steeltoe.Common.Extensions;
using Steeltoe.Management.Configuration;

namespace Steeltoe.Management.Endpoint.Actuators.Info;

internal sealed class InfoEndpointHandler : IInfoEndpointHandler
{
    private readonly IOptionsMonitor<InfoEndpointOptions> _optionsMonitor;
    private readonly IInfoContributor[] _contributors;
    private readonly ILogger<InfoEndpointHandler> _logger;

    public EndpointOptions Options => _optionsMonitor.CurrentValue;

    public InfoEndpointHandler(IOptionsMonitor<InfoEndpointOptions> optionsMonitor, IEnumerable<IInfoContributor> contributors, ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(optionsMonitor);
        ArgumentNullException.ThrowIfNull(contributors);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        IInfoContributor[] contributorArray = contributors.ToArray();
        ArgumentGuard.ElementsNotNull(contributorArray);

        _optionsMonitor = optionsMonitor;
        _contributors = contributorArray;
        _logger = loggerFactory.CreateLogger<InfoEndpointHandler>();
    }

    public async Task<IDictionary<string, object?>> InvokeAsync(object? argument, CancellationToken cancellationToken)
    {
        var builder = new InfoBuilder();

        foreach (IInfoContributor contributor in _contributors)
        {
            try
            {
                await contributor.ContributeAsync(builder, cancellationToken);
            }
            catch (Exception exception) when (!exception.IsCancellation())
            {
                _logger.LogWarning(exception, "Exception thrown by contributor '{ContributorTypeName}' while contributing to info endpoint.",
                    contributor.GetType());
            }
        }

        return builder.Build();
    }
}
