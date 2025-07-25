// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

namespace Steeltoe.Management.Endpoint.Actuators.Health.Contributors;

public sealed class LivenessStateContributorOptions
{
    internal static string GroupName => "liveness";

    /// <summary>
    /// Gets or sets a value indicating whether to enable the liveness contributor. Default value: false.
    /// </summary>
    public bool Enabled { get; set; }
}
