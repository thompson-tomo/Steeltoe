// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Steeltoe.Common.TestResources;
using Steeltoe.Management.Configuration;
using Steeltoe.Management.Endpoint.Actuators.All;
using Steeltoe.Management.Endpoint.Actuators.CloudFoundry;
using Steeltoe.Management.Endpoint.Actuators.HeapDump;
using Steeltoe.Management.Endpoint.Actuators.ThreadDump;
using Steeltoe.Management.Endpoint.Configuration;
using Steeltoe.Management.Endpoint.Test.Actuators.HeapDump;
using Steeltoe.Management.Endpoint.Test.Actuators.ThreadDump;

namespace Steeltoe.Management.Endpoint.Test;

public sealed class EndpointAuthorizationTest
{
    public static TheoryData<RegistrationMode, Type> EndpointTestData
    {
        get
        {
            List<Type> endpointOptionsTypes =
            [
                .. typeof(ConfigureEndpointOptions<>).Assembly.GetTypes().Where(type =>
                    type.IsAssignableTo(typeof(EndpointOptions)) && type != typeof(CloudFoundryEndpointOptions))
            ];

            TheoryData<RegistrationMode, Type> theoryData = [];

            foreach (Type endpointOptionsType in endpointOptionsTypes)
            {
                foreach (RegistrationMode mode in Enum.GetValues<RegistrationMode>())
                {
                    theoryData.Add(mode, endpointOptionsType);
                }
            }

            return theoryData;
        }
    }

    [Theory]
    [MemberData(nameof(EndpointTestData))]
    public async Task MapTestAuthSuccess(RegistrationMode mode, Type endpointOptionsType)
    {
        IHostBuilder hostBuilder = GetHostBuilder(policy => policy.RequireClaim("scope", "actuators.read"), mode);
        await ActAndAssertAsync(hostBuilder, endpointOptionsType, true);
    }

    [Theory]
    [MemberData(nameof(EndpointTestData))]
    public async Task MapTestAuthFail(RegistrationMode mode, Type endpointOptionsType)
    {
        IHostBuilder hostBuilder = GetHostBuilder(policy => policy.RequireClaim("scope", "invalid-scope"), mode);
        await ActAndAssertAsync(hostBuilder, endpointOptionsType, false);
    }

    private static HostBuilder GetHostBuilder(Action<AuthorizationPolicyBuilder> policyAction, RegistrationMode mode)
    {
        var appSettings = new Dictionary<string, string?>
        {
            ["Management:Endpoints:Actuator:Exposure:Include:0"] = "*"
        };

        HostBuilder hostBuilder = TestHostBuilderFactory.CreateWeb();
        hostBuilder.ConfigureAppConfiguration(configure => configure.AddInMemoryCollection(appSettings));

        hostBuilder.ConfigureServices(services =>
        {
            services.AddSingleton<IHeapDumper, FakeHeapDumper>();
            services.AddSingleton<IThreadDumper, FakeThreadDumper>();

            if (mode == RegistrationMode.Services)
            {
                services.AddAllActuators();
                services.ConfigureActuatorEndpoints(endpoints => endpoints.RequireAuthorization("TestAuth"));
            }
            else if (mode is RegistrationMode.UseEndpoints or RegistrationMode.MapEndpoints)
            {
                services.AddAllActuators(false);
            }

            services.AddAuthentication(TestAuthHandler.AuthenticationScheme).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.AuthenticationScheme, _ =>
                {
                });

            services.AddAuthorizationBuilder().AddPolicy("TestAuth", policyAction);
            services.AddServerSideBlazor();

            // Workaround for service provider validation failure:
            //   Unable to resolve service for type 'Microsoft.AspNetCore.Components.PersistentComponentState' while attempting
            //   to activate 'Microsoft.AspNetCore.Components.Forms.DefaultAntiforgeryStateProvider'.
            // This happens because we're adding Blazor, but without using WebAssemblyHostBuilder.
            services.AddRazorComponents();
        });

        hostBuilder.ConfigureWebHost(builder =>
        {
            builder.Configure(app =>
            {
                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapBlazorHub(); // https://github.com/SteeltoeOSS/Steeltoe/issues/729
                });

                if (mode == RegistrationMode.UseEndpoints)
                {
                    app.UseActuatorEndpoints(endpoints => endpoints.RequireAuthorization("TestAuth"));
                }
                else if (mode == RegistrationMode.MapEndpoints)
                {
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapActuators().RequireAuthorization("TestAuth");
                    });
                }
            });
        });

        return hostBuilder;
    }

    private async Task ActAndAssertAsync(IHostBuilder builder, Type endpointOptionsType, bool expectSuccess)
    {
        using IHost host = await builder.StartAsync(TestContext.Current.CancellationToken);

        Type optionsMonitorType = typeof(IOptionsMonitor<>).MakeGenericType(endpointOptionsType);
        object optionsMonitor = host.Services.GetRequiredService(optionsMonitorType);
        var options = (EndpointOptions)((dynamic)optionsMonitor).CurrentValue;

        ManagementOptions managementOptions = host.Services.GetRequiredService<IOptionsMonitor<ManagementOptions>>().CurrentValue;
        string path = options.GetPathMatchPattern(managementOptions.Path);
        path = path.Replace("health/{**_}", "health", StringComparison.Ordinal);
        HttpResponseMessage response;

        using HttpClient httpClient = host.GetTestClient();

        if (options.AllowedVerbs.Contains("Get"))
        {
            response = await httpClient.GetAsync(new Uri(path, UriKind.RelativeOrAbsolute), TestContext.Current.CancellationToken);
        }
        else
        {
            response = await httpClient.PostAsync(new Uri(path, UriKind.RelativeOrAbsolute), null, TestContext.Current.CancellationToken);
        }

        response.StatusCode.Should().Be(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.Forbidden);
    }

    public enum RegistrationMode
    {
        Services,
        UseEndpoints,
        MapEndpoints
    }

    private sealed class TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> optionsMonitor, ILoggerFactory loggerFactory, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(optionsMonitor, loggerFactory, encoder)
    {
        public const string AuthenticationScheme = "TestScheme";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claim = new Claim("scope", "actuators.read");
            var identity = new ClaimsIdentity([claim]);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
