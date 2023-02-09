﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Steeltoe.Connector.EFCore.Test;
using Steeltoe.Extensions.Configuration.CloudFoundry;
#if NET6_0_OR_GREATER
using Steeltoe.Extensions.Configuration.Kubernetes.ServiceBinding;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Steeltoe.Connector.PostgreSql.EFCore.Test;

public class PostgresDbContextOptionsExtensionsTest
{
    public PostgresDbContextOptionsExtensionsTest()
    {
        Environment.SetEnvironmentVariable("VCAP_APPLICATION", null);
        Environment.SetEnvironmentVariable("VCAP_SERVICES", null);
    }

    [Fact]
    public void UseNpgsql_ThrowsIfDbContextOptionsBuilderNull()
    {
        DbContextOptionsBuilder optionsBuilder = null;
        DbContextOptionsBuilder<GoodDbContext> goodBuilder = null;
        IConfigurationRoot config = null;

        var ex = Assert.Throws<ArgumentNullException>(() => PostgresDbContextOptionsExtensions.UseNpgsql(optionsBuilder, config));
        Assert.Contains(nameof(optionsBuilder), ex.Message);

        var ex2 = Assert.Throws<ArgumentNullException>(() => PostgresDbContextOptionsExtensions.UseNpgsql(optionsBuilder, config, "foobar"));
        Assert.Contains(nameof(optionsBuilder), ex2.Message);

        var ex3 = Assert.Throws<ArgumentNullException>(() => PostgresDbContextOptionsExtensions.UseNpgsql<GoodDbContext>(goodBuilder, config));
        Assert.Contains(nameof(optionsBuilder), ex3.Message);

        var ex4 = Assert.Throws<ArgumentNullException>(() => PostgresDbContextOptionsExtensions.UseNpgsql<GoodDbContext>(goodBuilder, config, "foobar"));
        Assert.Contains(nameof(optionsBuilder), ex4.Message);
    }

    [Fact]
    public void UseNpgsql_ThrowsIfConfigurationNull()
    {
        var optionsBuilder = new DbContextOptionsBuilder();
        var goodBuilder = new DbContextOptionsBuilder<GoodDbContext>();
        IConfigurationRoot config = null;

        var ex = Assert.Throws<ArgumentNullException>(() => PostgresDbContextOptionsExtensions.UseNpgsql(optionsBuilder, config));
        Assert.Contains(nameof(config), ex.Message);

        var ex2 = Assert.Throws<ArgumentNullException>(() => PostgresDbContextOptionsExtensions.UseNpgsql(optionsBuilder, config, "foobar"));
        Assert.Contains(nameof(config), ex2.Message);

        var ex3 = Assert.Throws<ArgumentNullException>(() => PostgresDbContextOptionsExtensions.UseNpgsql<GoodDbContext>(goodBuilder, config));
        Assert.Contains(nameof(config), ex3.Message);

        var ex4 = Assert.Throws<ArgumentNullException>(() => PostgresDbContextOptionsExtensions.UseNpgsql<GoodDbContext>(goodBuilder, config, "foobar"));
        Assert.Contains(nameof(config), ex4.Message);
    }

    [Fact]
    public void UseNpgsql_ThrowsIfServiceNameNull()
    {
        var optionsBuilder = new DbContextOptionsBuilder();
        var goodBuilder = new DbContextOptionsBuilder<GoodDbContext>();
        var config = new ConfigurationBuilder().Build();
        string serviceName = null;

        var ex2 = Assert.Throws<ArgumentException>(() => PostgresDbContextOptionsExtensions.UseNpgsql(optionsBuilder, config, serviceName));
        Assert.Contains(nameof(serviceName), ex2.Message);

        var ex4 = Assert.Throws<ArgumentException>(() => PostgresDbContextOptionsExtensions.UseNpgsql<GoodDbContext>(goodBuilder, config, serviceName));
        Assert.Contains(nameof(serviceName), ex4.Message);
    }

