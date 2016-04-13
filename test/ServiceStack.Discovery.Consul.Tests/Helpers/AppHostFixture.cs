// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Discovery.Consul.Tests
{
    using System;
    using System.Collections.Generic;

    using ServiceStack.Testing;

    public class AppHostFixture : IDisposable
    {
        public AppHostFixture()
        {
            var config = new HostConfig { WebHostUrl = "http://localhost" };
            Host = new BasicAppHost
                       {
                           TestMode = true,
                           Config = config,
                           Plugins =
                               new List<IPlugin>(
                               new[]
                                   {
                                       new ConsulFeature(
                                           url => new CsvServiceClient(url))
                                           {
                                               DiscoveryTypeResolver = new TestDiscovery()
                                           }
                                   })
                       };
            Host.Init();
        }

        public BasicAppHost Host { get; set; }

        public void Dispose()
        {
            Host.Dispose();
        }
    }
}