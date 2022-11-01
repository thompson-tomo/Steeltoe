// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Mvc;
using Steeltoe.CircuitBreaker.Hystrix.MetricsEvents.Test;

namespace Steeltoe.CircuitBreaker.Hystrix.MetricsEvents.Controllers.Test;

[Route("test/test.command")]
public class TestController : Controller
{
    [HttpGet]
    public async Task<IActionResult> RunCommand()
    {
        var cmd = new MyCommand();
        await cmd.ExecuteAsync();
        return Ok();
    }
}