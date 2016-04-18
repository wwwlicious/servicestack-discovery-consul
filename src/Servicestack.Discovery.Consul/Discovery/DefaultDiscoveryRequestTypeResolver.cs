// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Discovery.Consul
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class DefaultDiscoveryRequestTypeResolver : IDiscoveryRequestTypeResolver
    {
        public HashSet<Type> ExportTypes { get; private set; }

        public string[] GetRequestTypes(IAppHost host)
        {
            // registered the requestDTO type names for the lookup
            // ignores types based on 
            // https://github.com/ServiceStack/ServiceStack/wiki/Add-ServiceStack-Reference#excluding-types-from-add-servicestack-reference
            var nativeTypes = host.GetPlugin<NativeTypesFeature>();

            ExportTypes =
                host.Metadata.RequestTypes
                    .WithServiceDiscoveryAllowed()
                    .WithoutNativeTypes(nativeTypes)
                    .WithoutExternalRestrictions()
                    .ToHashSet();
                    
            return ExportTypes.Select(x => $"{ConsulFeatureSettings.TagDtoPrefix}{x.Name}").ToArray(); 
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
            // TODO optional extra: set polly exception retry policy
            return ConsulClient.GetService(dtoType.Name)?.Address;
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