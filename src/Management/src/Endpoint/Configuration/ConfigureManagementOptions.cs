// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Steeltoe.Common.Json;

namespace Steeltoe.Management.Endpoint.Configuration;

internal sealed class ConfigureManagementOptions : IConfigureOptionsWithKey<ManagementOptions>
{
    private const string CloudFoundryEnabledConfigurationKey = "Management:CloudFoundry:Enabled";
    private const string DefaultPath = "/actuator";
    internal const string DefaultCloudFoundryPath = "/cloudfoundryapplication";

    private readonly IConfiguration _configuration;
    private readonly HasCloudFoundrySecurityMiddlewareMarker _hasCloudFoundrySecurityMiddlewareMarker;

    public string ConfigurationKey => "Management:Endpoints";

    public ConfigureManagementOptions(IConfiguration configuration, HasCloudFoundrySecurityMiddlewareMarker hasCloudFoundrySecurityMiddlewareMarker)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(hasCloudFoundrySecurityMiddlewareMarker);

        _configuration = configuration;
        _hasCloudFoundrySecurityMiddlewareMarker = hasCloudFoundrySecurityMiddlewareMarker;
    }

    public void Configure(ManagementOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _configuration.GetSection(ConfigurationKey).Bind(options);

        if (bool.TryParse(_configuration[CloudFoundryEnabledConfigurationKey], out bool isEnabled))
        {
            options.IsCloudFoundryEnabled = isEnabled;
        }

        options.HasCloudFoundrySecurity = _hasCloudFoundrySecurityMiddlewareMarker.Value;

        ConfigureSerializerOptions(options);

        options.Path ??= DefaultPath;

        var configureExposure = new ConfigureExposure(_configuration);
        configureExposure.Configure(options.Exposure);
    }

    private static void ConfigureSerializerOptions(ManagementOptions options)
    {
        options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

        // This was added initially for the route mappings actuator, to make generic method signatures human-readable,
        // but may affect other endpoints too. Removing this is a breaking change.
        options.SerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

        options.SerializerOptions.AddJsonIgnoreEmptyCollection();

        foreach (string converterTypeName in options.CustomJsonConverters)
        {
            var converterType = Type.GetType(converterTypeName, true)!;
            var converterInstance = (JsonConverter)Activator.CreateInstance(converterType)!;

            options.SerializerOptions.Converters.Add(converterInstance);
        }

        if (!options.SerializerOptions.Converters.OfType<JsonStringEnumConverter>().Any())
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }
    }

    private sealed class ConfigureExposure
    {
        private const string ConfigurationKey = "Management:Endpoints:Actuator:Exposure";
        private const string SpringConfigurationKey = "Management:Endpoints:Web:Exposure";

        private static readonly HashSet<string> DefaultIncludes =
        [
            "health",
            "info"
        ];

        private readonly IConfiguration _configuration;

        public ConfigureExposure(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            _configuration = configuration;
        }

        public void Configure(Exposure options)
        {
            ArgumentNullException.ThrowIfNull(options);

            HashSet<string> includes = [];
            HashSet<string> excludes = [];

            IConfigurationSection springSection = _configuration.GetSection(SpringConfigurationKey);

            if (springSection.Exists())
            {
                includes = GetSetFromConfigurationCsvString(springSection, "Include") ?? [];
                excludes = GetSetFromConfigurationCsvString(springSection, "Exclude") ?? [];
            }

            _configuration.GetSection(ConfigurationKey).Bind(options);

            if (options.Include.Count == 0 && options.Exclude.Count == 0 && !springSection.Exists())
            {
                includes = DefaultIncludes;
            }
            else
            {
                if (options.Include is [""])
                {
                    includes.Clear();
                }
                else
                {
                    options.Include.ToList().ForEach(include => includes.Add(include));
                }

                if (options.Exclude is [""])
                {
                    excludes.Clear();
                }
                else
                {
                    options.Exclude.ToList().ForEach(exclude => excludes.Add(exclude));
                }
            }

            options.Include.Clear();
            includes.ToList().ForEach(options.Include.Add);

            options.Exclude.Clear();
            excludes.ToList().ForEach(options.Exclude.Add);
        }

        private static HashSet<string>? GetSetFromConfigurationCsvString(IConfigurationSection section, string key)
        {
            return section.GetValue<string?>(key)?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        }
    }
}
