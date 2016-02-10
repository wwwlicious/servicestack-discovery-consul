// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace ServiceStack.Discovery.Consul
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    using global::Consul;

    using ServiceStack;
    using ServiceStack.DataAnnotations;
    using ServiceStack.Logging;
    using ServiceStack.Redis;

    public class ConsulFeature : IPlugin
    {
        private AgentServiceRegistration registration;

        private readonly ConsulClient client;

        private readonly ILog logger;

        private readonly string baseUrl;

        private readonly List<string> tags = new List<string>();

        private readonly List<AgentServiceCheck> serviceChecks;

        public List<Action<AgentServiceCheck>> ServiceChecks { get; set; }

        public bool IncludeDefaultServiceHealth { get; set; } = true;
        
        public ConsulFeature(IAppHost appHost)
        {
            ServiceChecks = new List<Action<AgentServiceCheck>>();
            serviceChecks = new List<AgentServiceCheck>();

            // hack, not great but unsure how to improve, throw exception if WebHostUrl isn't set
            if (appHost.Config?.WebHostUrl == null)
                throw new ApplicationException("appHost.Config.WebHostUrl must be set to use the Consul plugin so that the service can sent it's full http://url:port to Consul");

            baseUrl = appHost.Config.WebHostUrl;

            if (!string.IsNullOrWhiteSpace(appHost.Config?.ApiVersion))
            {
                // for dns related queries, replace any invalid chars (.) in version strings
                tags.Add("version_{0}".Fmt(appHost.Config?.ApiVersion?.Replace('.', '-')));
            }

            client = new ConsulClient();

            logger = appHost.Config?.LogFactory?.GetLogger(typeof(ConsulFeature));

            // Create registration defaults
            if (IncludeDefaultServiceHealth)
                serviceChecks.AddRange(InitDefaultServiceChecks(appHost));
        }

        public void Register(IAppHost appHost)
        {
            appHost.AfterInitCallbacks.Add(RegisterService);
            appHost.OnDisposeCallbacks.Add(UnRegisterService);
        }

        private void RegisterService(IAppHost host)
        {
            var serviceName = ServiceStackHost.Instance.ServiceName;

            // cleanup - only needed as stopping debugging does not call dispose callback
            this.CleanupCritialServices(serviceName);

            // tags are used to lookup the requestDTO and get back the ServiceUri
            GetServiceTags(host);
            
            registration = new AgentServiceRegistration
            {
                ID = serviceName + Guid.NewGuid(),
                Checks = serviceChecks.ToArray(),
                Name = serviceName,
                Address = baseUrl,
                Tags = tags.ToArray()
            };
            var task = client.Agent.ServiceRegister(registration);
            
            // todo, if plugin is registered early, logging is to nulllogger
            if (task.Result.StatusCode != HttpStatusCode.OK)
            {
                logger.Fatal("Consul failed to register service", task.Exception);
            }
            else
            {
                // hack, not sure if this will work, supposed to cope with unclean exits
                AppDomain.CurrentDomain.ProcessExit += (obj, evnt) => { UnRegisterService(); };
                logger.Debug("Consul registered service");
            }
        }

        private void UnRegisterService(IAppHost host = null)
        {
            // unregister service with consul
            var task = client.Agent.ServiceDeregister(registration.ID);
            if (task.Result.StatusCode != HttpStatusCode.OK)
            {
                logger.Error("Consul failed to unregister service", task.Exception);
            }
            else
            {
                logger.Debug("Consul unregistered service");
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
        /// <returns>an array of agentservicecheck objects</returns>
        private AgentServiceCheck[] InitDefaultServiceChecks(IAppHost appHost)
        {
            var checks = new List<AgentServiceCheck>();
            appHost.RegisterService<HeartbeatService>();
            var heartbeatCheck = new AgentServiceCheck
                                     {
                                         Interval = TimeSpan.FromSeconds(20),
                                         Timeout = TimeSpan.FromSeconds(1),
                                         HTTP = baseUrl.CombineWith(new HeartbeatService.Heartbeat().ToGetUrl())
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
                        var redisHealthCheck = new AgentServiceCheck
                        {
                            Interval = TimeSpan.FromSeconds(10),
                            TCP = "{0}.{1}".Fmt(redisClient.Host, redisClient.Port)
                        };
                        checks.Add(redisHealthCheck);
                    }
                }
            }

            return checks.ToArray();
        }

        /// <summary>
        /// A catchall that unregisters services with a matching name on the same node with a critical status#
        /// Helps to clean up after possible crashes
        /// </summary>
        /// <param name="serviceName"></param>
        private void CleanupCritialServices(string serviceName)
        {
            foreach (var healthCheck in client.Health.State(CheckStatus.Critical).Result.Response)
            {
                // relies on only one service of the same name running per node
                if (healthCheck.ServiceName == serviceName && healthCheck.Node == client.Agent.NodeName)
                {
                    client.Agent.ServiceDeregister(healthCheck.ServiceID);
                }
            }
        }

        /// <summary>
        /// Last but not least, tags will be the main mechanism of discovery for any servicestack requestDTO's
        /// Need viable solution in server or client 
        /// </summary>
        /// <param name="host"></param>
        private void GetServiceTags(IAppHost host)
        {
            var ops = host.Metadata.RequestTypes.Where(x => !x.HasAttribute<ExcludeAttribute>()).ToArray();
            tags.AddRange(ops.Select(x => x.Name));
        }
    }
}