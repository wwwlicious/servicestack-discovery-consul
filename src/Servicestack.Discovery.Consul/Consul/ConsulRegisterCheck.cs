// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Discovery.Consul
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents a consul health check
    /// </summary>
    [Route("/v1/agent/check/register", "PUT")]
    public class ConsulRegisterCheck : IUrlFilter
    {
        /// <summary>
        /// Creates a new health check in consul
        /// </summary>
        /// <param name="name">The name to use for the health check in consul</param>
        /// <param name="serviceId">The Id of service to associate this health check with</param>
        public ConsulRegisterCheck(string name, string serviceId = null)
        {
            Name = name;
            ServiceID = serviceId;

            ID = string.IsNullOrWhiteSpace(serviceId) ? Name : $"{ServiceID}:{Name}";
        }

        /// <summary>
        /// Defaults to the ServiceName:Name
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// The name of the agent check
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The service name to associate the check with, leave blank for agent check
        /// </summary>
        public string ServiceID { get; set; }

        /// <summary>
        /// User notes on the check
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Check to execute a script on the host
        /// </summary>
        public string Script { get; set; }

        public string DockerContainerID { get; set; }

        /// <summary>
        /// Reference a shell script relative to the consul agent path
        /// </summary>
        public string Shell { get; set; }

        /// <summary>
        /// the GET HTTP URL consul will query
        /// </summary>
        public string HTTP { get; set; }

        /// <summary>
        /// the TCP IP:PORT address consul will query
        /// </summary>
        public string TCP { get; set; }

        /// <summary>
        /// the frequency with which consul will run the health check
        /// </summary>
        [IgnoreDataMember]
        public double? IntervalInSeconds { get; set; }

        public string Interval => IntervalInSeconds?.ToString("0s");

        [IgnoreDataMember]
        public string AclToken { get; set; }

        public string ToUrl(string absoluteUrl)
        {
            return string.IsNullOrWhiteSpace(AclToken) ? absoluteUrl : absoluteUrl.AddQueryParam("token", AclToken);
        }
    }
}