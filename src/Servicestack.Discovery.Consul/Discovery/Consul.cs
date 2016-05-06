// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Discovery.Consul
{
    public static class Consul
    {
        public static string ResolveTypedUrl(IServiceClientMeta client, string method, object dto)
        {
            var resolver = HostContext.GetPlugin<ConsulFeature>().Settings.GetDiscoveryTypeResolver();
            var baseUri = resolver.ResolveBaseUri(dto);
            return (baseUri ?? client?.BaseUri)?.CombineWith(dto.ToUrl(method ?? "GET", client?.Format));
        }
    }
}