// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Discovery.Consul
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents the response from a call to Consul
    /// </summary>
    public class ConsulHealthResponse
    {
        public Node Node { get; set; }

        public ConsulHealthService Service { get; set; }
    }

    [DataContract]
    public class Node
    {
        [DataMember(Name = "Node")]
        public string NodeName { get; set; }
    }

    public class ConsulHealthService
    {
        public string ID { get; set; }

        public string Service { get; set; }

        public string[] Tags { get; set; }

        public string Address { get; set; }

        public int Port { get; set; } = 0;
    }
}