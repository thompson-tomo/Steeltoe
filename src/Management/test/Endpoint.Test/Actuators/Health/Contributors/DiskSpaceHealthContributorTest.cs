// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Options;
using Steeltoe.Common;
using Steeltoe.Common.HealthChecks;
using Steeltoe.Common.TestResources;
using Steeltoe.Management.Endpoint.Actuators.Health.Contributors;

namespace Steeltoe.Management.Endpoint.Test.Actuators.Health.Contributors;

public sealed class DiskSpaceHealthContributorTest : BaseTest
{
    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        IOptionsMonitor<DiskSpaceContributorOptions> optionsMonitor = GetOptionsMonitorFromSettings<DiskSpaceContributorOptions>();
        var contributor = new DiskSpaceHealthContributor(optionsMonitor);
        Assert.Equal("diskSpace", contributor.Id);
    }

    [Fact]
    public async Task Health_InitializedWithDefaults_ReturnsExpected()
    {
        IOptionsMonitor<DiskSpaceContributorOptions> optionsMonitor = GetOptionsMonitorFromSettings<DiskSpaceContributorOptions>();
        var contributor = new DiskSpaceHealthContributor(optionsMonitor);
        Assert.Equal("diskSpace", contributor.Id);
        HealthCheckResult? result = await contributor.CheckHealthAsync(TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.Equal(HealthStatus.Up, result.Status);
        Assert.NotNull(result.Details);
        Assert.True(result.Details.ContainsKey("total"));
        Assert.True(result.Details.ContainsKey("free"));
        Assert.True(result.Details.ContainsKey("threshold"));
        Assert.True(result.Details.ContainsKey("path"));
        Assert.True(result.Details.ContainsKey("exists"));
    }

    [Fact]
    public async Task Health_UnknownDirectory_ReportsError()
    {
        var optionsMonitor = TestOptionsMonitor.Create(new DiskSpaceContributorOptions
        {
            Path = OperatingSystem.IsWindows() ? @"C:\does-not-exist" : "/does/not/exist"
        });

        var contributor = new DiskSpaceHealthContributor(optionsMonitor);

        HealthCheckResult? result = await contributor.CheckHealthAsync(TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result.Status.Should().Be(HealthStatus.Unknown);
        result.Description.Should().Be("Failed to determine free disk space.");
        result.Details.Should().Contain("error", "The configured path is invalid or does not exist.");
    }

    [Theory]
    [InlineData(@"C:\", @"C:\", PlatformID.Win32NT)]
    [InlineData(@"C:\Windows\System32", @"C:\", PlatformID.Win32NT)]
    [InlineData(@"C:\Windows\System32\", @"C:\", PlatformID.Win32NT)]
    [InlineData(@"c:\WINDOWS\SYSTEM32\", @"C:\", PlatformID.Win32NT)]
    [InlineData("/", "/", PlatformID.Unix, PlatformID.MacOSX)]
    [InlineData("/dev", "/dev", PlatformID.Unix, PlatformID.MacOSX)]
    [InlineData("/dev/", "/dev", PlatformID.Unix, PlatformID.MacOSX)]
    [InlineData("/dev/shm", "/dev/shm", PlatformID.Unix, PlatformID.MacOSX)]
    [InlineData("/dev/shm/", "/dev/shm", PlatformID.Unix, PlatformID.MacOSX)]
    [InlineData("/dev/shm/data", "/dev/shm", PlatformID.Unix, PlatformID.MacOSX)]
    [InlineData("/dev/shm-some", "/dev", PlatformID.Unix, PlatformID.MacOSX)]
    [InlineData("/dev/SHM", "/dev", PlatformID.Unix)]
    [InlineData("/dev/SHM", "/dev/shm", PlatformID.MacOSX)]
    public void Selects_correct_volume(string path, string? expected, params PlatformID[] platforms)
    {
        if (OperatingSystem.IsWindows() && !platforms.Contains(PlatformID.Win32NT))
        {
            return;
        }

        if (OperatingSystem.IsLinux() && !platforms.Contains(PlatformID.Unix))
        {
            return;
        }

        if (OperatingSystem.IsMacOS() && !platforms.Contains(PlatformID.MacOSX))
        {
            return;
        }

        DriveInfo[] systemDrives = OperatingSystem.IsWindows()
            ?
            [
                new DriveInfo(@"C:\"),
                new DriveInfo(@"D:\")
            ]
            :
            [
                new DriveInfo("/"),
                new DriveInfo("/dev"),
                new DriveInfo("/dev/shm")
            ];

        DriveInfo? drive = DiskSpaceHealthContributor.FindVolume(path, systemDrives);

        if (expected == null)
        {
            drive.Should().BeNull();
        }
        else
        {
            drive.Should().NotBeNull();
            drive.RootDirectory.FullName.Should().Be(expected);
        }
    }

    [Fact(Skip = "Integration test - Requires Windows file share")]
    public async Task SupportsWindowsFileShare()
    {
        if (Platform.IsWindows)
        {
            Dictionary<string, string?> settings = new()
            {
                ["Management:Endpoints:Health:DiskSpace:Path"] = @"\\localhost\steeltoe_network_share"
            };

            IOptionsMonitor<DiskSpaceContributorOptions> optionsMonitor = GetOptionsMonitorFromSettings<DiskSpaceContributorOptions>(settings);
            var contributor = new DiskSpaceHealthContributor(optionsMonitor);
            Assert.Equal("diskSpace", contributor.Id);
            HealthCheckResult? result = await contributor.CheckHealthAsync(TestContext.Current.CancellationToken);
            Assert.NotNull(result);
            Assert.Equal(HealthStatus.Up, result.Status);
            Assert.NotNull(result.Details);
            Assert.True(result.Details.ContainsKey("total"));
            Assert.True(result.Details.ContainsKey("free"));
            Assert.True(result.Details.ContainsKey("threshold"));
            Assert.True(result.Details.ContainsKey("path"));
            Assert.True(result.Details.ContainsKey("exists"));
        }
    }
}
