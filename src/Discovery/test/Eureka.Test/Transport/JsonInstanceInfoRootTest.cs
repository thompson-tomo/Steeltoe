// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using Steeltoe.Discovery.Eureka.AppInfo;
using Steeltoe.Discovery.Eureka.Transport;

namespace Steeltoe.Discovery.Eureka.Test.Transport;

public sealed class JsonInstanceInfoRootTest
{
    [Fact]
    public void Deserialize_GoodJson()
    {
        const string json = """
            {
              "instance": {
                "instanceId": "DESKTOP-GNQ5SUT",
                "app": "FOOBAR",
                "appGroupName": null,
                "ipAddr": "192.168.0.147",
                "sid": "na",
                "port": {
                  "@enabled": true,
                  "$": 80
                },
                "securePort": {
                  "@enabled": false,
                  "$": 443
                },
                "homePageUrl": "http://DESKTOP-GNQ5SUT:80/",
                "statusPageUrl": "http://DESKTOP-GNQ5SUT:80/Status",
                "healthCheckUrl": "http://DESKTOP-GNQ5SUT:80/health-check",
                "secureHealthCheckUrl": null,
                "vipAddress": "DESKTOP-GNQ5SUT:80",
                "secureVipAddress": "DESKTOP-GNQ5SUT:443",
                "countryId": 1,
                "dataCenterInfo": {
                  "@class": "com.netflix.appinfo.InstanceInfo$DefaultDataCenterInfo",
                  "name": "MyOwn"
                },
                "hostName": "DESKTOP-GNQ5SUT",
                "status": "UP",
                "overriddenStatus": "UNKNOWN",
                "leaseInfo": {
                  "renewalIntervalInSecs": 30,
                  "durationInSecs": 90,
                  "registrationTimestamp": 0,
                  "lastRenewalTimestamp": 0,
                  "renewalTimestamp": 0,
                  "evictionTimestamp": 0,
                  "serviceUpTimestamp": 0
                },
                "isCoordinatingDiscoveryServer": false,
                "metadata": {
                  "@class": "java.util.Collections$EmptyMap",
                  "metadata": null
                },
                "lastUpdatedTimestamp": 1458116137663,
                "lastDirtyTimestamp": 1458116137663,
                "actionType": "ADDED",
                "asgName": null
              }
            }
            """;

        var result = JsonSerializer.Deserialize<JsonInstanceInfoRoot>(json);

        result.Should().NotBeNull();
        result.Instance.Should().NotBeNull();

        // Random check some values
        result.Instance.ActionType.Should().Be(ActionType.Added);
        result.Instance.HealthCheckUrl.Should().Be("http://DESKTOP-GNQ5SUT:80/health-check");
        result.Instance.AppName.Should().Be("FOOBAR");
    }
}
