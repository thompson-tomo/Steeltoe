// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using Aspire;
using Steeltoe.Management.Tasks;

[assembly: ConfigurationSchema("", typeof(TaskSettings))]
[assembly: LoggingCategories("Steeltoe", "Steeltoe.Management", "Steeltoe.Management.Tasks")]

[assembly: InternalsVisibleTo("Steeltoe.Bootstrap.AutoConfiguration")]
[assembly: InternalsVisibleTo("Steeltoe.Discovery.Eureka")]
[assembly: InternalsVisibleTo("Steeltoe.Management.Endpoint.Test")]
[assembly: InternalsVisibleTo("Steeltoe.Management.Prometheus")]
[assembly: InternalsVisibleTo("Steeltoe.Management.Tracing")]
