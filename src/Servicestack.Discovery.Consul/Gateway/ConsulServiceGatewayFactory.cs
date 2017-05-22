// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace ServiceStack.Discovery.Consul
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public class ConsulServiceGatewayFactory : ServiceGatewayFactoryBase
    {
        private readonly DefaultGatewayDelegate defaultGateway;

        private readonly IServiceDiscovery<ConsulService, ServiceRegistration> discoveryClient;

        private readonly ConcurrentDictionary<string, HttpCacheEntry> sharedCache = new ConcurrentDictionary<string, HttpCacheEntry>();

        public HashSet<Type> LocalTypes { get; set; }

        public ConsulServiceGatewayFactory(DefaultGatewayDelegate defaultGateway, IServiceDiscovery<ConsulService, ServiceRegistration> discoveryClient)
        {
            defaultGateway.ThrowIfNull(nameof(defaultGateway));
            discoveryClient.ThrowIfNull(nameof(discoveryClient));

            this.defaultGateway = defaultGateway;
            this.discoveryClient = discoveryClient;
            this.LocalTypes = HostContext.Metadata?.RequestTypes ?? new HashSet<Type>();
        }

        public override IServiceGateway GetGateway(Type requestType)
        {
            //if (LocalTypes.Contains(requestType))
            //    return localGateway;
            // +		requestType.Assembly.GetName().Version	{2.0.1.0}	System.Version

            var service = discoveryClient.ResolveService(requestType);
            if (string.IsNullOrWhiteSpace(service?.Address))
            {
                throw new WebServiceException($"Could not resolve the uri in consul for external requestType {requestType.Name}");
            }

            // gateway creation delegate
            var serviceGateway = defaultGateway(service);

            // return if delegate is already using cachedclient
            if (serviceGateway is CachedServiceClient) return serviceGateway;

            // is http based client, if so, create cached client and use shared internal cache
            var serviceClient = serviceGateway as ServiceClientBase;
            return serviceClient == null ? serviceGateway : serviceClient.WithCache(sharedCache);
        }
    }
}