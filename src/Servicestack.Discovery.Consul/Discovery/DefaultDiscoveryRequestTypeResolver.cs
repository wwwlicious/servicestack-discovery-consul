// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Discovery.Consul
{
    using System;
    using System.Linq;

    using ServiceStack.DataAnnotations;
    using ServiceStack.Text;

    public class DefaultDiscoveryRequestTypeResolver : IDiscoveryRequestTypeResolver
    {
        public string[] GetRequestTypes(IAppHost host)
        {
            // registered the requestDTO type names for the lookup
            // ignores types based on https://github.com/ServiceStack/ServiceStack/wiki/Add-ServiceStack-Reference#excluding-types-from-add-servicestack-reference
            var nativeTypes = host.GetPlugin<NativeTypesFeature>();
            var requestTypes =
                host.Metadata.RequestTypes
                    .Where(x => PlatformExtensions.AllAttributes<ExcludeAttribute>((Type)x).All(a => a.Feature != Feature.Metadata))
                    .Where(x => !nativeTypes.MetadataTypesConfig.IgnoreTypes.Contains(x))
                    .Where(x => !nativeTypes.MetadataTypesConfig.IgnoreTypesInNamespaces.Contains(x.Namespace))
                    // TODO Respect the RestrictAttribute
                    //.Where(x => x.AllAttributes<RestrictAttribute>().Any(a => a.VisibilityTo == RequestAttributes.External))
                    .ToArray();

            return requestTypes.Select(x => x.Name).ToArray();
        }

        public string ResolveRequestType<T>()
        {
            // strategy (filter out critical, perfer health over warning), 
            // TODO include acltoken filtering
            // NOTE can use consul query to make a single call can find criteria (tag match and healthy vs warning services)
            var servicesJson = ConsulUris.GetServices.GetJsonFromUrl();
            try
            {
                // find services to serve request type
                var services = JsonObject.Parse(servicesJson)
                        .Select(x => x.Value.FromJson<ConsulServiceResponse>())
                        .ToArray();
                var matches = services.Where(x => x.Tags.Contains(typeof(T).Name)).ToArray();
                if (matches.Any())
                {
                    // TODO filter out any unhealthy services
                    return matches.First().Address;
                }
                return null;
            }
            catch (Exception e)
            {
                Logging.LogManager.GetLogger(typeof(ConsulClient)).Error($"Could not find service for {typeof(T).Name}", e);
                throw;
            }
        }
    }
}