// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace ServiceStack.Discovery.Consul
{
    using System;
    using System.Collections.Generic;

    using ServiceStack;

    public class ConsulFeature : IPlugin
    {
        public List<string> CustomTags { get; } = new List<string>();

        public List<ConsulRegisterCheck> ServiceChecks { get; } = new List<ConsulRegisterCheck>();

        public bool IncludeDefaultServiceHealth { get; set; } = true;
        
        public void Register(IAppHost appHost)
        {
            appHost.AfterInitCallbacks.Add(RegisterService);
            appHost.OnDisposeCallbacks.Add(UnRegisterService);
        }

        private void RegisterService(IAppHost host)
        {
            // HACK, not great but unsure how to improve
            // throws exception if WebHostUrl isn't set as this is how we get endpoint url:port
            if (host.Config?.WebHostUrl == null)
                throw new ApplicationException("appHost.Config.WebHostUrl must be set to use the Consul plugin so that the service can sent it's full http://url:port to Consul");

            ConsulClient.RegisterService(host, ServiceChecks, CustomTags, IncludeDefaultServiceHealth);
        }

        private void UnRegisterService(IAppHost host = null)
        {
            ConsulClient.DeregisterService();
        }
    }
}