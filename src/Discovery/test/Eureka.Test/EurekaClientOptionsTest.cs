// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Steeltoe.Common.TestResources;
using Steeltoe.Common.TestResources.IO;
using Steeltoe.Discovery.Eureka.Configuration;

namespace Steeltoe.Discovery.Eureka.Test;

public sealed class EurekaClientOptionsTest
{
    [Fact]
    public void Constructor_Initializes_Defaults()
    {
        var clientOptions = new EurekaClientOptions();

        Assert.True(clientOptions.Enabled);
        Assert.Equal(30, clientOptions.RegistryFetchIntervalSeconds);
        Assert.Null(clientOptions.EurekaServer.ProxyHost);
        Assert.Equal(0, clientOptions.EurekaServer.ProxyPort);
        Assert.Null(clientOptions.EurekaServer.ProxyUserName);
        Assert.Null(clientOptions.EurekaServer.ProxyPassword);
        Assert.True(clientOptions.EurekaServer.ShouldGZipContent);
        Assert.Equal(EurekaServerOptions.DefaultConnectTimeoutSeconds, clientOptions.EurekaServer.ConnectTimeoutSeconds);
        Assert.True(clientOptions.ShouldRegisterWithEureka);
        Assert.False(clientOptions.IsFetchDeltaDisabled);
        Assert.True(clientOptions.ShouldFilterOnlyUpInstances);
        Assert.True(clientOptions.ShouldFetchRegistry);
        Assert.Null(clientOptions.RegistryRefreshSingleVipAddress);
        Assert.Equal("http://localhost:8761/eureka/", clientOptions.EurekaServerServiceUrls);
        Assert.NotNull(clientOptions.Health);
        Assert.True(clientOptions.Health.ContributorEnabled);
        Assert.False(clientOptions.Health.CheckEnabled);
        Assert.Null(clientOptions.Health.MonitoredApps);
    }

    [Fact]
    public void Constructor_ConfiguresEurekaDiscovery_Correctly()
    {
        const string appSettings = """
            {
              "eureka": {
                "client": {
                  "eurekaServer": {
                    "proxyHost": "proxyHost",
                    "proxyPort": 100,
                    "proxyUserName": "proxyUserName",
                    "proxyPassword": "proxyPassword",
                    "shouldGZipContent": true,
                    "connectTimeoutSeconds": 100
                  },
                  "allowRedirects": true,
                  "shouldDisableDelta": true,
                  "shouldFilterOnlyUpInstances": true,
                  "shouldFetchRegistry": true,
                  "registryRefreshSingleVipAddress": "registryRefreshSingleVipAddress",
                  "shouldRegisterWithEureka": true,
                  "registryFetchIntervalSeconds": 100,
                  "instanceInfoReplicationIntervalSeconds": 100,
                  "serviceUrl": "https://foo.bar:8761/eureka/",
                  "Health": {
                    "CheckEnabled": "true"
                  }
                },
                "instance": {
                  "registrationMethod": "foobar",
                  "hostName": "myHostName",
                  "instanceId": "instanceId",
                  "appName": "appName",
                  "appGroup": "appGroup",
                  "instanceEnabledOnInit": true,
                  "port": 100,
                  "securePort": 100,
                  "nonSecurePortEnabled": true,
                  "securePortEnabled": true,
                  "leaseExpirationDurationInSeconds": 100,
                  "leaseRenewalIntervalInSeconds": 100,
                  "secureVipAddress": "secureVipAddress",
                  "vipAddress": "vipAddress",
                  "asgName": "asgName",
                  "metadataMap": {
                    "foo": "bar",
                    "bar": "foo"
                  },
                  "statusPageUrlPath": "statusPageUrlPath",
                  "statusPageUrl": "statusPageUrl",
                  "homePageUrlPath": "homePageUrlPath",
                  "homePageUrl": "homePageUrl",
                  "healthCheckUrlPath": "healthCheckUrlPath",
                  "healthCheckUrl": "healthCheckUrl",
                  "secureHealthCheckUrl": "secureHealthCheckUrl"
                }
              }
            }
            """;

        using var sandbox = new Sandbox();
        string path = sandbox.CreateFile(MemoryFileProvider.DefaultAppSettingsFileName, appSettings);
        string directory = Path.GetDirectoryName(path)!;
        string fileName = Path.GetFileName(path);
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.SetBasePath(directory);

        configurationBuilder.AddJsonFile(fileName);
        IConfiguration configuration = configurationBuilder.Build();

        IConfigurationSection clientSection = configuration.GetSection(EurekaClientOptions.ConfigurationPrefix);
        var clientOptions = new EurekaClientOptions();
        clientSection.Bind(clientOptions);

        Assert.Equal("proxyHost", clientOptions.EurekaServer.ProxyHost);
        Assert.Equal(100, clientOptions.EurekaServer.ProxyPort);
        Assert.Equal("proxyPassword", clientOptions.EurekaServer.ProxyPassword);
        Assert.Equal("proxyUserName", clientOptions.EurekaServer.ProxyUserName);
        Assert.Equal(100, clientOptions.EurekaServer.ConnectTimeoutSeconds);
        Assert.Equal("https://foo.bar:8761/eureka/", clientOptions.EurekaServerServiceUrls);
        Assert.Equal(100, clientOptions.RegistryFetchIntervalSeconds);
        Assert.Equal("registryRefreshSingleVipAddress", clientOptions.RegistryRefreshSingleVipAddress);
        Assert.True(clientOptions.IsFetchDeltaDisabled);
        Assert.True(clientOptions.ShouldFetchRegistry);
        Assert.True(clientOptions.ShouldFilterOnlyUpInstances);
        Assert.True(clientOptions.EurekaServer.ShouldGZipContent);
        Assert.True(clientOptions.ShouldRegisterWithEureka);
        Assert.NotNull(clientOptions.Health);
        Assert.True(clientOptions.Health.ContributorEnabled);
        Assert.True(clientOptions.Health.CheckEnabled);
        Assert.Null(clientOptions.Health.MonitoredApps);
    }

