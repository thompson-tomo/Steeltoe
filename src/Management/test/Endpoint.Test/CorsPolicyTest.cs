// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using Steeltoe.Common.Http.HttpClientPooling;
using Steeltoe.Common.TestResources;
using Steeltoe.Configuration.CloudFoundry;
using Steeltoe.Management.Endpoint.Actuators.CloudFoundry;
using Steeltoe.Management.Endpoint.Actuators.Info;
using Steeltoe.Management.Endpoint.Actuators.Refresh;
using Steeltoe.Management.Endpoint.Configuration;

namespace Steeltoe.Management.Endpoint.Test;

public sealed class CorsPolicyTest
{
    [Fact]
    public async Task ConfiguresDefaultActuatorsCorsPolicyForGetRequest()
    {
        WebApplicationBuilder builder = TestWebApplicationBuilderFactory.Create();
        builder.Services.AddInfoActuator();
        await using WebApplication app = builder.Build();

        CorsOptions corsOptions = app.Services.GetRequiredService<IOptions<CorsOptions>>().Value;
        CorsPolicy? corsPolicy = corsOptions.GetPolicy(ActuatorsCorsPolicyOptions.PolicyName);
        corsPolicy.Should().NotBeNull();
        corsPolicy.AllowAnyOrigin.Should().BeTrue();
        corsPolicy.Methods.Should().ContainSingle().Which.Should().Be("GET");

        await app.StartAsync(TestContext.Current.CancellationToken);
        using HttpClient httpClient = app.GetTestClient();

        var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://localhost/actuator/info"));
        request.Headers.Add("Origin", "http://example.api.com");
        HttpResponseMessage response = await httpClient.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("Access-Control-Allow-Origin");
        response.Headers.GetValues("Access-Control-Allow-Origin").Should().ContainSingle().And.Contain("*");
    }

    [Fact]
    public async Task ConfiguresDefaultActuatorsCorsPolicyForPreflightPostRequest()
    {
        WebApplicationBuilder builder = TestWebApplicationBuilderFactory.Create();
        builder.Services.AddRefreshActuator();
        await using WebApplication app = builder.Build();

        CorsOptions corsOptions = app.Services.GetRequiredService<IOptions<CorsOptions>>().Value;
        CorsPolicy? corsPolicy = corsOptions.GetPolicy(ActuatorsCorsPolicyOptions.PolicyName);
        corsPolicy.Should().NotBeNull();
        corsPolicy.AllowAnyOrigin.Should().BeTrue();
        corsPolicy.Methods.Should().ContainSingle().Which.Should().Be("POST");

        await app.StartAsync(TestContext.Current.CancellationToken);
        using HttpClient httpClient = app.GetTestClient();

        var request = new HttpRequestMessage(HttpMethod.Options, new Uri("http://localhost/actuator/refresh"));
        request.Headers.Add("Origin", "http://example.api.com");
        request.Headers.Add("Access-Control-Request-Method", "POST");
        HttpResponseMessage response = await httpClient.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.Headers.Should().ContainKey("Access-Control-Allow-Origin");
        response.Headers.GetValues("Access-Control-Allow-Origin").Should().ContainSingle().And.Contain("*");
        response.Headers.Should().ContainKey("Access-Control-Allow-Methods");
        response.Headers.GetValues("Access-Control-Allow-Methods").Should().ContainSingle().And.Contain("POST");
    }

    [Fact]
    public async Task ConfiguresCustomActuatorsCorsPolicyForGetRequest()
    {
        WebApplicationBuilder builder = TestWebApplicationBuilderFactory.Create();
        builder.Services.AddInfoActuator();
        builder.Services.ConfigureActuatorsCorsPolicy(policy => policy.WithOrigins("http://example.api.com"));
        await using WebApplication app = builder.Build();

        CorsOptions corsOptions = app.Services.GetRequiredService<IOptions<CorsOptions>>().Value;
        CorsPolicy? corsPolicy = corsOptions.GetPolicy(ActuatorsCorsPolicyOptions.PolicyName);
        corsPolicy.Should().NotBeNull();
        corsPolicy.AllowAnyOrigin.Should().BeFalse();
        corsPolicy.IsOriginAllowed("http://example.api.com").Should().BeTrue();
        corsPolicy.IsOriginAllowed("http://google.com").Should().BeFalse();
        corsPolicy.Methods.Should().ContainSingle().Which.Should().Be("GET");

        await app.StartAsync(TestContext.Current.CancellationToken);
        using HttpClient httpClient = app.GetTestClient();

        var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://localhost/actuator/info"));
        request.Headers.Add("Origin", "http://example.api.com");
        HttpResponseMessage response = await httpClient.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("Access-Control-Allow-Origin");
        response.Headers.GetValues("Access-Control-Allow-Origin").Should().ContainSingle().And.Contain("http://example.api.com");

        request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://localhost/actuator/info"));
        request.Headers.Add("Origin", "http://google.com");
        response = await httpClient.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().NotContainKey("Access-Control-Allow-Origin");
    }

