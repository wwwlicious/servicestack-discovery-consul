// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace ServiceStack.Discovery.Consul
{
    using System.Linq;

    using ServiceStack.DataAnnotations;

    public class DefaultRequestTypeDiscoveryStrategy : IRequestTypeDiscoveryStrategy
    {
        public string[] GetRequestTypes(IAppHost host)
        {
            // registered the requestDTO type names for the lookup
            // ignores based on https://github.com/ServiceStack/ServiceStack/wiki/Add-ServiceStack-Reference#excluding-types-from-add-servicestack-reference
            var nativeTypes = host.GetPlugin<NativeTypesFeature>();
            var requestTypes =
                host.Metadata.RequestTypes
                    .Where(x => x.AllAttributes<ExcludeAttribute>().All(a => a.Feature != Feature.Metadata))
                    .Where(x => !nativeTypes.MetadataTypesConfig.IgnoreTypes.Contains(x))
                    .Where(x => !nativeTypes.MetadataTypesConfig.IgnoreTypesInNamespaces.Contains(x.Namespace))
                    //.Where(x => x.AllAttributes<RestrictAttribute>().Any(a => a.VisibilityTo == RequestAttributes.External))
                    .ToArray();

            return requestTypes.Select(x => x.Name).ToArray();
        }

        public string ResolveRequestType<T>()
        {
            return "a";
        }
    }
}