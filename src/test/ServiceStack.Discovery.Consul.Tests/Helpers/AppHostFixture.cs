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
            var config = new HostConfig { WebHostUrl = "http://localhost", DebugMode = true };
            TestTypes = new[]
            {
                new KeyValuePair<Type, string>(typeof (TestDto), typeof (TestDto).Name),
                new KeyValuePair<Type, string>(typeof (TestDto2), typeof (TestDto2).Name)
            };
            var plugins = new List<IPlugin>();
            plugins.Add(new NativeTypesFeature());
            plugins.Add(new ConsulFeature(settings =>
            {
                settings.AddServiceDiscovery(new TestServiceDiscovery(TestTypes));
            }));

            Host = new BasicAppHost
            {
                TestMode = true,
                Config = config,
                Plugins = plugins,
            };
            
            Host.Init();
            Host.Config.WebHostUrl = "http://localhost";
        }

        public KeyValuePair<Type, string>[] TestTypes { get; private set; }

        public BasicAppHost Host { get; set; }

        public void Dispose()
        {
            Host.Dispose();
        }
    }

    public class TestDto : IReturn<TestDto> { }
    public class TestDto2 : IReturn<TestDto2> { }
}