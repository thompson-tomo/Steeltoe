// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Mvc;

namespace Steeltoe.Management.Endpoint.Test.Actuators.RouteMappings.AppTypes.TestApiControllers;

[ApiController]
[Route("[controller]")]
internal sealed class SimpleTestController : ControllerBase
{
    [HttpGet]
    public IEnumerable<string> GetAll()
    {
        return [];
    }
}
