// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Discovery.Consul
{
    using System.Net;

    using ServiceStack.DataAnnotations;

    /// <summary>
    /// This service creates a simple heartbeat endpoint
    /// </summary>
    /// <remarks>The heartbeat is executed when this service is registered to obtain the baseUrl</remarks>
    [Exclude(Feature.Metadata)]
    public class HeartbeatService : Service
    {
        public Heartbeat Any(Heartbeat heartbeat)
        {
            heartbeat.StatusCode = (int)HttpStatusCode.OK;
            heartbeat.Url = Request.GetBaseUrl();
            return heartbeat;
        }
    }
}