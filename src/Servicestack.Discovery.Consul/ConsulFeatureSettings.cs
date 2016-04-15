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
        public const string TagDtoPrefix = "req-";

        private readonly List<string> customTags = new List<string>();

        private readonly List<ConsulRegisterCheck> serviceChecks = new List<ConsulRegisterCheck>();

        private IDiscoveryRequestTypeResolver typeResolver = new DefaultDiscoveryRequestTypeResolver();

        private HostHealthCheck healthCheck;

        private DefaultGatewayDelegate defaultGateway = baseUri => new JsonServiceClient(baseUri);

        public bool IncludeDefaultServiceHealth { get; set; } = true;
        
        /// <summary>
        /// Add service tags
        /// </summary>
        /// <param name="tags"></param>
        public void AddTags(params string[] tags)
        {
            // prefix is used to prevent and check for request name collisions between services
            var t = tags.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

            if(t.Any(x => x.StartsWith(TagDtoPrefix)))
                throw new InvalidTagException($"custom tags cannot use the reserved prefix '{TagDtoPrefix}'");

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

        public DefaultGatewayDelegate GetGateway()
        {
            return defaultGateway;
        }

        public string[] GetCustomTags()
        {
            return customTags.ToArray();
        }

        public HostHealthCheck GetHealthCheck()
        {
            return healthCheck;
        }

        public ConsulRegisterCheck[] GetServiceChecks()
        {
            return serviceChecks.ToArray();
        }

        public IDiscoveryRequestTypeResolver GetDiscoveryTypeResolver()
        {
            return typeResolver;
        }
    }
}