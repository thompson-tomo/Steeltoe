// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Configuration;
using Steeltoe.Common.TestResources;
using Steeltoe.Configuration.CloudFoundry;
using Steeltoe.Connectors.Services;
using Xunit;

namespace Steeltoe.Connectors.CloudFoundry.Test;

public class CloudFoundryServiceInfoCreatorTest
{
    [Fact]
    public void Constructor_ThrowsIfConfigNull()
    {
        const IConfiguration configuration = null;

        var ex = Assert.Throws<ArgumentNullException>(() => CloudFoundryServiceInfoCreator.Instance(configuration));
        Assert.Contains(nameof(configuration), ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Constructor_ReturnsInstance()
    {
        IConfiguration configuration = new ConfigurationBuilder().Build();

        var inst = CloudFoundryServiceInfoCreator.Instance(configuration);
        Assert.NotNull(inst);
    }

    [Fact]
    public void Constructor_ReturnsNewInstance()
    {
        IConfiguration configuration1 = new ConfigurationBuilder().Build();
        IConfiguration configuration2 = new ConfigurationBuilder().Build();

        var inst = CloudFoundryServiceInfoCreator.Instance(configuration1);
        Assert.NotNull(inst);

        var inst2 = CloudFoundryServiceInfoCreator.Instance(configuration2);
        Assert.NotSame(inst, inst2);
    }

    [Fact]
    public void BuildServiceInfoFactories_BuildsExpected()
    {
        IConfiguration configuration = new ConfigurationBuilder().Build();

        var inst = CloudFoundryServiceInfoCreator.Instance(configuration);
        Assert.NotNull(inst);
        Assert.NotNull(inst.Factories);
        Assert.NotEmpty(inst.Factories);
    }

    [Fact]
    public void BuildServiceInfos_NoCloudFoundryServices_BuildsExpected()
    {
        var builder = new ConfigurationBuilder();
        builder.AddCloudFoundry();
        IConfigurationRoot configurationRoot = builder.Build();

        var creator = CloudFoundryServiceInfoCreator.Instance(configurationRoot);

        Assert.NotNull(creator.ServiceInfos);
        Assert.Equal(0, creator.ServiceInfos.Count);
    }

    [Fact]
    public void BuildServiceInfos_WithCloudFoundryServices_BuildsExpected()
    {
        const string environment2 = @"
                {
                    ""p-mysql"": [{
                        ""credentials"": {
                            ""hostname"": ""192.168.0.90"",
                            ""port"": 3306,
                            ""name"": ""cf_b4f8d2fa_a3ea_4e3a_a0e8_2cd040790355"",
                            ""username"": ""Dd6O1BPXUHdrmzbP"",
                            ""password"": ""7E1LxXnlH2hhlPVt"",
                            ""uri"": ""mysql://Dd6O1BPXUHdrmzbP:7E1LxXnlH2hhlPVt@192.168.0.90:3306/cf_b4f8d2fa_a3ea_4e3a_a0e8_2cd040790355?reconnect=true"",
                            ""jdbcUrl"": ""jdbc:mysql://192.168.0.90:3306/cf_b4f8d2fa_a3ea_4e3a_a0e8_2cd040790355?user=Dd6O1BPXUHdrmzbP&password=7E1LxXnlH2hhlPVt""
                        },
                        ""syslog_drain_url"": null,
                        ""label"": ""p-mysql"",
                        ""provider"": null,
                        ""plan"": ""100mb-dev"",
                        ""name"": ""spring-cloud-broker-db"",
                        ""tags"": [
                            ""mysql"",
                            ""relational""
                        ]
                    }],
                    ""p-rabbitmq"": [{
                        ""credentials"": {
                            ""http_api_uris"": [""https://03c7a684-6ff1-4bd0-ad45-d10374ffb2af:l5oq2q0unl35s6urfsuib0jvpo@pivotal-rabbitmq.system.testcloud.com/api/""],
                            ""ssl"": false,
                            ""dashboard_url"": ""https://pivotal-rabbitmq.system.testcloud.com/#/login/03c7a684-6ff1-4bd0-ad45-d10374ffb2af/l5oq2q0unl35s6urfsuib0jvpo"",
                            ""password"": ""l5oq2q0unl35s6urfsuib0jvpo"",
                            ""protocols"": {
                                ""management"": {
                                    ""path"": ""/api/"",
                                    ""ssl"": false,
                                    ""hosts"": [""192.168.0.81""],
                                    ""password"": ""l5oq2q0unl35s6urfsuib0jvpo"",
                                    ""username"": ""03c7a684-6ff1-4bd0-ad45-d10374ffb2af"",
                                    ""port"": 15672,
                                    ""host"": ""192.168.0.81"",
                                    ""uri"": ""https://03c7a684-6ff1-4bd0-ad45-d10374ffb2af:l5oq2q0unl35s6urfsuib0jvpo@192.168.0.81:15672/api/"",
                                    ""uris"": [""https://03c7a684-6ff1-4bd0-ad45-d10374ffb2af:l5oq2q0unl35s6urfsuib0jvpo@192.168.0.81:15672/api/""]
                                },
                                ""amqp"": {
                                    ""vhost"": ""fb03d693-91fe-4dc5-8203-ff7a6390df66"",
                                    ""username"": ""03c7a684-6ff1-4bd0-ad45-d10374ffb2af"",
                                    ""password"": ""l5oq2q0unl35s6urfsuib0jvpo"",
                                    ""port"": 5672,
                                    ""host"": ""192.168.0.81"",
                                    ""hosts"": [""192.168.0.81""],
                                    ""ssl"": false,
                                    ""uri"": ""amqp://03c7a684-6ff1-4bd0-ad45-d10374ffb2af:l5oq2q0unl35s6urfsuib0jvpo@192.168.0.81:5672/fb03d693-91fe-4dc5-8203-ff7a6390df66"",
                                    ""uris"": [""amqp://03c7a684-6ff1-4bd0-ad45-d10374ffb2af:l5oq2q0unl35s6urfsuib0jvpo@192.168.0.81:5672/fb03d693-91fe-4dc5-8203-ff7a6390df66""]
                                }
                            },
                            ""username"": ""03c7a684-6ff1-4bd0-ad45-d10374ffb2af"",
                            ""hostname"": ""192.168.0.81"",
                            ""hostnames"": [""192.168.0.81""],
                            ""vhost"": ""fb03d693-91fe-4dc5-8203-ff7a6390df66"",
                            ""http_api_uri"": ""https://03c7a684-6ff1-4bd0-ad45-d10374ffb2af:l5oq2q0unl35s6urfsuib0jvpo@pivotal-rabbitmq.system.testcloud.com/api/"",
                            ""uri"": ""amqp://03c7a684-6ff1-4bd0-ad45-d10374ffb2af:l5oq2q0unl35s6urfsuib0jvpo@192.168.0.81/fb03d693-91fe-4dc5-8203-ff7a6390df66"",
                            ""uris"": [""amqp://03c7a684-6ff1-4bd0-ad45-d10374ffb2af:l5oq2q0unl35s6urfsuib0jvpo@192.168.0.81/fb03d693-91fe-4dc5-8203-ff7a6390df66""]
                        },
                        ""syslog_drain_url"": null,
                        ""label"": ""p-rabbitmq"",
                        ""provider"": null,
                        ""plan"": ""standard"",
                        ""name"": ""spring-cloud-broker-rmq"",
                        ""tags"": [
                            ""rabbitmq"",
                            ""messaging"",
                            ""message-queue"",
                            ""amqp"",
                            ""stomp"",
                            ""mqtt"",
                            ""pivotal""
                        ]
                    }]
                }";

        using var appScope = new EnvironmentVariableScope("VCAP_APPLICATION", TestHelpers.VcapApplication);
        using var servicesScope = new EnvironmentVariableScope("VCAP_SERVICES", environment2);

        var builder = new ConfigurationBuilder();
        builder.AddCloudFoundry();
        IConfigurationRoot configurationRoot = builder.Build();
        var creator = CloudFoundryServiceInfoCreator.Instance(configurationRoot);
        Assert.NotNull(creator.ServiceInfos);
        Assert.Equal(2, creator.ServiceInfos.Count);
    }

    [Fact]
    public void GetServiceInfo_NoVCAPs_ReturnsExpected()
    {
        var builder = new ConfigurationBuilder();
        builder.AddCloudFoundry();
        IConfigurationRoot configurationRoot = builder.Build();
        var creator = CloudFoundryServiceInfoCreator.Instance(configurationRoot);

        IEnumerable<IServiceInfo> result2 = creator.GetServiceInfosOfType(typeof(MySqlServiceInfo));
        Assert.NotNull(result2);
        Assert.Empty(result2);

        var result5 = creator.GetServiceInfo<MySqlServiceInfo>("foobar-db2");
        Assert.Null(result5);

        var result6 = creator.GetServiceInfo<MySqlServiceInfo>("spring-cloud-broker-db2");
        Assert.Null(result6);

        IServiceInfo result7 = creator.GetServiceInfo("spring-cloud-broker-db2");
        Assert.Null(result7);
    }

    [Fact]
    public void GetServiceInfosType_WithVCAPs_ReturnsExpected()
    {
        const string environment2 = @"
                {
                    ""p-mysql"": [{
                        ""credentials"": {
                            ""hostname"": ""192.168.0.90"",
                            ""port"": 3306,
                            ""name"": ""cf_b4f8d2fa_a3ea_4e3a_a0e8_2cd040790355"",
                            ""username"": ""Dd6O1BPXUHdrmzbP"",
                            ""password"": ""7E1LxXnlH2hhlPVt"",
                            ""uri"": ""mysql://Dd6O1BPXUHdrmzbP:7E1LxXnlH2hhlPVt@192.168.0.90:3306/cf_b4f8d2fa_a3ea_4e3a_a0e8_2cd040790355?reconnect=true"",
                            ""jdbcUrl"": ""jdbc:mysql://192.168.0.90:3306/cf_b4f8d2fa_a3ea_4e3a_a0e8_2cd040790355?user=Dd6O1BPXUHdrmzbP&password=7E1LxXnlH2hhlPVt""
                        },
                        ""syslog_drain_url"": null,
                        ""label"": ""p-mysql"",
                        ""provider"": null,
                        ""plan"": ""100mb-dev"",
                        ""name"": ""spring-cloud-broker-db"",
                        ""tags"": [
                            ""mysql"",
                            ""relational""
                        ]
                    },
                    {
                        ""credentials"": {
                            ""hostname"": ""192.168.0.90"",
                            ""port"": 3306,
                            ""name"": ""cf_b4f8d2fa_a3ea_4e3a_a0e8_2cd040790355"",
                            ""username"": ""Dd6O1BPXUHdrmzbP"",
                            ""password"": ""7E1LxXnlH2hhlPVt"",
                            ""uri"": ""mysql://Dd6O1BPXUHdrmzbP:7E1LxXnlH2hhlPVt@192.168.0.90:3306/cf_b4f8d2fa_a3ea_4e3a_a0e8_2cd040790355?reconnect=true"",
                            ""jdbcUrl"": ""jdbc:mysql://192.168.0.90:3306/cf_b4f8d2fa_a3ea_4e3a_a0e8_2cd040790355?user=Dd6O1BPXUHdrmzbP&password=7E1LxXnlH2hhlPVt""
                        },
                        ""syslog_drain_url"": null,
                        ""label"": ""p-mysql"",
                        ""provider"": null,
                        ""plan"": ""100mb-dev"",
                        ""name"": ""spring-cloud-broker-db2"",
                        ""tags"": [
                            ""mysql"",
                            ""relational""
                        ]
                    }]
                }";

        using var appScope = new EnvironmentVariableScope("VCAP_APPLICATION", TestHelpers.VcapApplication);
        using var servicesScope = new EnvironmentVariableScope("VCAP_SERVICES", environment2);

        var builder = new ConfigurationBuilder();
        builder.AddCloudFoundry();
        IConfigurationRoot configurationRoot = builder.Build();
        var creator = CloudFoundryServiceInfoCreator.Instance(configurationRoot);

        IEnumerable<MySqlServiceInfo> result = creator.GetServiceInfosOfType<MySqlServiceInfo>();
        Assert.Equal(2, result.Count(si => si != null));

        IEnumerable<IServiceInfo> result2 = creator.GetServiceInfosOfType(typeof(MySqlServiceInfo));
        Assert.Equal(2, result2.Count(si => si is MySqlServiceInfo));

        var result5 = creator.GetServiceInfo<MySqlServiceInfo>("foobar-db2");
        Assert.Null(result5);

        var result6 = creator.GetServiceInfo<MySqlServiceInfo>("spring-cloud-broker-db2");
        Assert.NotNull(result6);

        IServiceInfo result7 = creator.GetServiceInfo("spring-cloud-broker-db2");
        Assert.NotNull(result7);
        Assert.True(result7 is MySqlServiceInfo);
    }
}
