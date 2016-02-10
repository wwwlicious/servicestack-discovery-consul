// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace ServiceStack.Discovery.Consul
{
    using System.Linq;

    using global::Consul;

    using ServiceStack;

    public static class ServiceDiscoveryClient
    {
        public static ServiceClientBase TryGetClientFor<T>(this ServiceClientBase client)
        {
            client = new JsonServiceClient();
            client.SetBaseUri(FindBaseUrlForDto<T>());
            return client.BaseUri.IsNullOrEmpty() ? null : client;
        }

        private static string FindBaseUrlForDto<T>()
        {
            var consulClient = new ConsulClient();
            string serviceUrl = null;

            var services = consulClient.Catalog.Services().Result.Response;
            var service = services.Where(x => x.Value.Contains(typeof(T).Name)).ToArray();
            if (service.Any())
            {
                var catalogServices = consulClient.Catalog.Service(service.First().Key).Result.Response;
                serviceUrl = catalogServices.First().ServiceAddress;
            }

            return serviceUrl;
        }
    }
}