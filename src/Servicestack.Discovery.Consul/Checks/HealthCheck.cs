// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Discovery.Consul
{
    using ServiceStack.DataAnnotations;

    [Exclude(Feature.Metadata)]
    public class HealthCheck
    {
        public HealthCheck(ServiceHealth status = ServiceHealth.Ok, string message = null)
        {
            HealthResult = status;
            Message = message ?? status.ToString();
        }

        public ServiceHealth HealthResult { get; set; }

        public string Message { get; set; }
    }
}