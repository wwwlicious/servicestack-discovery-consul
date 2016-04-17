// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Discovery.Consul
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    using ServiceStack;
    using ServiceStack.FluentValidation;
    using ServiceStack.Logging;
    using ServiceStack.Redis;
    using ServiceStack.Text;
    using ServiceStack.Text.Json;

    public static class ConsulClient
    {
        private static readonly ConsulRegisterCheckValidator HealthcheckValidator = new ConsulRegisterCheckValidator();
        private static readonly ConsulRegisterServiceValidator ServiceValidator = new ConsulRegisterServiceValidator();

        /// <summary>
        /// Tags are the main mechanism of discovery for any servicestack requestDTO's
        /// </summary>
        public static IDiscoveryRequestTypeResolver DiscoveryRequestResolver { get; set; }

        /// <summary>
        /// Registers the servicestack apphost with the local consul agent
        /// </summary>
        /// <param name="host">the apphost to register with consul</param>
        /// <param name="checks">the health checks to associate with this service</param>
        /// <param name="healthCheck"></param>
        /// <param name="customTags">adds custom tags to the registration</param>
        /// <param name="includeDefaultServiceHealth"></param>
        /// <returns></returns>
        public static ConsulServiceRegistration RegisterService(IAppHost host, ConsulRegisterCheck[] checks, HostHealthCheck healthCheck, string[] customTags, bool includeDefaultServiceHealth)
        {
            // get endpoint http://url:port/path and version
            var baseUrl = host.Config.WebHostUrl.CombineWith(host.Config.HandlerFactoryPath);

            // TODO for sorting by versioning to work, any registered version tag must be numeric
            // option 1: use ApiVersion but throw exception to stop host if it is not numeric
            // option 2: use a dedicated numeric version property which defaults to 1.0
            // option 3: use the appost's assembly version
            var version = "v{0}".Fmt(host.Config?.ApiVersion?.Replace('.', '-'));

            host.RegisterService<HealthCheckService>();

            // build tags from request types
            var tags = new List<string> { version, "ServiceStack" };
            tags.AddRange(DiscoveryRequestResolver.GetRequestTypes(host));
            tags.AddRange(customTags);

            var registration = new ConsulServiceRegistration($"SS-{HostContext.ServiceName}", version)
                                   {
                                        Address = baseUrl,
                                        Tags = tags.ToArray()
                                   };

            ServiceValidator.ValidateAndThrow(registration);

            var registrationUrl = ConsulUris.LocalAgent.CombineWith(registration.ToPutUrl());
            registrationUrl.PostJsonToUrl(registration, null,
                response =>
                    {
                        var logger = LogManager.GetLogger(typeof(ConsulClient));
                        if (response.StatusCode.IsErrorResponse())
                        {
                            logger.Fatal($"Could not register appHost with Consul. It will not be discoverable: {registration}");
                        }
                        else
                        {
                            logger.Info($"Registered service with Consul {registration}");
                            AppDomain.CurrentDomain.ProcessExit += (sender, args) => DeregisterService(registration);
                            AppDomain.CurrentDomain.UnhandledException +=  (sender, args) => DeregisterService(registration);
                        }
                    });

            if (includeDefaultServiceHealth)
            {
                foreach (var check in InitDefaultServiceChecks(host, baseUrl, registration.ID))
                {
                    RegisterHealthCheck(check);
                }
            }

            foreach (var check in checks)
            {
                check.ServiceID = registration.ID;
                RegisterHealthCheck(check);
            }

            if (healthCheck != null)
            {
                RegisterHealthCheck(new ConsulRegisterCheck("SS-HealthCheck", registration.ID)
                {
                    IntervalInSeconds = healthCheck.IntervalInSeconds,
                    HTTP = baseUrl.CombineWith("/json/reply/healthcheck"),
                    Notes = "This check is an HTTP GET request which expects the service to return 200 OK"
                });
            }

            return registration;
        }

        public static void DeregisterService(ConsulServiceRegistration registration)
        {
            ConsulUris.DeregisterService(registration.ID).GetJsonFromUrl(
                null,
                response =>
                    {
                        var logger = LogManager.GetLogger(typeof(ConsulClient));
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            logger.Error($"Consul failed to unregister service `{registration}`");
                        }
                        else
                        {
                            logger.Debug("Consul unregistered service");
                        }
                    });
        }

        private static void RegisterHealthCheck(ConsulRegisterCheck check)
        {
            var logger = LogManager.GetLogger(typeof(ConsulClient));
            try
            {
                HealthcheckValidator.ValidateAndThrow(check);

                ConsulUris.LocalAgent.CombineWith(check.ToPutUrl()).PostJsonToUrlAsync(
                    check,
                    null,
                    response =>
                    {
                        
                        if (response.IsErrorResponse())
                        {
                            logger.Error($"Could not register health check with Consul. {response.StatusDescription}");
                        }
                        else
                        {
                            logger.Info($"Registered health check with Consul `{check}`");
                        }
                    });
            }
            catch (Exception ex)
            {
                logger.Error("Failed to register health check", ex);
            }
        }

        /// <summary>
        /// Checks are tcp or http pings that can return a status, default status is ok
        /// The default check is just a basic heartbeat api call and if redis is configured a tcp call to
        /// it's endpoint
        /// </summary>
        /// <remarks>
        /// other possible default checks might be 
        ///     cpu load
        ///     diskspace
        ///     ssl cert expiry
        ///     api usage stats?
        /// </remarks>
        /// <param name="appHost">the current apphost</param>
        /// <param name="baseUrl">the servicestack hosted uri</param>
        /// <param name="serviceName">the servicestack service name</param>
        /// <returns>an array of agentservicecheck objects</returns>
        private static ConsulRegisterCheck[] InitDefaultServiceChecks(IAppHost appHost, string baseUrl, string serviceName)
        {
            var heartbeatCheck = CreateHeartbeatCheck(baseUrl, serviceName);
            var checks = new List<ConsulRegisterCheck> { heartbeatCheck };

            // If redis is setup, add redis health check
            var clientsManager = appHost.TryResolve<IRedisClientsManager>();
            if (clientsManager != null)
            {
                try
                {
                    using (var redisClient = clientsManager.GetReadOnlyClient())
                    {
                        if (redisClient != null)
                        {
                            var redisHealthCheck = new ConsulRegisterCheck("SS-Redis", serviceName)
                            {
                                IntervalInSeconds = 10,
                                TCP = "{0}:{1}".Fmt(redisClient.Host, redisClient.Port),
                                Notes = "This check ensures that redis is responding correctly"
                            };
                            checks.Add(redisHealthCheck);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger(typeof(ConsulClient)).Error("Could not create a redis connection from the registered IRedisClientsManager, skipping consul health check", ex);
                }
            }

            // TODO Setup health checks for any registered IDbConnectionFactories

            return checks.ToArray();
        }

        private static ConsulRegisterCheck CreateHeartbeatCheck(string baseUrl, string serviceName)
        {
            return new ConsulRegisterCheck("SS-HeartBeat", serviceName)
            {
                IntervalInSeconds = 30,
                HTTP = baseUrl.CombineWith("/json/reply/heartbeat"),
                Notes = "A heartbeat service to check if the service is reachable, expects 200 response"
            };
        }

        /// <summary>
        /// Returns a list of catalog services and tags
        /// </summary>
        /// <returns>service id's and tags</returns>
        public static ConsulCatalogServiceResponse[] GetServices()
        {
            try
            {
                var servicesJson = ConsulUris.GetServices.GetJsonFromUrl();

                if (string.IsNullOrWhiteSpace(servicesJson))
                    throw new WebServiceException(
                        $"Expected json but received empty or null reponse from {ConsulUris.GetServices}");

                return JsonObject.Parse(servicesJson)
                    .Select(x => new ConsulCatalogServiceResponse { ID = x.Key, Tags = x.Value.FromJson<string[]>()})
                    .ToArray();
            }
            catch (Exception e)
            {
                const string message = "Unable to retrieve services from Consul";
                LogManager.GetLogger(typeof(ConsulClient)).Error(message, e);
                throw new GatewayServiceDiscoveryException(message, e);
            }
        }

        /// <summary>
        /// Returns all matching services for a tag
        /// </summary>
        /// <param name="dtoName"></param>
        /// <returns></returns>
        public static ConsulCatalogServiceResponse[] FindService(string dtoName)
        {
            // filter by tags, servicestack tag is autoregistered with service and used as an additional filter
            // todo collapse identical service ID's
            return
                GetServices()
                    .Where(
                        x =>
                            x.Tags.Contains($"{ConsulFeatureSettings.TagDtoPrefix}{dtoName}") &&
                            x.Tags.Contains("ServiceStack"))
                    .ToArray();
        }

        /// <summary>
        /// Gets the service 
        /// </summary>
        /// <param name="dtoName"></param>
        public static ConsulServiceResponse GetService(string dtoName)
        {
            // todo once service versioning functionality is in place
            //  should use strategy high > low or low > high to sort services
            var service = FindService(dtoName).FirstOrDefault();

            if(service == null)
                throw new GatewayServiceDiscoveryException($"No services are currently registered to process the request of type '{dtoName}'");

            // `passing` filters out any services with critical or warning health states
            // todo add flag to allow warning services to be included in results
            // `near=_agent` sorts results by shortest round trip time
            var healthUri = ConsulUris.LocalAgent.CombineWith($"/v1/health/service/{service.ID}?near=_agent&passing&tag={ConsulFeatureSettings.TagDtoPrefix}{dtoName}");
            try
            {
                var response = healthUri.GetJsonFromUrl();
                if (string.IsNullOrWhiteSpace(response))
                    throw new WebServiceException($"Expected json but received empty or null reponse from {healthUri}");

                var serviceResponse = JsonObject.ParseArray(response);
                var svc = serviceResponse[0]["Service"].FromJsv<ConsulServiceResponse>();

                // parse response
                return svc;
            }
            catch (Exception e)
            {
                var message = $"No healthy services are currently registered to process the request of type '{dtoName}'";
                LogManager.GetLogger(typeof(ConsulClient)).Error(message, e);
                throw new GatewayServiceDiscoveryException(message, e);
            }
        }
    }
}