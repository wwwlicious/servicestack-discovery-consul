// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Discovery.Consul
{
    using System;
    using System.Linq;
    using System.Net;

    using ServiceStack;
    using FluentValidation;
    using Logging;

    /// <summary>
    /// Consul client deals with consul api calls
    /// </summary>
    public static class ConsulClient
    {
        private static readonly ConsulRegisterCheckValidator HealthcheckValidator = new ConsulRegisterCheckValidator();
        private static readonly ConsulRegisterServiceValidator ServiceValidator = new ConsulRegisterServiceValidator();

        /// <summary>
        /// Registers the servicestack apphost with the local consul agent
        /// </summary>
        /// <exception cref="GatewayServiceDiscoveryException">throws exception if registration was not successful</exception>
        public static void RegisterService(ServiceRegistration registration)
        {
            var consulServiceRegistration = new ConsulServiceRegistration(registration.Id, registration.Name)
                                   {
                                        Address = registration.Address,
                                        Tags = registration.Tags,
                                        Port = registration.Port
                                   };

            ServiceValidator.ValidateAndThrow(consulServiceRegistration);

            var registrationUrl = ConsulUris.LocalAgent.CombineWith(consulServiceRegistration.ToPutUrl());
            registrationUrl.PostJsonToUrl(consulServiceRegistration, null,
                response =>
                {
                    var logger = LogManager.GetLogger(typeof(ConsulClient));
                    if (response.StatusCode.IsErrorResponse())
                    {
                        logger.Fatal(
                            $"Could not register appHost with Consul. It will not be discoverable: {consulServiceRegistration}");
                        throw new GatewayServiceDiscoveryException("Failed to register service with consul");
                    }
                    else
                    {
                        logger.Info($"Registered service with Consul {consulServiceRegistration}");
                        AppDomain.CurrentDomain.ProcessExit +=
                            (sender, args) => UnregisterService(consulServiceRegistration.ID);
                        AppDomain.CurrentDomain.UnhandledException +=
                            (sender, args) => UnregisterService(consulServiceRegistration.ID);
                    }
                });
        }

        /// <summary>
        /// Removes a service registation (and it's associated health checks) from consul
        /// </summary>
        /// <param name="serviceId">the id of the service to unregister</param>
        /// <exception cref="GatewayServiceDiscoveryException">throws exception if unregistration was not successful</exception>
        public static void UnregisterService(string serviceId)
        {
            ConsulUris.DeregisterService(serviceId).GetJsonFromUrl(
                null,
                response =>
                    {
                        var logger = LogManager.GetLogger(typeof(ConsulClient));
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            logger.Error($"Consul failed to unregister service `{serviceId}`");
                            throw new GatewayServiceDiscoveryException($"Failed to unregister service: {serviceId}");
                        }
                        else
                        {
                            logger.Debug($"Consul unregistered service: {serviceId}");
                        }
                    });
        }

        /// <summary>
        /// Returns a list of catalog services and tags
        /// </summary>
        /// <returns>service id's and tags</returns>
        /// <exception cref="GatewayServiceDiscoveryException">throws exception if unable to get services</exception>
        public static ConsulServiceResponse[] GetServices(string serviceName)
        {
            try
            {
                var response = ConsulUris.GetServices(serviceName).GetJsonFromUrl();

                if (string.IsNullOrWhiteSpace(response))
                    throw new WebServiceException(
                        $"Expected json but received empty or null reponse from {ConsulUris.GetServices(serviceName)}");

                return GetConsulServiceResponses(response);
            }
            catch (Exception e)
            {
                const string message = "Unable to retrieve services from Consul";
                LogManager.GetLogger(typeof(ConsulClient)).Error(message, e);
                throw new GatewayServiceDiscoveryException(message, e);
            }
        }

        /// <summary>
        /// Gets the service 
        /// </summary>
        /// <param name="serviceName">The global service name for servicestack services</param>
        /// <param name="tagName">the tagName to find the service for</param>
        /// <returns>The service for the tagName</returns>
        /// <exception cref="GatewayServiceDiscoveryException">throws exception if no service available for dto</exception>
        public static ConsulServiceResponse GetService(string serviceName, string tagName)
        {
            // todo add flag to allow warning services to be included in results

            // `passing` filters out any services with critical or warning health states
            // `near=_agent` sorts results by shortest round trip time
            var healthUri = ConsulUris.GetService(serviceName, tagName);
            try
            {
                var response = healthUri.GetJsonFromUrl();
                if (string.IsNullOrWhiteSpace(response))
                    throw new WebServiceException($"Expected json but received empty or null reponse from {healthUri}");

                return GetConsulServiceResponses(response).First();
            }
            catch (Exception e)
            {
                var message = $"No healthy services are currently registered to process the request of type '{tagName}'";
                LogManager.GetLogger(typeof(ConsulClient)).Error(message, e);
                throw new GatewayServiceDiscoveryException(message, e);
            }
        }

        /// <summary>
        /// Registers service health checks with consul
        /// </summary>
        /// <param name="healthChecks">the health checks to register</param>
        /// <exception cref="GatewayServiceDiscoveryException">throws exception if unable to register health checks</exception>
        public static void RegisterHealthChecks(params ServiceHealthCheck[] healthChecks)
        {
            var logger = LogManager.GetLogger(typeof(ConsulClient));

            foreach (var check in healthChecks)
            {
                try
                {
                    var consulCheck = new ConsulRegisterCheck(check.Id, check.ServiceId)
                    {
                        HTTP = check.Http,
                        TCP = check.Tcp,
                        IntervalInSeconds = check.IntervalInSeconds,
                        Notes = check.Notes,
                        DeregisterCriticalServiceAfterInMinutes = check.DeregisterCriticalServiceAfterInMinutes
                    };
                    HealthcheckValidator.ValidateAndThrow(consulCheck);

                    var registerUrl = consulCheck.ToPutUrl();
                    ConsulUris.LocalAgent.CombineWith(registerUrl).PutJsonToUrl(
                        consulCheck,
                        null,
                        response =>
                        {
                            if (response.IsErrorResponse())
                            {
                                logger.Error(
                                    $"Could not register health check ${check.Id} with Consul. {response.StatusDescription}");
                            }
                            else
                            {
                                logger.Info($"Registered health check with Consul `{check.Id}`");
                            }
                        });
                }
                catch (Exception ex)
                {
                    logger.Error("Failed to register health check", ex);
                    throw new GatewayServiceDiscoveryException($"Could not register service health check {check.Id}", ex);
                }
            }
        }

        private static ConsulServiceResponse[] GetConsulServiceResponses(string response)
        {
            var health = response.FromJson<ConsulHealthResponse[]>();
            return health.Select(ConsulServiceResponse.Create).ToArray();
        }
    }
}