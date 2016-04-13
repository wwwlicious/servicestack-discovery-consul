// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace ServiceStack.Discovery.Consul
{
    using System;
    using System.Collections.Generic;

    public class ConsulServiceGatewayFactory : ServiceGatewayFactoryBase
    {
        private readonly ConsulFeature.DefaultGatewayDelegate defaultGateway;

        private readonly IDiscoveryRequestTypeResolver typeResolver;

        public HashSet<Type> LocalTypes { get; set; }

        public ConsulServiceGatewayFactory(ConsulFeature.DefaultGatewayDelegate defaultGateway, IDiscoveryRequestTypeResolver typeResolver)
        {
            defaultGateway.ThrowIfNull(nameof(defaultGateway));
            typeResolver.ThrowIfNull(nameof(typeResolver));

            this.defaultGateway = defaultGateway;
            this.typeResolver = typeResolver;
            this.LocalTypes = HostContext.Metadata?.RequestTypes ?? new HashSet<Type>();
        }

        public override IServiceGateway GetGateway(Type requestType)
        {
            if (LocalTypes.Contains(requestType))
                return localGateway;

            var baseUri = typeResolver.ResolveBaseUri(requestType);
            if (string.IsNullOrWhiteSpace(baseUri))
            {
                throw new WebServiceException($"Could not resolve the uri in consul for external requestType {requestType.Name}");
            }

            return defaultGateway(baseUri);
        }
    }
}