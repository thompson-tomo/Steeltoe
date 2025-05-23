// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Steeltoe.Common;
using Steeltoe.Common.Extensions;
using Steeltoe.Common.Net;
using Steeltoe.Common.TestResources;
using Steeltoe.Discovery.Consul.Configuration;
using Steeltoe.Discovery.Consul.Registry;

namespace Steeltoe.Discovery.Consul.Test.Registry;

internal static class TestRegistrationFactory
{
    public static ConsulRegistration Create(IDictionary<string, string?> appSettings)
    {
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(appSettings).Build();

        var services = new ServiceCollection();
        services.AddSingleton(configuration);
        services.AddApplicationInstanceInfo();

        using ServiceProvider serviceProvider = services.BuildServiceProvider(true);
        var appInfo = serviceProvider.GetRequiredService<IApplicationInstanceInfo>();

        var options = new ConsulDiscoveryOptions();
        configuration.GetSection("consul:discovery").Bind(options);

        var domainNameResolverMock = new Mock<IDomainNameResolver>();
        var inetUtilsMock = new Mock<InetUtils>(domainNameResolverMock.Object, new TestOptionsMonitor<InetOptions>(), NullLogger<InetUtils>.Instance);
        var configurer = new PostConfigureConsulDiscoveryOptions(configuration, domainNameResolverMock.Object, inetUtilsMock.Object, appInfo);
        configurer.PostConfigure(null, options);

        TestOptionsMonitor<ConsulDiscoveryOptions> optionsMonitor = TestOptionsMonitor.Create(options);
        return ConsulRegistration.Create(optionsMonitor);
    }
}
