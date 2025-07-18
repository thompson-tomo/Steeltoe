// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Options;
using Steeltoe.Common.HealthChecks;
using Steeltoe.Management.Endpoint.Actuators.Health.Contributors.FileSystem;

namespace Steeltoe.Management.Endpoint.Actuators.Health.Contributors;

internal sealed class DiskSpaceHealthContributor : IHealthContributor
{
    private readonly IDiskSpaceProvider _diskSpaceProvider;
    private readonly IOptionsMonitor<DiskSpaceContributorOptions> _optionsMonitor;

    public string Id => "diskSpace";

    public DiskSpaceHealthContributor(IDiskSpaceProvider diskSpaceProvider, IOptionsMonitor<DiskSpaceContributorOptions> optionsMonitor)
    {
        ArgumentNullException.ThrowIfNull(diskSpaceProvider);
        ArgumentNullException.ThrowIfNull(optionsMonitor);

        _diskSpaceProvider = diskSpaceProvider;
        _optionsMonitor = optionsMonitor;
    }

    public Task<HealthCheckResult?> CheckHealthAsync(CancellationToken cancellationToken)
    {
        HealthCheckResult? result = Health();
        return Task.FromResult(result);
    }

    private HealthCheckResult? Health()
    {
        DiskSpaceContributorOptions options = _optionsMonitor.CurrentValue;

        if (!options.Enabled)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(options.Path))
        {
            HealthCheckResult? networkDiskHealth = GetNetworkDiskSpaceHealth(options);

            if (networkDiskHealth != null)
            {
                return networkDiskHealth;
            }

            HealthCheckResult? localDiskHealth = GetLocalDiskSpaceHealth(options);

            if (localDiskHealth != null)
            {
                return localDiskHealth;
            }
        }

        var unknownDiskHealth = new HealthCheckResult
        {
            Status = HealthStatus.Unknown,
            Description = "Failed to determine free disk space.",
            Details =
            {
                ["error"] = "The configured path is invalid or does not exist."
            }
        };

        if (!string.IsNullOrEmpty(options.Path))
        {
            unknownDiskHealth.Details["path"] = options.Path;
        }

        return unknownDiskHealth;
    }

    private HealthCheckResult? GetNetworkDiskSpaceHealth(DiskSpaceContributorOptions options)
    {
        if (_diskSpaceProvider.IsRunningOnWindows && options.Path?.StartsWith(@"\\", StringComparison.Ordinal) == true)
        {
            IDirectoryInfoWrapper directoryInfo = _diskSpaceProvider.GetDirectoryInfo(options.Path);

            if (directoryInfo.Exists)
            {
                INetworkShareWrapper? networkShare = _diskSpaceProvider.TryGetNetworkShare(options.Path);

                if (networkShare != null)
                {
                    return new HealthCheckResult
                    {
                        Status = networkShare.FreeBytesAvailable >= (ulong)options.Threshold ? HealthStatus.Up : HealthStatus.Down,
                        Details =
                        {
                            ["total"] = networkShare.TotalNumberOfBytes,
                            ["free"] = networkShare.FreeBytesAvailable,
                            ["threshold"] = options.Threshold,
                            ["path"] = options.Path,
                            ["exists"] = true
                        }
                    };
                }
            }
        }

        return null;
    }

    private HealthCheckResult? GetLocalDiskSpaceHealth(DiskSpaceContributorOptions options)
    {
        string absolutePath = Path.GetFullPath(options.Path!);
        IDirectoryInfoWrapper directoryInfo = _diskSpaceProvider.GetDirectoryInfo(absolutePath);

        if (directoryInfo.Exists)
        {
            IList<IDriveInfoWrapper> systemDrives = _diskSpaceProvider.GetDrives();
            IDriveInfoWrapper? driveInfo = FindVolume(absolutePath, systemDrives);

            if (driveInfo != null)
            {
                long freeSpaceInBytes = driveInfo.TotalFreeSpace;

                return new HealthCheckResult
                {
                    Status = freeSpaceInBytes >= options.Threshold ? HealthStatus.Up : HealthStatus.Down,
                    Details =
                    {
                        ["total"] = driveInfo.TotalSize,
                        ["free"] = freeSpaceInBytes,
                        ["threshold"] = options.Threshold,
                        ["path"] = absolutePath,
                        ["exists"] = driveInfo.RootDirectory.Exists
                    }
                };
            }
        }

        return null;
    }

    internal static IDriveInfoWrapper? FindVolume(string absolutePath, IEnumerable<IDriveInfoWrapper> systemDrives)
    {
        // Prefer to match "/mnt/data/path/to/directory" against "/mnt/data" over "/".
        IDriveInfoWrapper? longestMatch = null;

        foreach (IDriveInfoWrapper drive in systemDrives)
        {
            string volumePath = drive.RootDirectory.FullName;

            if (!PathIsOrStartsWith(absolutePath, volumePath))
            {
                continue;
            }

            if (longestMatch == null || longestMatch.RootDirectory.FullName.Length < drive.RootDirectory.FullName.Length)
            {
                longestMatch = drive;
            }
        }

        return longestMatch;
    }

    private static bool PathIsOrStartsWith(string absolutePath, string volumePath)
    {
        if (!absolutePath.StartsWith(volumePath, StringComparison.OrdinalIgnoreCase))
        {
            // Exit fast if no match is possible.
            return false;
        }

        // "/tmp/someLonger" is not a subdirectory of "/tmp/some".
        // And on Linux, "/tmp/SOME" is not the same as "/tmp/some".

        string relativePath = Path.GetRelativePath(volumePath, absolutePath);
        return !relativePath.StartsWith("..", StringComparison.Ordinal);
    }
}
