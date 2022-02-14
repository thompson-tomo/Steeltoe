// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Steeltoe.Common.Discovery;
using Steeltoe.Discovery;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Steeltoe.Common.Http.Test
{
    internal class TestDiscoveryClient : IDiscoveryClient
    {
        private readonly IServiceInstance _instance;

        public TestDiscoveryClient(IServiceInstance instance = null) => _instance = instance;

        public string Description => throw new NotImplementedException();

        public IList<string> Services => throw new NotImplementedException();

        public IList<IServiceInstance> GetInstances(string serviceId) => _instance != null ? new List<IServiceInstance>() { _instance } : new List<IServiceInstance>();

        public IServiceInstance GetLocalServiceInstance() => throw new NotImplementedException();

        public Task ShutdownAsync() => throw new NotImplementedException();
    }
}
