// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Configuration;
using Steeltoe.Configuration.Kubernetes.ServiceBindings.PostProcessors;

namespace Steeltoe.Configuration.Kubernetes.ServiceBindings.Test;

public sealed class PostProcessorsTest : BasePostProcessorsTest
{
    [Fact]
    public void Processes_MySql_configuration()
    {
        var postProcessor = new MySqlKubernetesPostProcessor();

        Tuple<string, string>[] secrets =
        [
            Tuple.Create("host", "test-host"),
            Tuple.Create("port", "test-port"),
            Tuple.Create("database", "test-database"),
            Tuple.Create("username", "test-username"),
            Tuple.Create("password", "test-password")
        ];

        Dictionary<string, string?> configurationData = GetConfigurationData(TestBindingName, MySqlKubernetesPostProcessor.BindingType, secrets);
        PostProcessorConfigurationProvider provider = GetConfigurationProvider(postProcessor);

        postProcessor.PostProcessConfiguration(provider, configurationData);

        string keyPrefix = GetOutputKeyPrefix(TestBindingName, MySqlKubernetesPostProcessor.BindingType);
        configurationData.Should().ContainKey($"{keyPrefix}:host").WhoseValue.Should().Be("test-host");
        configurationData.Should().ContainKey($"{keyPrefix}:port").WhoseValue.Should().Be("test-port");
        configurationData.Should().ContainKey($"{keyPrefix}:database").WhoseValue.Should().Be("test-database");
        configurationData.Should().ContainKey($"{keyPrefix}:username").WhoseValue.Should().Be("test-username");
        configurationData.Should().ContainKey($"{keyPrefix}:password").WhoseValue.Should().Be("test-password");
    }

    [Fact]
    public void Processes_PostgreSql_configuration()
    {
        var postProcessor = new PostgreSqlKubernetesPostProcessor();

        Tuple<string, string>[] secrets =
        [
            Tuple.Create("host", "test-host"),
            Tuple.Create("port", "test-port"),
            Tuple.Create("database", "test-database"),
            Tuple.Create("username", "test-username"),
            Tuple.Create("password", "test-password")
        ];

        Dictionary<string, string?> configurationData = GetConfigurationData(TestBindingName, PostgreSqlKubernetesPostProcessor.BindingType, secrets);
        PostProcessorConfigurationProvider provider = GetConfigurationProvider(postProcessor);

        postProcessor.PostProcessConfiguration(provider, configurationData);

        string keyPrefix = GetOutputKeyPrefix(TestBindingName, PostgreSqlKubernetesPostProcessor.BindingType);
        configurationData.Should().ContainKey($"{keyPrefix}:host").WhoseValue.Should().Be("test-host");
        configurationData.Should().ContainKey($"{keyPrefix}:port").WhoseValue.Should().Be("test-port");
        configurationData.Should().ContainKey($"{keyPrefix}:database").WhoseValue.Should().Be("test-database");
        configurationData.Should().ContainKey($"{keyPrefix}:username").WhoseValue.Should().Be("test-username");
        configurationData.Should().ContainKey($"{keyPrefix}:password").WhoseValue.Should().Be("test-password");
    }

    [Fact]
    public void Processes_MongoDb_configuration()
    {
        var postProcessor = new MongoDbKubernetesPostProcessor();

        Tuple<string, string>[] secrets =
        [
            Tuple.Create("host", "test-host"),
            Tuple.Create("port", "test-port"),
            Tuple.Create("database", "test-database"),
            Tuple.Create("username", "test-username"),
            Tuple.Create("password", "test-password")
        ];

        Dictionary<string, string?> configurationData = GetConfigurationData(TestBindingName, MongoDbKubernetesPostProcessor.BindingType, secrets);
        PostProcessorConfigurationProvider provider = GetConfigurationProvider(postProcessor);

        postProcessor.PostProcessConfiguration(provider, configurationData);

        string keyPrefix = GetOutputKeyPrefix(TestBindingName, MongoDbKubernetesPostProcessor.BindingType);
        configurationData.Should().ContainKey($"{keyPrefix}:server").WhoseValue.Should().Be("test-host");
        configurationData.Should().ContainKey($"{keyPrefix}:port").WhoseValue.Should().Be("test-port");
        configurationData.Should().ContainKey($"{keyPrefix}:database").WhoseValue.Should().Be("test-database");
        configurationData.Should().ContainKey($"{keyPrefix}:authenticationDatabase").WhoseValue.Should().Be("test-database");
        configurationData.Should().ContainKey($"{keyPrefix}:username").WhoseValue.Should().Be("test-username");
        configurationData.Should().ContainKey($"{keyPrefix}:password").WhoseValue.Should().Be("test-password");
    }

