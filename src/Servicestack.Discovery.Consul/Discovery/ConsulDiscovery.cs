// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace ServiceStack.Discovery.Consul
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ServiceStack.Logging;
    using ServiceStack.Redis;

    /// <summary>
    /// Manages register, unregister, holds registration state
    /// </summary>
    public class ConsulDiscovery : IDiscovery
    {
        /// <summary>
        /// Contains the service registration information
        /// </summary>
        public ServiceRegistration Registration { get; private set; }

        /// <summary>
        /// Registers the apphost with consul
        /// </summary>
        /// <param name="appHost"></param>
        public void Register(IAppHost appHost)
        {
            // get endpoint http://url:port/path and version
            var baseUrl = appHost.Config.WebHostUrl.CombineWith(appHost.Config.HandlerFactoryPath);
            var dtoTypes = GetRequestTypes(appHost);
            var customTags = appHost.GetPlugin<ConsulFeature>().Settings.GetCustomTags();

            // construct registration 
            var registration = new ServiceRegistration
            {
                Name = "api",
                Id = $"ss-{HostContext.ServiceName}-{Guid.NewGuid()}",
                Address = baseUrl,
                Version = GetVersion(),
                Port = GetPort(baseUrl)
            };

            // build the service tags
            var tags = new List<string> { $"ss-version-{registration.Version}" };
            tags.AddRange(dtoTypes.Select(x => x.Name));
            tags.AddRange(customTags);
            registration.Tags = tags.ToArray();
            
            // register the service and healthchecks with consul
            ConsulClient.RegisterService(registration);
            var heathChecks = CreateHealthChecks(registration);
            ConsulClient.RegisterHealthChecks(heathChecks);
            registration.HealthChecks = heathChecks;

            // TODO Generate warnings if dto's have [Restrict(RequestAttributes.Secure)] 
            // but are being registered without an https:// baseUri

            // TODO for sorting by versioning to work, any registered version tag must be numeric
            // option 1: use ApiVersion but throw exception to stop host if it is not numeric
            // option 2: use a dedicated numeric version property which defaults to 1.0
            // option 3: use the appost's assembly version
            //var version = "v{0}".Fmt(host.Config?.ApiVersion?.Replace('.', '-'));

            // assign if self-registration was successful
            Registration = registration;
        }

        /// <summary>
        /// Unregistered the apphost with consul
        /// </summary>
        /// <param name="appHost"></param>
        public void Unregister(IAppHost appHost)
        {
            if (Registration == null) return;

            ConsulClient.UnregisterService(Registration.Id);
            Registration = null;
        }

        public ConsulService[] GetServices(string serviceName)
        {
            var response = ConsulClient.GetServices(serviceName);
            return response.Select(x => new ConsulService(x)).ToArray();
        }

        public ConsulService GetService(string serviceName, string dtoName)
        {
            var response = ConsulClient.GetService(serviceName, dtoName);
            return new ConsulService(response);
        }

        public HashSet<Type> GetRequestTypes(IAppHost host)
        {
            // registered the requestDTO type names for the lookup
            // ignores types based on 
            // https://github.com/ServiceStack/ServiceStack/wiki/Add-ServiceStack-Reference#excluding-types-from-add-servicestack-reference
            var nativeTypes = host.GetPlugin<NativeTypesFeature>();

            return
                host.Metadata.RequestTypes
                    .WithServiceDiscoveryAllowed()
                    .WithoutNativeTypes(nativeTypes)
                    .ToHashSet();
        }

        public string ResolveBaseUri(object dto)
        {
            return ResolveBaseUri(dto.GetType());
        }

        public string ResolveBaseUri(Type dtoType)
        {
            // handles all tag matching, healthy and lowest round trip time (rtt)
            // throws GatewayServiceDiscoveryException back to the Gateway 
            // to allow retry/exception handling at call site
            return GetService(Registration.Name, dtoType.Name)?.Address;
        }

        private ServiceHealthCheck[] CreateHealthChecks(ServiceRegistration registration)
        {
            var checks = new List<ServiceHealthCheck>();
            var serviceId = registration.Id;
            var baseUrl = registration.Address;

            var settings = HostContext.GetPlugin<ConsulFeature>().Settings;
            if (settings.IncludeDefaultServiceHealth)
            {
                var heartbeatCheck = CreateHeartbeatCheck(baseUrl, serviceId);
                checks.Add(heartbeatCheck);

                var redisCheck = CreateRedisCheck(serviceId);
                if (redisCheck != null)
                    checks.Add(redisCheck);
            }

            var customCheck = CreateCustomCheck(baseUrl, serviceId);
            if (customCheck != null)
                checks.Add(customCheck);

            // TODO Setup health checks for any registered IDbConnectionFactories

            return checks.ToArray();
        }
        private int? GetPort(string baseUrl)
        {
            var uri = new Uri(baseUrl, UriKind.Absolute);
            return uri.Port;
        }

        private decimal GetVersion()
        {
            // defaults to get the servicestack version number, throws if not numeric
            return decimal.Parse($"{HostContext.Config.ApiVersion}");
        }

        private ServiceHealthCheck CreateCustomCheck(string baseUrl, string serviceId)
        {
            var settings = HostContext.GetPlugin<ConsulFeature>().Settings;
            var customHealthCheck = settings.GetHealthCheck();
            if (customHealthCheck == null)
            {
                return null;
            }

            return new ServiceHealthCheck
            {
                Id = "SS-HealthCheck",
                ServiceId = serviceId,
                IntervalInSeconds = customHealthCheck.IntervalInSeconds,
                DeregisterCriticalServiceAfterInMinutes = customHealthCheck.DeregisterIfCriticalAfterInMinutes,
                Http = baseUrl.CombineWith("/json/reply/healthcheck"),
                Notes = "This check is an HTTP GET request which expects the service to return 200 OK"
            };
        }

        private ServiceHealthCheck CreateRedisCheck(string serviceId)
        {
            var clientsManager = HostContext.TryResolve<IRedisClientsManager>();
            if (clientsManager == null)
            {
                return null;
            }

            try
            {
                using (var redisClient = clientsManager.GetReadOnlyClient())
                {
                    if (redisClient != null)
                    {
                        var redisHealthCheck = new ServiceHealthCheck
                        {
                            Id = "SS-Redis",
                            ServiceId = serviceId,
                            IntervalInSeconds = 10,
                            Tcp = $"{redisClient.Host}:{redisClient.Port}",
                            Notes = "This check ensures that redis is responding correctly"
                        };
                        return redisHealthCheck;
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof(ConsulClient))
                    .Error(
                        "Could not create a redis connection from the registered IRedisClientsManager, skipping consul health check",
                        ex);
            }

            return null;
        }

        private static ServiceHealthCheck CreateHeartbeatCheck(string baseUrl, string serviceId)
        {
            return new ServiceHealthCheck
            {
                Id = "SS-Heartbeat",
                ServiceId = serviceId,
                IntervalInSeconds = 30,
                Http = baseUrl.CombineWith("/json/reply/heartbeat"),
                Notes = "A heartbeat service to check if the service is reachable, expects 200 response",
                DeregisterCriticalServiceAfterInMinutes = 90
            };
        }
    }
}