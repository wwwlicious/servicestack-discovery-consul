// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace ServiceStack.Discovery.Consul.Tests
{
    using System;

    using FluentAssertions;

    using ServiceStack.Testing;

    using Xunit;

    public class ConsulFeatureTests
    {
        [Fact]
        public void Config_WebHostUrl_ThrowsException_IfNotSet()
        {
            Action action = () => new ConsulFeature(new BasicAppHost());
            action.ShouldThrow<ApplicationException>().Which.Message.Should().Be("appHost.Config.WebHostUrl must be set to use the Consul plugin so that the service can sent it's full http://url:port to Consul");
        }

        [Fact]
        public void ServiceChecks_Should_Be_Empty()
        {
            var host = new BasicAppHost { Config = new HostConfig { WebHostUrl = "http://localhost" } };

            var plugin = new ConsulFeature(host);

            plugin.ServiceChecks.Should().BeEmpty();
        } 
    }
}