// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Discovery.Consul
{
    public class HostHealthCheck
    {
        /// <summary>
        /// Registers a delegate to run as a service health check
        /// </summary>
        /// <param name="healthCheckDelegate"></param>
        /// <param name="intervalInSeconds"></param>
        /// <param name="deregisterIfCriticalAfterInMinutes"></param>
        public HostHealthCheck(HealthCheckDelegate healthCheckDelegate, int intervalInSeconds = 60, int? deregisterIfCriticalAfterInMinutes = null)
        {
            healthCheckDelegate.ThrowIfNull(nameof(healthCheckDelegate));
            
            HealthCheckDelegate = healthCheckDelegate;
            IntervalInSeconds = intervalInSeconds;
            DeregisterIfCriticalAfterInMinutes = deregisterIfCriticalAfterInMinutes;
        }

        /// <summary>
        /// the delegate to run to check appHost health
        /// </summary>
        public HealthCheckDelegate HealthCheckDelegate { get; set; }

        /// <summary>
        /// How often the check is run, defaults to 60 seconds
        /// </summary>
        public int IntervalInSeconds { get; set; }

        /// <summary>
        /// If health check is failing after x minutes, will deregister service, leave as default(null) to disable
        /// </summary>
        public int? DeregisterIfCriticalAfterInMinutes { get; set; }
    }
}