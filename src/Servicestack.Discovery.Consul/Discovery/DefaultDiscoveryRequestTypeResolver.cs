// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Discovery.Consul
{
    using System;
    using System.Linq;

    using ServiceStack.DataAnnotations;

    public class DefaultDiscoveryRequestTypeResolver : IDiscoveryRequestTypeResolver
    {
        public string[] GetRequestTypes(IAppHost host)
        {
            // registered the requestDTO type names for the lookup
            // ignores types based on 
            // https://github.com/ServiceStack/ServiceStack/wiki/Add-ServiceStack-Reference#excluding-types-from-add-servicestack-reference
            var nativeTypes = host.GetPlugin<NativeTypesFeature>();

            var requestTypes =
                host.Metadata.RequestTypes
                    .Where(x => x.AllAttributes<ExcludeAttribute>().All(a => a.Feature != Feature.Metadata))
                    .Where(x => !nativeTypes.MetadataTypesConfig.IgnoreTypes.Contains(x))
                    .Where(x => !nativeTypes.MetadataTypesConfig.IgnoreTypesInNamespaces.Contains(x.Namespace))
                    // TODO Respect the RestrictAttribute
                    //.Where(x => x.AllAttributes<RestrictAttribute>().Any(a => a.VisibilityTo == RequestAttributes.External))
                    .ToArray();

            return requestTypes.Select(x => $"{ConsulFeatureSettings.TagDtoPrefix}{x.Name}").ToArray();
        }

        public string ResolveBaseUri(Type dtoType)
        {
            // handles all tag matching, healthy and lowest round trip time (rtt)
            // throws GatewayServiceDiscoveryException back to the Gateway 
            // to allow retry/exception handling at call site
            // TODO optional extra: set polly exception retry policy
            return ConsulClient.GetService(dtoType.Name)?.Address;
        }

        public string ResolveBaseUri(object dto)
        {
            return ResolveBaseUri(dto.GetType());
        }
    }
}