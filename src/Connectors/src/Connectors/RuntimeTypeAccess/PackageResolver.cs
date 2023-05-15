// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

#nullable enable

using System.Reflection;
using Steeltoe.Common;

namespace Steeltoe.Connectors.RuntimeTypeAccess;

/// <summary>
/// Dynamically loads <see cref="Type" />s at runtime from a list of candidates.
/// </summary>
internal abstract class PackageResolver
{
    private readonly IReadOnlyList<string> _assemblyNames;
    private readonly IReadOnlyList<string> _packageNames;

    protected PackageResolver(string assemblyName, string packageName)
        : this(new List<string>
        {
            assemblyName
        }, new List<string>
        {
            packageName
        })
    {
    }

    protected PackageResolver(IReadOnlyList<string> assemblyNames, IReadOnlyList<string> packageNames)
    {
        ArgumentGuard.NotNull(assemblyNames);
        ArgumentGuard.ElementsNotNullOrEmpty(assemblyNames);
        ArgumentGuard.NotNull(packageNames);
        ArgumentGuard.ElementsNotNullOrEmpty(packageNames);

        _assemblyNames = assemblyNames;
        _packageNames = packageNames;
    }

    protected TypeAccessor ResolveType(string typeName)
    {
        return ResolveType(new List<string>
        {
            typeName
        });
    }

    protected TypeAccessor ResolveType(IReadOnlyList<string> typeNames)
    {
        ArgumentGuard.NotNull(typeNames);
        ArgumentGuard.ElementsNotNullOrEmpty(typeNames);

        List<Exception> exceptions = new();

        // A type can be moved to a different assembly in a future NuGet version, so probe all combinations to be resilient against that.
        foreach (string assemblyName in _assemblyNames)
        {
            try
            {
                Assembly assembly = Assembly.Load(new AssemblyName(assemblyName));

                foreach (string typeName in typeNames)
                {
                    try
                    {
                        Type type = assembly.GetType(typeName, true)!;
                        return new TypeAccessor(type);
                    }
                    catch (Exception exception) when (exception is ArgumentException or IOException or BadImageFormatException or TypeLoadException)
                    {
                        exceptions.Add(exception);
                    }
                }
            }
            catch (Exception exception) when (exception is ArgumentException or IOException or BadImageFormatException)
            {
                exceptions.Add(exception);
            }
        }

        throw new AggregateException(
            _packageNames.Count == 1
                ? $"Unable to load a required type. Please add the '{_packageNames[0]}' NuGet package to your project."
                : $"Unable to load a required type. Please add one of these NuGet packages to your project: '{string.Join("', '", _packageNames)}'.",
            exceptions);
    }
}
