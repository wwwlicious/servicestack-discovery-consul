// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace ServiceStack.Discovery.Consul
{
    using ServiceStack.FluentValidation;
    using ServiceStack.Logging;

    public class DiscoveryService : Service
    {
        private GetServiceValidator GetServiceValidator { get; set; } = new GetServiceValidator();

        // return current registration and health info
        public IServiceDiscovery<ConsulService, ServiceRegistration> Discovery { get; set; }

        public object Any(ServiceRegistration registration)
        {
            return Discovery.Registration;
        }

        /// <summary>
        /// Gets the available consul services
        /// </summary>
        public object Any(GetServices request)
        {
            var allServices = Discovery.GetServices(Discovery.Registration.Name);
            return new GetServicesResponse { Services = allServices };
        }

        /// <summary>
        /// Gets the service for an operation
        /// </summary>
        public object Any(GetService request)
        {
            // we can't rely on AppHost having validation plugin configured so just manually check
            GetServiceValidator.ValidateAndThrow(request);
            var response = Discovery.GetService(Discovery.Registration.Name, request.DtoName);
            return new GetServiceResponse(response);
        }
    }
}