// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Discovery.Consul
{
    using System;
    using System.Collections.Generic;

    public class DefaultDiscoveryRequestTypeResolver : IDiscoveryRequestTypeResolver
    {
        public IDiscovery Discovery { get; set; }

        public HashSet<Type> GetRequestTypes(IAppHost host)
        {
            // registered the requestDTO type names for the lookup
            // ignores types based on 
            // https://github.com/ServiceStack/ServiceStack/wiki/Add-ServiceStack-Reference#excluding-types-from-add-servicestack-reference
            var nativeTypes = host.GetPlugin<NativeTypesFeature>();

            return 
                host.Metadata.RequestTypes
                    .WithServiceDiscoveryAllowed()
                    .WithoutNativeTypes(nativeTypes)
                    .ToHashSet();
        }

        /// <summary>
        /// handles all tag matching, healthy and lowest round trip time (rtt)
        /// </summary>
        /// <exception cref="GatewayServiceDiscoveryException">If service is not found or unavailable</exception>
        /// <param name="dtoType">The request DTO type</param>
        /// <returns>the uri for the service to send the request to</returns>
        public string ResolveBaseUri(Type dtoType)
        {
            // handles all tag matching, healthy and lowest round trip time (rtt)
            // throws GatewayServiceDiscoveryException back to the Gateway 
            // to allow retry/exception handling at call site
            return Discovery.GetService(Discovery.Registration.Name, dtoType.Name)?.Address;
        }

        /// <summary>
        /// handles all tag matching, healthy and lowest round trip time (rtt)
        /// </summary>
        /// <exception cref="GatewayServiceDiscoveryException">If service is not found or unavailable</exception>
        /// <param name="dto">The request DTO</param>
        /// <returns>the uri for the service to send the request to</returns>
        public string ResolveBaseUri(object dto)
        {
            return ResolveBaseUri(dto.GetType());
        }
    }
}