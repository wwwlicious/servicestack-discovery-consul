// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Discovery.Consul
{
    using System;
    using System.Net;

    using DataAnnotations;

    /// <summary>
    /// This service creates health endpoints for consul to issue requests to
    /// </summary>
    /// <remarks>The heartbeat is executed when this service is registered to obtain the baseUrl</remarks>
    [Exclude(Feature.Metadata)]
    public class HealthCheckService : Service
    {
        /// <summary>
        /// Provides a simple heartbeat endpoint
        /// </summary>
        /// <remarks>The heartbeat is executed when this service is registered to obtain the baseUrl</remarks>
        public object Get(Heartbeat heartbeat)
        {
            return new Heartbeat { Url = Request.GetBaseUrl() };
        }

        /// <summary>
        /// Provides a custom healthcheck endpoint for each service to configure
        /// </summary>
        /// <remarks>The healthcheck if specified at plugin registration is executed</remarks>
        public object Get(HealthCheck check)
        {
            var healthCheck = HostContext.GetPlugin<ConsulFeature>().Settings.GetHealthCheck();
            if (healthCheck == null) return new HealthCheck(ServiceHealth.Ok, "Not implemented");

            try
            {
                // based on https://www.consul.io/docs/agent/checks.html
                var result = healthCheck.HealthCheckDelegate.Invoke(HostContext.AppHost);
                switch (result.HealthResult)
                {
                    case ServiceHealth.Warning:
                        // return nonstandard code http://stackoverflow.com/a/22645395/191877
                        return new HttpError((System.Net.HttpStatusCode)429, result.Message);
                    case ServiceHealth.Critical:
                        return new HttpError(HttpStatusCode.ServiceUnavailable, result.Message);
                }
                return result;
            }
            catch (Exception ex)
            {
                return new HttpError(HttpStatusCode.ServiceUnavailable, $"Health check threw unexpected exception {ex}");
            }
        }
    }
}