﻿using KubeClient;
using KubeClient.Models;
using Ocelot.Logging;
using Ocelot.ServiceDiscovery.Providers;
using Ocelot.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ocelot.Provider.Kubernetes
{
    public class KubeProvider : IServiceDiscoveryProvider
    {
        private KubeRegistryConfiguration kubeRegistryConfiguration;
        private IOcelotLoggerFactory factory;
        private IKubeApiClient kubeApi;
        private IKubeApiClientFactory kubeClientFactory;

        public KubeProvider(KubeRegistryConfiguration kubeRegistryConfiguration, IOcelotLoggerFactory factory, IKubeApiClientFactory kubeClientFactory)
        {
            this.kubeRegistryConfiguration = kubeRegistryConfiguration;
            this.factory = factory;
            this.kubeApi = kubeClientFactory.Get(kubeRegistryConfiguration);
        }

        public async Task<List<Service>> Get()
        {
            var service = await kubeApi.ServicesV1().Get(kubeRegistryConfiguration.KeyOfServiceInK8s, kubeRegistryConfiguration.KubeNamespace);
            var services = new List<Service>();
            if (IsValid(service))
            {
                services.Add(BuildService(service));
            }
            return services;
        }

        private bool IsValid(ServiceV1 service)
        {
            if (string.IsNullOrEmpty(service.Spec.ClusterIP) || service.Spec.Ports.Count <= 0)
            {
                return false;
            }

            return true;
        }

        private Service BuildService(ServiceV1 serviceEntry)
        {
            var servicePort = serviceEntry.Spec.Ports.FirstOrDefault();
            return new Service(
                serviceEntry.Metadata.Name,
                new ServiceHostAndPort(serviceEntry.Spec.ClusterIP, servicePort.Port),
                serviceEntry.Metadata.Uid,
                string.Empty,
                Enumerable.Empty<string>());
        }
    }
}