    [Fact]
    public async Task Client_EnabledByDefault()
    {
        var services = new ServiceCollection();
        services.AddEurekaDiscoveryClient();

        var appSettings = new Dictionary<string, string?>
        {
            ["eureka:client:serviceUrl"] = "http://testhost/eureka"
        };

        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(appSettings).Build());

        await using ServiceProvider serviceProvider = services.BuildServiceProvider(true);
        var clientOptions = serviceProvider.GetRequiredService<IOptions<EurekaClientOptions>>();

        Assert.True(clientOptions.Value.Enabled);
    }

    [Fact]
    public async Task ClientDisabledBySpringCloudDiscovery_EnabledFalse()
    {
        var services = new ServiceCollection();
        services.AddEurekaDiscoveryClient();

        var appSettings = new Dictionary<string, string?>
        {
            ["spring:cloud:discovery:enabled"] = "false",
            ["eureka:client:serviceUrl"] = "http://testhost/eureka"
        };

        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(appSettings).Build());

        await using ServiceProvider serviceProvider = services.BuildServiceProvider(true);
        var clientOptions = serviceProvider.GetRequiredService<IOptions<EurekaClientOptions>>();

        Assert.False(clientOptions.Value.Enabled);
    }

    [Fact]
    public async Task ClientOptionsValidation_SucceedsWhenEurekaIsTurnedOff()
    {
        using var scope = new EnvironmentVariableScope("DOTNET_RUNNING_IN_CONTAINER", "true");

        var appSettings = new Dictionary<string, string?>
        {
            ["eureka:client:enabled"] = "false"
        };

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(appSettings).Build());
        services.AddEurekaDiscoveryClient();

        await using ServiceProvider serviceProvider = services.BuildServiceProvider(true);
        var clientOptions = serviceProvider.GetRequiredService<IOptions<EurekaClientOptions>>();

        Action action = () => _ = clientOptions.Value;

        action.Should().NotThrow();
    }

    [Fact]
    public async Task ClientOptionsValidation_FailsWhenServiceUrlIsNotProvided()
    {
        var appSettings = new Dictionary<string, string?>
        {
            ["eureka:client:ServiceUrl"] = " "
        };

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(appSettings).Build());
        services.AddEurekaDiscoveryClient();

        await using ServiceProvider serviceProvider = services.BuildServiceProvider(true);
        var clientOptions = serviceProvider.GetRequiredService<IOptions<EurekaClientOptions>>();

        Action action = () => _ = clientOptions.Value;

        action.Should().ThrowExactly<OptionsValidationException>().WithMessage("Eureka Service URL must be provided.");
    }

    [Fact]
    public async Task ClientOptionsValidation_FailsWhenServiceUrlsAreInvalid()
    {
        var appSettings = new Dictionary<string, string?>
        {
            ["eureka:client:ServiceUrl"] = "http://server,/not-fully-qualified,https://eureka.com,broken"
        };

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(appSettings).Build());
        services.AddEurekaDiscoveryClient();

        await using ServiceProvider serviceProvider = services.BuildServiceProvider(true);
        var clientOptions = serviceProvider.GetRequiredService<IOptions<EurekaClientOptions>>();

        Action action = () => _ = clientOptions.Value;

        OptionsValidationException? exception = action.Should().ThrowExactly<OptionsValidationException>().Which;
        exception.Failures.Should().ContainSingle(message => message == "Eureka URL '/not-fully-qualified' is invalid.");
        exception.Failures.Should().ContainSingle(message => message == "Eureka URL 'broken' is invalid.");
    }

    [Fact]
    public async Task ClientOptionsValidation_FailsWhenRunningInCloudWithLocalhost()
    {
        using var scope = new EnvironmentVariableScope("DOTNET_RUNNING_IN_CONTAINER", "true");

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddEurekaDiscoveryClient();

        await using ServiceProvider serviceProvider = services.BuildServiceProvider(true);
        var clientOptions = serviceProvider.GetRequiredService<IOptions<EurekaClientOptions>>();

        Action action = () => _ = clientOptions.Value;

        action.Should().ThrowExactly<OptionsValidationException>().WithMessage(
            "Eureka URL 'http://localhost:8761/eureka/' is not valid in containerized or cloud environments. " +
            "Please configure Eureka:Client:ServiceUrl with a non-localhost address or add a service binding.");
    }

    [Fact]
    public async Task Client_FavorsEurekaClientEnabled()
    {
        var services = new ServiceCollection();
        services.AddEurekaDiscoveryClient();

        var appSettings = new Dictionary<string, string?>
        {
            ["spring:cloud:discovery:enabled"] = "false",
            ["eureka:client:enabled"] = "true",
            ["eureka:client:serviceUrl"] = "http://testhost/eureka"
        };

        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(appSettings).Build());

        await using ServiceProvider serviceProvider = services.BuildServiceProvider(true);
        var clientOptions = serviceProvider.GetRequiredService<IOptions<EurekaClientOptions>>();

        Assert.True(clientOptions.Value.Enabled);
    }
}
