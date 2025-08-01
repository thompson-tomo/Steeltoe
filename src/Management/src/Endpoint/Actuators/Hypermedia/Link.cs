// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Serialization;

namespace Steeltoe.Management.Endpoint.Actuators.Hypermedia;

public sealed class Link
{
    public string Href { get; set; }

    [JsonPropertyName("templated")]
    public bool IsTemplated { get; }

    public Link(string href, bool isTemplated)
    {
        ArgumentException.ThrowIfNullOrEmpty(href);

        Href = href;
        IsTemplated = isTemplated;
    }
}