    [Fact]
    public async Task ConfiguresCustomActuatorsCorsPolicyForPreflightPostRequest()
    {
        const int preflightMaxAge = 5;

        WebApplicationBuilder builder = TestWebApplicationBuilderFactory.Create();
        builder.Services.AddRefreshActuator();
        builder.Services.ConfigureActuatorsCorsPolicy(policy => policy.AllowAnyOrigin().SetPreflightMaxAge(TimeSpan.FromSeconds(preflightMaxAge)));
        await using WebApplication app = builder.Build();

        CorsOptions corsOptions = app.Services.GetRequiredService<IOptions<CorsOptions>>().Value;
        CorsPolicy? corsPolicy = corsOptions.GetPolicy(ActuatorsCorsPolicyOptions.PolicyName);
        corsPolicy.Should().NotBeNull();
        corsPolicy.AllowAnyOrigin.Should().BeTrue();
        corsPolicy.PreflightMaxAge.Should().Be(TimeSpan.FromSeconds(preflightMaxAge));
        corsPolicy.Methods.Should().ContainSingle().Which.Should().Be("POST");

        await app.StartAsync(TestContext.Current.CancellationToken);
        using HttpClient httpClient = app.GetTestClient();

        var request = new HttpRequestMessage(HttpMethod.Options, new Uri("http://localhost/actuator/refresh"));
        request.Headers.Add("Origin", "http://example.api.com");
        request.Headers.Add("Access-Control-Request-Method", "POST");
        HttpResponseMessage response = await httpClient.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.Headers.Should().ContainKey("Access-Control-Allow-Origin").WhoseValue.Should().ContainSingle().And.Contain("*");
        response.Headers.Should().ContainKey("Access-Control-Allow-Methods").WhoseValue.Should().ContainSingle().And.Contain("POST");
        response.Headers.Should().ContainKey("Access-Control-Max-Age").WhoseValue.Should().ContainSingle().And.Contain($"{preflightMaxAge}");
    }

