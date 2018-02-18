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
        /// <summary>
        /// The consul node
        /// </summary>
        public Node Node { get; set; }

        /// <summary>
        /// The health service information
        /// </summary>
        public ConsulHealthService Service { get; set; }
    }

    /// <summary>
    /// A consul node
    /// </summary>
    [DataContract]
    public class Node
    {
        /// <summary>
        /// The node name
        /// </summary>
        [DataMember(Name = "Node")]
        public string NodeName { get; set; }
    }

    /// <summary>
    /// Dto for consul health service information
    /// </summary>
    public class ConsulHealthService
    {
        /// <summary>
        /// The service identifier
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// The service name
        /// </summary>
        public string Service { get; set; }

        /// <summary>
        /// the service tags
        /// </summary>
        public string[] Tags { get; set; }

        /// <summary>
        /// the service address
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// the service port
        /// </summary>
        public int Port { get; set; } = 0;
    }
}