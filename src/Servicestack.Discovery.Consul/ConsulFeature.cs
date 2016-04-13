// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Discovery.Consul
{
    using System;
    using System.Collections.Generic;

    using Funq;

    using ServiceStack;
    using ServiceStack.Web;

    /// <summary>
    /// Enabled service to service calls by dynamically looking up remote service url
    /// </summary>
    public class ConsulFeature : IPlugin
    {
        public delegate IServiceGateway DefaultGatewayDelegate(string baseUri);

        private DefaultGatewayDelegate DefaultGateway { get; }
    
        /// <summary>
        /// Can be used to add tags such as environment to the service registration
        /// </summary>
        public List<string> CustomTags { get; } = new List<string>();

        public List<ConsulRegisterCheck> ServiceChecks { get; } = new List<ConsulRegisterCheck>();

        public IDiscoveryRequestTypeResolver DiscoveryTypeResolver { get; set; } = new DefaultDiscoveryRequestTypeResolver();

        /// <summary>
        /// Enables service discovery using consul to resolve the correct url for a remote RequestDTO
        /// </summary>
        /// <param name="defaultGateway">If specified, will register your preferred client for external calls, defaults to JsonServiceClient</param>
        public ConsulFeature(DefaultGatewayDelegate defaultGateway = null)
        {
            DefaultGateway = defaultGateway ?? (baseUri => new JsonServiceClient(baseUri));
        }

        public bool IncludeDefaultServiceHealth { get; set; } = true;

        private ConsulServiceRegistration Registration { get; set; }

        public void Register(IAppHost appHost)
        {
            // HACK: not great but unsure how to improve
            // throws exception if WebHostUrl isn't set as this is how we get endpoint url:port
            if (appHost.Config?.WebHostUrl == null)
                throw new ApplicationException("appHost.Config.WebHostUrl must be set to use the Consul plugin so that the service can sent it's full http://url:port to Consul");

            appHost.AfterInitCallbacks.Add(RegisterService);
            appHost.OnDisposeCallbacks.Add(UnRegisterService);
            ConsulClient.DiscoveryRequestResolver = DiscoveryTypeResolver;

            var container = appHost.GetContainer();
            container
                .Register<IServiceGatewayFactory>(x => new ConsulServiceGatewayFactory(DefaultGateway, DiscoveryTypeResolver))
                .ReusedWithin(ReuseScope.None);

            // register plugin link
            appHost.GetPlugin<MetadataFeature>()?.AddPluginLink(ConsulUris.LocalAgent.CombineWith("ui"), "Consul Agent WebUI");
        }

        private void RegisterService(IAppHost host)
        {
            Registration = ConsulClient.RegisterService(host, ServiceChecks, CustomTags, IncludeDefaultServiceHealth);
        }

        private void UnRegisterService(IAppHost host = null)
        {
            ConsulClient.DeregisterService(Registration);
        }
    }
}