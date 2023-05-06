// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

namespace Steeltoe.Configuration.CloudFoundry.ServiceBinding;

internal sealed class RedisPostProcessor : CloudFoundryConfigurationPostProcessor
{
    internal const string BindingType = "redis";

    public override void PostProcessConfiguration(PostProcessorConfigurationProvider provider, IDictionary<string, string> configurationData)
    {
        if (!provider.IsBindingTypeEnabled(BindingType))
        {
            return;
        }

        foreach (string key in FilterKeys(configurationData, BindingType))
        {
            var mapper = ServiceBindingMapper.Create(configurationData, key, BindingType);

            // See Redis connection string parameters at: https://stackexchange.github.io/StackExchange.Redis/Configuration.html
            mapper.MapFromTo("credentials:host", "host");
            mapper.MapFromTo("credentials:port", "port");
            mapper.MapFromTo("credentials:password", "password");
        }
    }
}