    [Fact]
    public void AddDbContext_NoVCAPs_AddsDbContext_WithPostgresConnection()
    {
        IServiceCollection services = new ServiceCollection();
        var config = new ConfigurationBuilder().Build();

        services.AddDbContext<GoodDbContext>(options =>
            options.UseNpgsql(config));

        var service = services.BuildServiceProvider().GetService<GoodDbContext>();
        Assert.NotNull(service);
        var con = service.Database.GetDbConnection();
        Assert.NotNull(con);
        Assert.NotNull(con as NpgsqlConnection);
    }

    [Fact]
    public void AddDbContext_WithServiceName_NoVCAPs_ThrowsConnectorException()
    {
        IServiceCollection services = new ServiceCollection();
        var config = new ConfigurationBuilder().Build();

        services.AddDbContext<GoodDbContext>(options =>
            options.UseNpgsql(config, "foobar"));

        var ex = Assert.Throws<ConnectorException>(() => services.BuildServiceProvider().GetService<GoodDbContext>());
        Assert.Contains("foobar", ex.Message);
    }

    [Fact]
    public void AddDbContext_MultiplePostgresServices_ThrowsConnectorException()
    {
        IServiceCollection services = new ServiceCollection();

        Environment.SetEnvironmentVariable("VCAP_APPLICATION", TestHelpers.VCAP_APPLICATION);
        Environment.SetEnvironmentVariable("VCAP_SERVICES", PostgresTestHelpers.TwoServerVCAP_EDB);

        var builder = new ConfigurationBuilder();
        builder.AddCloudFoundry();
        var config = builder.Build();

        services.AddDbContext<GoodDbContext>(options =>
            options.UseNpgsql(config));

        var ex = Assert.Throws<ConnectorException>(() => services.BuildServiceProvider().GetService<GoodDbContext>());
        Assert.Contains("Multiple", ex.Message);
    }

    [Fact]
    public void AddDbContexts_WithEDBVCAPs_AddsDbContexts()
    {
        IServiceCollection services = new ServiceCollection();

        Environment.SetEnvironmentVariable("VCAP_APPLICATION", TestHelpers.VCAP_APPLICATION);
        Environment.SetEnvironmentVariable("VCAP_SERVICES", PostgresTestHelpers.SingleServerVCAP_EDB);

        var builder = new ConfigurationBuilder();
        builder.AddCloudFoundry();
        var config = builder.Build();

        services.AddDbContext<GoodDbContext>(options =>
            options.UseNpgsql(config));

        var built = services.BuildServiceProvider();
        var service = built.GetService<GoodDbContext>();
        Assert.NotNull(service);

        var con = service.Database.GetDbConnection();
        Assert.NotNull(con);
        var postCon = con as NpgsqlConnection;
        Assert.NotNull(postCon);

        var connString = con.ConnectionString;
        Assert.NotNull(connString);

        Assert.Contains("1e9e5dae-ed26-43e7-abb4-169b4c3beaff", connString);
        Assert.Contains("5432", connString);
        Assert.Contains("postgres.testcloud.com", connString);
        Assert.Contains("lmu7c96mgl99b2t1hvdgd5q94v", connString);
        Assert.Contains("1e9e5dae-ed26-43e7-abb4-169b4c3beaff", connString);
    }