    [Fact]
    public void Processes_RabbitMQ_configuration()
    {
        var postProcessor = new RabbitMQKubernetesPostProcessor();

        Tuple<string, string>[] secrets =
        [
            Tuple.Create("host", "test-host"),
            Tuple.Create("port", "test-port"),
            Tuple.Create("username", "test-username"),
            Tuple.Create("password", "test-password"),
            Tuple.Create("virtual-host", "test-virtual-host")
        ];

        Dictionary<string, string?> configurationData = GetConfigurationData(TestBindingName, RabbitMQKubernetesPostProcessor.BindingType, secrets);
        PostProcessorConfigurationProvider provider = GetConfigurationProvider(postProcessor);

        postProcessor.PostProcessConfiguration(provider, configurationData);

        string keyPrefix = GetOutputKeyPrefix(TestBindingName, RabbitMQKubernetesPostProcessor.BindingType);
        configurationData.Should().ContainKey($"{keyPrefix}:host").WhoseValue.Should().Be("test-host");
        configurationData.Should().ContainKey($"{keyPrefix}:port").WhoseValue.Should().Be("test-port");
        configurationData.Should().ContainKey($"{keyPrefix}:username").WhoseValue.Should().Be("test-username");
        configurationData.Should().ContainKey($"{keyPrefix}:password").WhoseValue.Should().Be("test-password");
        configurationData.Should().ContainKey($"{keyPrefix}:virtualHost").WhoseValue.Should().Be("test-virtual-host");
    }

    [Fact]
    public void Processes_Redis_configuration()
    {
        var postProcessor = new RedisKubernetesPostProcessor();

        Tuple<string, string>[] secrets =
        [
            Tuple.Create("host", "test-host"),
            Tuple.Create("port", "test-port"),
            Tuple.Create("ssl", "test-ssl"),
            Tuple.Create("password", "test-password"),
            Tuple.Create("database", "test-database"),
            Tuple.Create("client-name", "test-client-name")
        ];

        Dictionary<string, string?> configurationData = GetConfigurationData(TestBindingName, RedisKubernetesPostProcessor.BindingType, secrets);
        PostProcessorConfigurationProvider provider = GetConfigurationProvider(postProcessor);

        postProcessor.PostProcessConfiguration(provider, configurationData);

        string keyPrefix = GetOutputKeyPrefix(TestBindingName, RedisKubernetesPostProcessor.BindingType);
        configurationData.Should().ContainKey($"{keyPrefix}:host").WhoseValue.Should().Be("test-host");
        configurationData.Should().ContainKey($"{keyPrefix}:port").WhoseValue.Should().Be("test-port");
        configurationData.Should().ContainKey($"{keyPrefix}:ssl").WhoseValue.Should().Be("test-ssl");
        configurationData.Should().ContainKey($"{keyPrefix}:password").WhoseValue.Should().Be("test-password");
        configurationData.Should().ContainKey($"{keyPrefix}:defaultDatabase").WhoseValue.Should().Be("test-database");
        configurationData.Should().ContainKey($"{keyPrefix}:name").WhoseValue.Should().Be("test-client-name");
    }

    [Fact]
    public void Processes_ApplicationConfigurationService_ConfigurationData()
    {
        var postProcessor = new ApplicationConfigurationServicePostProcessor();

        Tuple<string, string>[] secrets =
        [
            Tuple.Create("provider", "acs"),
            Tuple.Create("random", "data"),
            Tuple.Create("from", "some-source"),
            Tuple.Create("secret", "password"),
            Tuple.Create("secret.one", "password1"),
            Tuple.Create("secret__two", "password2")
        ];

        Dictionary<string, string?> configurationData =
            GetConfigurationData(TestBindingName, ApplicationConfigurationServicePostProcessor.BindingType, secrets);

        PostProcessorConfigurationProvider provider = GetConfigurationProvider(postProcessor);

        postProcessor.PostProcessConfiguration(provider, configurationData);

        configurationData.Should().ContainKey("random").WhoseValue.Should().Be("data");
        configurationData.Should().ContainKey("from").WhoseValue.Should().Be("some-source");
        configurationData.Should().ContainKey("secret").WhoseValue.Should().Be("password");
        configurationData.Should().ContainKey("secret:one").WhoseValue.Should().Be("password1");
        configurationData.Should().ContainKey("secret:two").WhoseValue.Should().Be("password2");
        configurationData.Should().NotContainKey("type");
        configurationData.Should().NotContainKey("provider");
    }

    [Fact]
    public void PopulatesDotNetFriendlyKeysFromOtherFormats()
    {
        string rootDirectory = GetK8SResourcesDirectory();
        var source = new KubernetesServiceBindingConfigurationSource(new DirectoryServiceBindingsReader(rootDirectory));
        var postProcessor = new ApplicationConfigurationServicePostProcessor();
        source.RegisterPostProcessor(postProcessor);

        IConfiguration configuration = new ConfigurationBuilder().Add(source).Build();

        configuration["test-secret-key"].Should().Be("test-secret-value");
        configuration["key:with:periods"].Should().Be("test-secret-value.");
        configuration["key:with:double:underscores"].Should().Be("test-secret-value0");
        configuration["key:with:double:underscores_"].Should().Be("test-secret-value1");
        configuration["key:with:double:underscores:"].Should().Be("test-secret-value2");
    }

    private static string GetK8SResourcesDirectory()
    {
        return Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "resources", "k8s");
    }
}
