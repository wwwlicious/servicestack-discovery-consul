// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Discovery.Consul.Tests
{
    using System;

    using FluentAssertions;

    using ServiceStack.Testing;

    using Xunit;

    [Collection("AppHost")]
    public class ConsulFeatureTests 
    {
        private readonly AppHostFixture fixture;

        public ConsulFeatureTests(AppHostFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void Config_WebHostUrl_ThrowsException_IfNotSet()
        {
            Action action = () => new ConsulFeature().Register(new BasicAppHost());

            action.ShouldThrow<ApplicationException>().Which.Message.Should().Be("appHost.Config.WebHostUrl must be set to use the Consul plugin, this is so consul will know the full external http://url:port for the service");
        }

        [Fact]
        public void ServiceChecks_Should_Be_Empty()
        {
            var plugin = new ConsulFeature();

            plugin.Settings.GetServiceChecks().Should().BeEmpty();
        }

        [Fact]
        public void DiscoveryClient_Should_BeNull()
        {
            var plugin = new ConsulFeature();

            plugin.Settings.GetDiscoveryClient().Should().BeNull();
        }
    }
}