// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace ServiceStack.Discovery.Consul
{
    public static class Consul
    {
        public static string ResolveTypedUrl(IServiceClientMeta client, string method, object dto)
        {
            var baseUri = ConsulClient.DiscoveryRequestResolver.ResolveBaseUri(dto);
            return baseUri?.CombineWith(dto.ToRelativeUri(method)) ?? client?.BaseUri;
        }
    }
}