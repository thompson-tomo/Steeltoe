// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Steeltoe.Configuration.RandomValue.Test;

public sealed class RandomValueSourceTest
{
    [Fact]
    public void Constructors_InitializesDefaults()
    {
        var source = new RandomValueSource(NullLoggerFactory.Instance);

        source.Prefix.Should().Be("random:");

        source = new RandomValueSource("foobar:", NullLoggerFactory.Instance);

        source.Prefix.Should().Be("foobar:");
    }

    [Fact]
    public void Build_ReturnsProvider()
    {
        var source = new RandomValueSource(NullLoggerFactory.Instance);
        IConfigurationProvider provider = source.Build(new ConfigurationBuilder());

        provider.Should().BeOfType<RandomValueProvider>();
    }
}
