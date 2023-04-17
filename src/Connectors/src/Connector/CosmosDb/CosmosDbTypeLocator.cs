// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using System.Reflection;
using Steeltoe.Common.Reflection;

// ReSharper disable once CheckNamespace
namespace Steeltoe.Connector.CosmosDb;

/// <summary>
/// Assemblies and types used for interacting with CosmosDB.
/// </summary>
public static class CosmosDbTypeLocator
{
    /// <summary>
    /// Gets a list of supported CosmosDB assemblies.
    /// </summary>
    public static string[] Assemblies { get; internal set; } =
    {
        "Microsoft.Azure.Cosmos.Client"
    };

    /// <summary>
    /// Gets a list of supported CosmosDB client types.
    /// </summary>
    public static string[] ConnectionTypeNames { get; internal set; } =
    {
        "Microsoft.Azure.Cosmos.CosmosClient"
    };

    public static string[] ClientOptionsTypeNames { get; internal set; } =
    {
        "Microsoft.Azure.Cosmos.CosmosClientOptions"
    };

    /// <summary>
    /// Gets CosmosDbClient from CosmosDB Library.
    /// </summary>
    /// <exception cref="ConnectorException">
    /// When type is not found.
    /// </exception>
    public static Type CosmosClient => ReflectionHelpers.FindTypeOrThrow(Assemblies, ConnectionTypeNames, "CosmosClient", "a CosmosDB client");

    public static Type CosmosClientOptions => ReflectionHelpers.FindTypeOrThrow(Assemblies, ClientOptionsTypeNames, "CosmosClientOptions", "a CosmosDB client");

    /// <summary>
    /// Gets a method that lists accounts available in a CosmosClient.
    /// </summary>
    public static MethodInfo ReadAccountAsyncMethod => FindMethodOrThrow(CosmosClient, "ReadAccountAsync");

    private static MethodInfo FindMethodOrThrow(Type type, string methodName)
    {
        MethodInfo returnType = ReflectionHelpers.FindMethod(type, methodName);

        if (returnType == null)
        {
            throw new ConnectorException("Unable to find required CosmosDB type or method, are you missing a CosmosDB Nuget package?");
        }

        return returnType;
    }
}