    [Fact]
    public void AddDbContexts_WithCrunchyVCAPs_AddsDbContexts()
    {
        IServiceCollection services = new ServiceCollection();

        Environment.SetEnvironmentVariable("VCAP_APPLICATION", TestHelpers.VCAP_APPLICATION);
        Environment.SetEnvironmentVariable("VCAP_SERVICES", PostgresTestHelpers.SingleServerVCAP_Crunchy);

        var builder = new ConfigurationBuilder();
        builder.AddCloudFoundry();
        var config = builder.Build();

        services.AddDbContext<GoodDbContext>(options =>
            options.UseNpgsql(config));

        var built = services.BuildServiceProvider();
        var service = built.GetService<GoodDbContext>();
        Assert.NotNull(service);

        var con = service.Database.GetDbConnection();
        Assert.NotNull(con);
        var postCon = con as NpgsqlConnection;
        Assert.NotNull(postCon);

        var connString = con.ConnectionString;
        Assert.NotNull(connString);

        Assert.Contains("Host=10.194.59.205", connString);
        Assert.Contains("Port=5432", connString);
        Assert.Contains("Username=testrolee93ccf859894dc60dcd53218492b37b4", connString);
        Assert.Contains("Password=Qp!1mB1$Zk2T!$!D85_E", connString);
        Assert.Contains("Database=steeltoe", connString);
    }

    [Fact]
    public void AddDbContexts_WithEncodedCrunchyVCAPs_AddsDbContexts()
    {
        IServiceCollection services = new ServiceCollection();

        Environment.SetEnvironmentVariable("VCAP_APPLICATION", TestHelpers.VCAP_APPLICATION);
        Environment.SetEnvironmentVariable("VCAP_SERVICES", PostgresTestHelpers.SingleServerEncodedVCAP_Crunchy);

        var appsettings = new Dictionary<string, string>();

        var builder = new ConfigurationBuilder();
        builder.AddInMemoryCollection(appsettings);
        builder.AddCloudFoundry();
        var config = builder.Build();

        services.AddDbContext<GoodDbContext>(options =>
            options.UseNpgsql(config));

        var built = services.BuildServiceProvider();
        var service = built.GetService<GoodDbContext>();
        Assert.NotNull(service);

        var con = service.Database.GetDbConnection();
        Assert.NotNull(con);
        var postCon = con as NpgsqlConnection;
        Assert.NotNull(postCon);

        var connString = con.ConnectionString;
        Assert.NotNull(connString);

        Assert.Contains("Host=10.194.59.205", connString);
        Assert.Contains("Port=5432", connString);
        Assert.Contains("Username=testrolee93ccf859894dc60dcd53218492b37b4", connString);
        Assert.Contains("Password=Qp!1mB1$Zk2T!$!D85_E", connString);
        Assert.Contains("Database=steeltoe", connString);
    }

#if NET6_0_OR_GREATER
    [Fact]
    public void AddDbContext_WithK8sBinding_AddsDbContext()
    {
        var rootDir = Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "resources", "bindings");
        Environment.SetEnvironmentVariable("SERVICE_BINDING_ROOT", rootDir);

        try
        {
            IServiceCollection services = new ServiceCollection();

            var appsettings = new Dictionary<string, string>
            {
                { "steeltoe:kubernetes:bindings:enable", "true" }
            };

            var builder = new ConfigurationBuilder();
            builder.AddInMemoryCollection(appsettings);
            builder.AddKubernetesServiceBindings(false);
            var configurationRoot = builder.Build();

            services.AddDbContext<GoodDbContext>(options => options.UseNpgsql(configurationRoot));

            using var built = services.BuildServiceProvider();
            using var dbContext = built.GetService<GoodDbContext>();
            dbContext.Should().NotBeNull();

            using var connection = dbContext.Database.GetDbConnection();
            connection.Should().BeOfType<NpgsqlConnection>().And.Subject.Should().NotBeNull();

            connection.ConnectionString.Should().Contain("Host=10.194.59.205");
            connection.ConnectionString.Should().Contain("Port=5432");
            connection.ConnectionString.Should().Contain("Username=testrolee93ccf859894dc60dcd53218492b37b4");
            connection.ConnectionString.Should().Contain("Password=Qp!1mB1$Zk2T!$!D85_E");
            connection.ConnectionString.Should().Contain("Database=steeltoe");
        }
        finally
        {
            Environment.SetEnvironmentVariable("SERVICE_BINDING_ROOT", null);
        }
    }
#endif
}