// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace ServiceStack.Discovery.Consul
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;

    using ServiceStack;
    using ServiceStack.Redis;

    public interface IRequestTypeDiscoveryStrategy
    {
        string[] GetRequestTypes(IAppHost host);

        string ResolveRequestType<T>();
    }

    public static class ConsulClient
    {
        /// <summary>
        /// Tags will be the main mechanism of discovery for any servicestack requestDTO's
        /// </summary>
        public static IRequestTypeDiscoveryStrategy TypeDiscoveryStrategy { get; set; } = new DefaultRequestTypeDiscoveryStrategy();

        public static ServiceClientBase TryGetClientFor<T>(this ServiceClientBase client, T type)
        {
            return TryGetClientFor<T>(client);
        }

        public static ServiceClientBase TryGetClientFor<T>(this ServiceClientBase client)
        {
            var getMeTheRemoteUrlFromConsul = FindBaseUrlForDto<T>();
            client.SetBaseUri(getMeTheRemoteUrlFromConsul);
            return client.BaseUri.IsNullOrEmpty() ? null : client;
        }

        /// <summary>
        /// Registers the servicestack apphost with the local consul agent
        /// </summary>
        /// <param name="host">the apphost to register with consul</param>
        /// <param name="checks">the health checks to associate with this service</param>
        /// <param name="customTags">adds custom tags to the registration</param>
        /// <param name="includeDefaultServiceHealth"></param>
        /// <returns></returns>
        public static void RegisterService(IAppHost host, List<ConsulRegisterCheck> checks, List<string> customTags, bool includeDefaultServiceHealth)
        {
            // get endpoint http://url:port/path and version
            var baseUrl = host.Config.WebHostUrl.CombineWith(host.Config.HandlerFactoryPath);
            var version = "v{0}".Fmt(host.Config?.ApiVersion?.Replace('.', '-'));

            // build tags from request types
            var tags = new List<string> { version };
            tags.AddRange(TypeDiscoveryStrategy.GetRequestTypes(host));
            tags.AddRange(customTags);

            Registration = new ConsulRegisterService(HostContext.ServiceName, version)
                                   {
                                        Address = baseUrl,
                                        Tags = tags.ToArray()
                                   };

            var registrationUrl = ConsulUris.Agent.CombineWith(Registration.ToPutUrl());
            registrationUrl.PostJsonToUrlAsync(Registration, null,
                response =>
                    {
                        var logger = host.Config.LogFactory.GetLogger(typeof(ConsulClient));
                        if (response.StatusCode.IsErrorResponse())
                        {
                            logger.Fatal($"Could not register appHost with Consul. It will not be discoverable {Registration}");
                        }
                        else
                        {
                            logger.Info($"Registered service with Consul {Registration}");
                            AppDomain.CurrentDomain.ProcessExit += (sender, args) => DeregisterService();
                            AppDomain.CurrentDomain.UnhandledException +=  (sender, args) => DeregisterService();
                        }
                    });

            if (includeDefaultServiceHealth)
            {
                foreach (var check in InitDefaultServiceChecks(host, baseUrl, Registration.ID))
                {
                    RegisterHealthCheck(check);
                }
            }

            foreach (var check in checks)
            {
                RegisterHealthCheck(check);
            }
        }

        /// <summary>
        /// Holds the current service's consul details
        /// </summary>
        public static ConsulRegisterService Registration { get; private set; }

        private static void RegisterHealthCheck(ConsulRegisterCheck check)
        {
            ConsulUris.Agent.CombineWith(check.ToPutUrl()).PostJsonToUrlAsync(
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

        public static void DeregisterService()
        {
            ConsulUris.DeregisterService(Registration.ID).GetJsonFromUrl(
                null,
                response =>
                    {
                        var logger = HostConfig.Instance.LogFactory.GetLogger(typeof(ConsulClient));
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            logger.Error($"Consul failed to unregister service {Registration}");
                        }
                        else
                        {
                            logger.Debug("Consul unregistered service");
                        }
                    });

        }

        private static string FindBaseUrlForDto<T>()
        {
            // strategy (filter out critical, perfer health over warning), 
            // TODO include acltoken filtering
            // use query - yes single call can find criteria or get say healthy vs warning services?
            
            var servicesJson = ConsulUris.GetServices.GetJsonFromUrl();
            var response = servicesJson.FromJson<ConsulServicesResponse>();

            // find services to serve request type
            var services = response.Services.Where(x => x.Tags.Contains(typeof(T).Name)).ToArray();
            if (services.Any())
            {
                // filter out any unhealthy services
                return services.First().Address;

                //var node = services.First();
                //var health = "http://127.0.0.1:8500/v1/health/checked/{0}".Fmt(node.Service).GetJsonFromUrl().FromJson<ConsulHealth>();
                //var healthyService = health.First(x => x.Status == HttpStatusCode.OK);
            }

            return "";
            //return !serviceUrl.IsNullOrEmpty() ? serviceUrl : null;


            //string serviceUrl = null;

            //var services = consulClient.Catalog.Services().Result.Response;
            //var service = services.Where(x => x.Value.Contains(typeof(T).Name)).ToArray();
            //if (service.Any())
            //{
            //    var catalogServices = consulClient.Catalog.Service(service.First().Key).Result.Response;
            //    serviceUrl = catalogServices.First().ServiceAddress;
            //}

            //return serviceUrl;
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
        /// <param name="baseUrl"></param>
        /// <returns>an array of agentservicecheck objects</returns>
        private static ConsulRegisterCheck[] InitDefaultServiceChecks(IAppHost appHost, string baseUrl, string serviceName)
        {
            var checks = new List<ConsulRegisterCheck>();
            appHost.RegisterService<HeartbeatService>();
            var heartbeatCheck = new ConsulRegisterCheck("heartbeat", serviceName)
            {
                IntervalInSeconds = 20,
                HTTP = baseUrl.CombineWith("/json/reply/heartbeat"),
                Notes = "This check is a GET HTTP request which expects the service to return 200 OK"
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

    public class ConsulUris
    {
        private static string baseUrl = "http://127.0.0.1:8500";

        public static readonly Func<string, string> DeregisterService = serviceId => $"{baseUrl}/v1/agent/service/deregister/{serviceId}";

        public static readonly Func<string, string> GetService = serviceName => $"{baseUrl}/v1/agent/service/{serviceName}";

        public static readonly string GetServices = $"{baseUrl}/v1/agent/services";

        public static readonly string Agent = "http://127.0.0.1:8500";
    }

    public class ConsulHealth
    {
        public ConsulHealthStatus[] Checks { get; set; }
    }

    public class ConsulHealthStatus
    {
        public string ServiceName { get; set; }

        public HttpStatusCode Status { get; set; }
    }

    public struct ConsulServicesResponse
    {
        public ConsulService[] Services { get; set; }

        public override string ToString()
        {
            return $"{Services.Select(x => x.ToJson())},";
        }

        public static ConsulServicesResponse ParseJson(string json)
        {
            var services = json.TrimStart('{').TrimEnd('}').Split(',');
            return new ConsulServicesResponse
                       {
                           Services =
                               services.Select(
                                   x => x.FromJson<ConsulService>())
                               .ToArray()
                       };
        }

        public struct ConsulService
        {
            public string ID { get; set; }

            public string Service { get; set; }

            public string[] Tags { get; set; }

            public string Address { get; set; }

            public int Port { get; set; }
        }

        /// <summary>
        /// The dynamic json key requires custom deserialization
        /// </summary>
        public struct ConsulServices
        {
            public ConsulService[] Services { get; set; }

            public override string ToString()
            {
                var sb = new StringBuilder();
                foreach (var service in Services)
                {
                    sb.Append($"{{ \"{service.ID}\" : {service.ToJson()} }}");
                }
                return sb.ToString();
            }

            public static ConsulService ParseJson(string json)
            {
                return json.Split(':')[1].FromJson<ConsulService>();
            }
        }

        public class ConsulServiceResponse
        {
            public ConsulNode[] Nodes { get; set; }
        }

        public class ConsulNode
        {
            public string ServiceId { get; set; }

            public string ServiceAddress { get; set; }
        }
    }
}