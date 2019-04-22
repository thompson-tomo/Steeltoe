﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Steeltoe.Discovery.Eureka;
using Steeltoe.Discovery.Eureka.AppInfo;
using Steeltoe.Discovery.Eureka.Transport;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Steeltoe.Discovery.Client
{
    public class EurekaDiscoveryClientBase
    {
        internal protected IEurekaClientConfig ClientConfig;
        internal protected IEurekaInstanceConfig InstConfig;
        internal protected IEurekaClient Client;

        protected EurekaDiscoveryClientBase(EurekaClientOptions clientOptions, EurekaInstanceOptions instOptions, IEurekaHttpClient httpClient, IApplicationLifetime lifeCycle = null, ILoggerFactory logFactory = null)
        {
            if (clientOptions == null)
            {
                throw new ArgumentNullException(nameof(clientOptions));
            }

            ClientConfig = clientOptions;
            InstConfig = instOptions;

            if (InstConfig == null)
            {
                DiscoveryManager.Instance.Initialize(ClientConfig, httpClient, logFactory);
            }
            else
            {
                ConfigureInstanceIfNeeded(InstConfig);
                DiscoveryManager.Instance.Initialize(ClientConfig, InstConfig, httpClient, logFactory);
            }

            if (lifeCycle != null)
            {
                lifeCycle.ApplicationStopping.Register(() => { ShutdownAsync(); });
            }

            Client = DiscoveryManager.Instance.Client;
        }
        public virtual string Description
        {
            get
            {
                return "Unknown";
            }
        }

        public IList<string> Services
        {
            get
            {
                return GetServices();
            }
        }

        protected internal virtual IList<string> GetServices()
        {
            Applications applications = Client.Applications;
            if (applications == null)
            {
                return new List<string>();
            }
            IList<Application> registered = applications.GetRegisteredApplications();
            List<string> names = new List<string>();
            foreach (Application app in registered)
            {
                if (app.Instances.Count == 0)
                {
                    continue;
                }

                names.Add(app.Name.ToLowerInvariant());

            }
            return names;
        }

        protected internal virtual void ConfigureInstanceIfNeeded(IEurekaInstanceConfig instConfig)
        {
            if (string.IsNullOrEmpty(instConfig.AppName))
            {
                instConfig.AppName = "unknown";
            }

            if (string.IsNullOrEmpty(instConfig.InstanceId))
            {
                var hostName = instConfig.GetHostName(false);
                var appName = instConfig.AppName;
                var index = (instConfig.NonSecurePort == -1) ? EurekaInstanceOptions.Default_NonSecurePort.ToString() : instConfig.NonSecurePort.ToString();
                instConfig.InstanceId = hostName + ":" + appName + ":" + index;
            }

            if (string.IsNullOrEmpty(instConfig.VirtualHostName))
            {
                instConfig.VirtualHostName = instConfig.AppName;
            }

            if (string.IsNullOrEmpty(instConfig.SecureVirtualHostName))
            {
                instConfig.SecureVirtualHostName = instConfig.AppName;
            }

        }

        public virtual Task ShutdownAsync()
        {
            ApplicationInfoManager.Instance.InstanceStatus = InstanceStatus.DOWN;
            return Client.ShutdownAsync();
        }
    }
}