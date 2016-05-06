// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Discovery.Consul
{
    /// <summary>
    /// Represents a consul registered service address
    /// </summary>
    public class ConsulServiceResponse
    {
        public string Node { get; set; }

        public string ServiceID { get; set; }

        public string ServiceName { get; set; }

        public string[] ServiceTags { get; set; }

        public string ServiceAddress { get; set; }

        public int ServicePort { get; set; } = 0;
    }
}