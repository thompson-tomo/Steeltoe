// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Configuration;

namespace Steeltoe.Configuration.SpringBoot.Test;

public sealed class SpringBootCommandLineSourceTest
{
    [Fact]
    public void Build__ReturnsProvider()
    {
        var builder = new ConfigurationBuilder();
        var source = new SpringBootCommandLineSource([]);
        IConfigurationProvider provider = source.Build(builder);

        provider.Should().BeOfType<SpringBootCommandLineProvider>();
    }
}