    [Fact]
    public async Task ConfiguresDefaultActuatorsCorsPolicyForGetRequestOnCloudFoundry()
    {
        const string token =
            "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJodHRwczovL3VhYS5jbG91ZC5jb20vb2F1dGgvdG9rZW4iLCJpYXQiOjE3MzcwNjMxNzYsImV4cCI6MTc2ODU5OTE3NiwiYXVkIjoiYWN0dWF0b3IiLCJzdWIiOiJ1c2VyQGVtYWlsLmNvbSIsInNjb3BlIjpbImFjdHVhdG9yLnJlYWQiLCJjbG91ZF9jb250cm9sbGVyLnVzZXIiXSwiRW1haWwiOiJ1c2VyQGVtYWlsLmNvbSIsImNsaWVudF9pZCI6ImFwcHNfbWFuYWdlcl9qcyIsInVzZXJfbmFtZSI6InVzZXJAZW1haWwuY29tIiwidXNlcl9pZCI6InVzZXIifQ.bfCtDFxcWF8Yuie2p89S8_fTuUkAOd3i9M8PyKDV-N0";

        using var scope = new EnvironmentVariableScope("VCAP_APPLICATION", """
            {
                "cf_api": "https://api.cloud.com",
                "limits": {
                    "fds": 16384,
                    "mem": 1024,
                    "disk": 1024
                },
                "application_name": "spring-cloud-broker",
                "application_uris": [
                    "spring-cloud-broker.apps.test-cloud.com"
                ],
                "name": "spring-cloud-broker",
                "space_name": "p-spring-cloud-services",
                "space_id": "65b73473-94cc-4640-b462-7ad52838b4ae",
                "uris": [
                    "spring-cloud-broker.apps.test-cloud.com"
                ],
                "users": null,
                "version": "07e112f7-2f71-4f5a-8a34-db51dbed30a3",
                "application_version": "07e112f7-2f71-4f5a-8a34-db51dbed30a3",
                "application_id": "798c2495-fe75-49b1-88da-b81197f2bf06"
            }
            """);

        WebApplicationBuilder builder = TestWebApplicationBuilderFactory.Create();
        builder.Configuration.AddCloudFoundry();
        builder.Services.AddInfoActuator();
        builder.Services.AddCloudFoundryActuator();
        await using WebApplication app = builder.Build();

        DelegateToMockHttpClientHandler handler = new DelegateToMockHttpClientHandler().Setup(mock =>
        {
            mock.Expect(HttpMethod.Get, "https://api.cloud.com/v2/apps/798c2495-fe75-49b1-88da-b81197f2bf06/permissions")
                .WithHeaders("Authorization", $"bearer {token}").Respond("application/json", """
                    {
                        "read_sensitive_data": true
                    }
                    """);
        });

        app.Services.GetRequiredService<HttpClientHandlerFactory>().Using(handler);

        CorsOptions corsOptions = app.Services.GetRequiredService<IOptions<CorsOptions>>().Value;
        CorsPolicy? corsPolicy = corsOptions.GetPolicy(ActuatorsCorsPolicyOptions.PolicyName);
        corsPolicy.Should().NotBeNull();
        corsPolicy.AllowAnyOrigin.Should().BeTrue();
        corsPolicy.Methods.Should().ContainSingle().Which.Should().Be("GET");
        corsPolicy.Headers.Should().BeEquivalentTo("Authorization", "X-Cf-App-Instance", "Content-Type", "Content-Disposition");

        await app.StartAsync(TestContext.Current.CancellationToken);
        using HttpClient httpClient = app.GetTestClient();

        var request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://localhost/cloudfoundryapplication/info"));
        request.Headers.Authorization = AuthenticationHeaderValue.Parse($"bearer {token}");
        request.Headers.Add("Origin", "http://example.api.com");
        HttpResponseMessage response = await httpClient.SendAsync(request, TestContext.Current.CancellationToken);

        handler.Mock.VerifyNoOutstandingExpectation();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("Access-Control-Allow-Origin");
        response.Headers.GetValues("Access-Control-Allow-Origin").Should().ContainSingle().And.Contain("*");
    }

    [Fact]
    public async Task DefaultActuatorsCorsPolicyFiresBeforeCloudFoundrySecurity()
    {
        using var scope = new EnvironmentVariableScope("VCAP_APPLICATION", "{}");

        WebApplicationBuilder builder = TestWebApplicationBuilderFactory.Create();
        builder.Services.AddCloudFoundryActuator();
        await using WebApplication app = builder.Build();

        await app.StartAsync(TestContext.Current.CancellationToken);

        using HttpClient httpClient = app.GetTestClient();
        var corsRequest = new HttpRequestMessage(HttpMethod.Options, new Uri("http://localhost/cloudfoundryapplication"));
        corsRequest.Headers.Add("access-control-request-method", "GET");
        corsRequest.Headers.Add("access-control-request-headers", "authorization");
        corsRequest.Headers.Add("origin", "http://example.api.com");
        HttpResponseMessage corsResponse = await httpClient.SendAsync(corsRequest, TestContext.Current.CancellationToken);

        corsResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        corsResponse.Headers.Should().ContainKey("Access-Control-Allow-Origin");
        corsResponse.Headers.GetValues("Access-Control-Allow-Origin").Should().ContainSingle().And.Contain("*");
        corsResponse.Headers.Should().ContainKey("Access-Control-Allow-Headers");

        corsResponse.Headers.GetValues("Access-Control-Allow-Headers").Should().ContainSingle().And
            .Contain("Authorization,X-Cf-App-Instance,Content-Type,Content-Disposition");

        corsResponse.Headers.Should().ContainKey("Access-Control-Allow-Methods");
        corsResponse.Headers.GetValues("Access-Control-Allow-Methods").Should().ContainSingle().And.Contain("GET");

        var actuatorRequest = new HttpRequestMessage(HttpMethod.Get, new Uri("http://localhost/cloudfoundryapplication"));
        actuatorRequest.Headers.Add("Origin", "http://example.api.com");
        HttpResponseMessage response = await httpClient.SendAsync(actuatorRequest, TestContext.Current.CancellationToken);

        // Returns ServiceUnavailable because UseCloudFoundrySecurity is invoked, but not fully mocked
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }
}
