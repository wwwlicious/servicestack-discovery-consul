// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace ServiceStack.Discovery.Consul
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    using ServiceStack;
    using ServiceStack.Redis;
    using ServiceStack.FluentValidation;

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
        /// <param name="customTags">adds custom tags to the registration</param>
        /// <param name="includeDefaultServiceHealth"></param>
        /// <returns></returns>
        public static ConsulServiceRegistration RegisterService(IAppHost host, List<ConsulRegisterCheck> checks, List<string> customTags, bool includeDefaultServiceHealth)
        {
            // get endpoint http://url:port/path and version
            var baseUrl = host.Config.WebHostUrl.CombineWith(host.Config.HandlerFactoryPath);
            var version = "v{0}".Fmt(host.Config?.ApiVersion?.Replace('.', '-'));

            // build tags from request types
            var tags = new List<string> { version };
            tags.AddRange(DiscoveryRequestResolver.GetRequestTypes(host));
            tags.AddRange(customTags);

            var registration = new ConsulServiceRegistration(HostContext.ServiceName, version)
                                   {
                                        Address = baseUrl,
                                        Tags = tags.ToArray()
                                   };

            ServiceValidator.ValidateAndThrow(registration);

            var registrationUrl = ConsulUris.LocalAgent.CombineWith(registration.ToPutUrl());
            registrationUrl.PostJsonToUrlAsync(registration, null,
                response =>
                    {
                        var logger = host.Config.LogFactory.GetLogger(typeof(ConsulClient));
                        if (response.StatusCode.IsErrorResponse())
                        {
                            logger.Fatal($"Could not register appHost with Consul. It will not be discoverable {registration}");
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
                RegisterHealthCheck(check);
            }


            return registration;
        }

        public static void DeregisterService(ConsulServiceRegistration registration)
        {
            ConsulUris.DeregisterService(registration.ID).GetJsonFromUrl(
                null,
                response =>
                    {
                        var logger = HostConfig.Instance.LogFactory.GetLogger(typeof(ConsulClient));
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            logger.Error($"Consul failed to unregister service {registration}");
                        }
                        else
                        {
                            logger.Debug("Consul unregistered service");
                        }
                    });

        }

        private static void RegisterHealthCheck(ConsulRegisterCheck check)
        {
            HealthcheckValidator.ValidateAndThrow(check);

            ConsulUris.LocalAgent.CombineWith(check.ToPutUrl()).PostJsonToUrlAsync(
                check,
                null,
                response =>
                    {
                        var logger = HostConfig.Instance.LogFactory.GetLogger(typeof(ConsulClient));
                        if (response.IsErrorResponse())
                        {
                            logger.Error($"Could not register health check with Consul. {response.StatusDescription}");
                        }
                        else
                        {
                            logger.Info($"Registered health check with Consul {check}");
                        }
                    });
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
            var checks = new List<ConsulRegisterCheck>();
            appHost.RegisterService<HeartbeatService>();
            var heartbeatCheck = new ConsulRegisterCheck("heartbeat", serviceName)
            {
                IntervalInSeconds = 20,
                HTTP = baseUrl.CombineWith("/json/reply/heartbeat"),
                Notes = "This check is an HTTP GET request which expects the service to return 200 OK"
            };
            checks.Add(heartbeatCheck);

            // If redis is setup, add redis health check
            var clientsManager = appHost.TryResolve<IRedisClientsManager>();
            if (clientsManager != null)
            {
                using (var redisClient = clientsManager.GetReadOnlyClient())
                {
                    if (redisClient != null)
                    {
                        var redisHealthCheck = new ConsulRegisterCheck("redis", serviceName)
                        {
                            IntervalInSeconds = 10,
                            TCP = "{0}:{1}".Fmt(redisClient.Host, redisClient.Port),
                            Notes =  "This check ensures that redis is responding correctly"
                        };
                        checks.Add(redisHealthCheck);
                    }
                }
            }

            return checks.ToArray();
        }
    }
}