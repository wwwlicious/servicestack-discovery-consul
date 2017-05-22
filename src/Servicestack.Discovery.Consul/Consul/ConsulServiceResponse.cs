// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
using System.Collections.Generic;

namespace ServiceStack.Discovery.Consul
{
    /// <summary>
    /// Represents a consul registered service address
    /// </summary>
    public class ConsulServiceResponse
    {
        public string Node { get; private set; }

        public string ServiceID { get; private set; }

        public string ServiceName { get; private set; }

        public string[] ServiceTags { get; private set; }

        public string ServiceAddress { get; private set; }

        public int ServicePort { get; private set; }

        public static ConsulServiceResponse Create(ConsulHealthResponse response)
        {
            if (response?.Node == null || (response.Service == null))
                return null;

            return new ConsulServiceResponse
            {
                Node = response.Node.NodeName,
                ServiceID = response.Service.ID,
                ServiceName = response.Service.Service,
                ServiceTags = response.Service.Tags,
                ServiceAddress = response.Service.Address,
                ServicePort = response.Service.Port
            };
        }

        public static ConsulServiceResponse Create(KeyValuePair<string, List<string>> services)
        {

            return new ConsulServiceResponse
            {

                ServiceName = services.Key,
                ServiceTags = services.Value.ToArray(),
            };
        }
    }
}