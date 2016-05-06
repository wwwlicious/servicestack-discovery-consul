// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace ServiceStack.Discovery.Consul
{
    using System.Collections.Generic;
    using System.Linq;

    using ServiceStack.FluentValidation;

    public class ConsulFeatureSettings
    {
        /// <summary>
        /// Prefix used when registering and looking up requestDTO's
        /// </summary>
        public const string GlobalServiceName = "servicestack";

        private readonly List<string> customTags = new List<string>();

        private readonly List<ConsulRegisterCheck> serviceChecks = new List<ConsulRegisterCheck>();

        private IDiscoveryRequestTypeResolver typeResolver = new DefaultDiscoveryRequestTypeResolver();

        private HostHealthCheck healthCheck;

        private DefaultGatewayDelegate defaultGateway = baseUri => new JsonServiceClient(baseUri);

        /// <summary>
        /// Set to false to exclude adding the default health checks on service registration
        /// </summary>
        public bool IncludeDefaultServiceHealth { get; set; } = true;
        
        /// <summary>
        /// Add custom service tags to your service registration
        /// </summary>
        /// <param name="tags"></param>
        public void AddTags(params string[] tags)
        {
            var t = tags.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            customTags.AddRange(t);
        }

        /// <summary>
        /// Add a service health check based on a HTTP or TCP endpoint
        /// </summary>
        /// <param name="checks">more or more consul health checks</param>
        public void AddServiceCheck(params ConsulRegisterCheck[] checks)
        {
            var validator = new ConsulRegisterCheckValidator();
            foreach (var check in checks)
            {
                validator.ValidateAndThrow(check);
            }

            this.serviceChecks.AddRange(checks);
        }

        /// <summary>
        /// Add a service health check based on a delegate
        /// </summary>
        /// <param name="check">a delegate to represent a service health check</param>
        /// <param name="intervalInSeconds">the timed interval for running this check in seconds</param>
        public void AddServiceCheck(HealthCheckDelegate check, int intervalInSeconds = 60)
        {
            healthCheck = new HostHealthCheck(check, intervalInSeconds);
        }

        /// <summary>
        /// Override the default discovery type resolver
        /// </summary>
        /// <param name="resolver">the type resolver</param>
        public void AddDiscoveryTypeResolver(IDiscoveryRequestTypeResolver resolver)
        {
            typeResolver = resolver;
        }

        /// <summary>
        /// Override the default remote service client
        /// </summary>
        /// <param name="externalGateway">the external gateway to use</param>
        public void SetDefaultGateway(DefaultGatewayDelegate externalGateway)
        {
            defaultGateway = externalGateway;
        }

        /// <summary>
        /// Get's the preferred external gateway for service discovery
        /// </summary>
        /// <returns></returns>
        public DefaultGatewayDelegate GetGateway()
        {
            return defaultGateway;
        }

        /// <summary>
        /// Gets the list of custom tags registered with the service
        /// </summary>
        /// <returns></returns>
        public string[] GetCustomTags()
        {
            return customTags.ToArray();
        }

        /// <summary>
        /// Gets the custom health check delegate for the service registration
        /// </summary>
        /// <returns></returns>
        public HostHealthCheck GetHealthCheck()
        {
            return healthCheck;
        }

        /// <summary>
        /// Gets the custom service checks for the service registration
        /// </summary>
        /// <returns></returns>
        public ConsulRegisterCheck[] GetServiceChecks()
        {
            return serviceChecks.ToArray();
        }

        /// <summary>
        /// Gets the type resolver used for service discovery
        /// </summary>
        /// <returns></returns>
        public IDiscoveryRequestTypeResolver GetDiscoveryTypeResolver()
        {
            return typeResolver;
        }
    }
}