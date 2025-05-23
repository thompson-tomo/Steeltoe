// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using System.Net;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Steeltoe.Common.TestResources;
using Steeltoe.Management.Endpoint.Actuators.DbMigrations;
using Steeltoe.Management.Endpoint.Configuration;

namespace Steeltoe.Management.Endpoint.Test.Actuators.DbMigrations;

public sealed class EndpointMiddlewareTest : BaseTest
{
    private static readonly Dictionary<string, string?> AppSettings = new()
    {
        ["management:endpoints:actuator:exposure:include:0"] = "dbmigrations"
    };

    [Fact]
    public async Task HandleEntityFrameworkCoreRequestAsync_ReturnsExpected()
    {
        IOptionsMonitor<DbMigrationsEndpointOptions> endpointOptionsMonitor = GetOptionsMonitorFromSettings<DbMigrationsEndpointOptions>();
        IOptionsMonitor<ManagementOptions> managementOptions = GetOptionsMonitorFromSettings<ManagementOptions>(AppSettings);

        var services = new ServiceCollection();
        services.AddScoped<MockDbContext>();

        await using ServiceProvider serviceProvider = services.BuildServiceProvider(true);

        var handler = new DbMigrationsEndpointHandler(endpointOptionsMonitor, serviceProvider, new TestDatabaseMigrationScanner(), NullLoggerFactory.Instance);
        var middleware = new DbMigrationsEndpointMiddleware(handler, managementOptions, NullLoggerFactory.Instance);

        HttpContext context = CreateRequest("GET", "/dbmigrations");
        await middleware.InvokeAsync(context, null);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body, Encoding.UTF8);
        string json = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
        var descriptor = new DbMigrationsDescriptor();
        descriptor.AppliedMigrations.Add("applied");
        descriptor.PendingMigrations.Add("pending");

        string expected = Serialize(new Dictionary<string, DbMigrationsDescriptor>
        {
            [nameof(MockDbContext)] = descriptor
        });

        Assert.Equal(expected, json);
    }

    [Fact]
    public async Task EntityFrameworkCoreActuator_ReturnsExpectedData()
    {
        WebHostBuilder builder = TestWebHostBuilderFactory.Create();
        builder.UseStartup<Startup>();
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(AppSettings));

        using IWebHost app = builder.Build();
        await app.StartAsync(TestContext.Current.CancellationToken);

        using HttpClient client = app.GetTestClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("http://localhost/actuator/dbmigrations"), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var descriptor = new DbMigrationsDescriptor();
        descriptor.AppliedMigrations.Add("applied");
        descriptor.PendingMigrations.Add("pending");

        string expected = Serialize(new Dictionary<string, DbMigrationsDescriptor>
        {
            [nameof(MockDbContext)] = descriptor
        });

        Assert.Equal(expected, json);
    }

    [Fact]
    public void RoutesByPathAndVerb()
    {
        var endpointOptions = GetOptionsFromSettings<DbMigrationsEndpointOptions>();
        ManagementOptions managementOptions = GetOptionsMonitorFromSettings<ManagementOptions>().CurrentValue;

        Assert.True(endpointOptions.RequiresExactMatch());
        Assert.Equal("/actuator/dbmigrations", endpointOptions.GetPathMatchPattern(managementOptions.Path));
    }

    private HttpContext CreateRequest(string method, string path)
    {
        HttpContext context = new DefaultHttpContext
        {
            TraceIdentifier = Guid.NewGuid().ToString()
        };

        context.Response.Body = new MemoryStream();
        context.Request.Method = method;
        context.Request.Path = path;
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("localhost");
        return context;
    }
}
