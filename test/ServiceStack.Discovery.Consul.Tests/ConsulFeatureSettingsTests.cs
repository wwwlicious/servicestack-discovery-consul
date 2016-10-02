// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 

using FluentAssertions;
using Xunit;

namespace ServiceStack.Discovery.Consul.Tests
{
    public class ConsulFeatureSettingsTests
    {
        private readonly ConsulFeatureSettings settings;

        public ConsulFeatureSettingsTests()
        {
            settings = new ConsulFeatureSettings();
        }

        [Fact]
        public void VerifyDefaults()
        {
            settings.IncludeDefaultServiceHealth.Should().BeTrue();
            settings.GetDiscoveryClient().Should().BeNull();
            settings.GetHealthCheck().Should().BeNull();
            settings.GetServiceChecks().Should().BeEmpty();
            settings.GetCustomTags().Should().BeEmpty();
            settings.GetGateway()("test").Should().BeOfType<JsonServiceClient>();
        }

        [Fact]
        public void CanAddCustomTags()
        {
            settings.AddTags("one", "two");

            settings.GetCustomTags().Should().HaveCount(2).And.BeEquivalentTo("one", "two");
        }

        [Fact]
        public void NullOrEmptyTagsAreIgnored()
        {
            settings.AddTags(null, "", "    ", "me");

            settings.GetCustomTags().Should().HaveCount(1).And.BeEquivalentTo("me");
        }

        [Fact]
        public void CanTurnOffDefaultHealthChecks()
        {
            settings.IncludeDefaultServiceHealth = false;

            settings.IncludeDefaultServiceHealth.Should().BeFalse();
        }

        [Fact]
        public void CanOverrideDiscoveryClient()
        {
            var client = new TestDiscovery();
            settings.AddDiscoveryTypeResolver(client);

            settings.GetDiscoveryClient().Should().Be(client);
        }

        [Fact]
        public void CanOverrideDefaultGateway()
        {
            settings.SetDefaultGateway(uri => new JsvServiceClient(uri)
            {
                UserName = "loompa"
            });

            var client = settings.GetGateway()("oompa").Should().BeOfType<JsvServiceClient>().Subject;
            client.BaseUri.Should().Be("oompa");
            client.UserName.Should().Be("loompa");
        }

        [Fact]
        public void CanAddServiceChecks()
        {
            settings.AddServiceCheck(new ConsulRegisterCheck("checkone", "id1") { HTTP = "http://test", IntervalInSeconds = 10 });

            var check = settings.GetServiceChecks().Should().ContainSingle().Subject;
            check.Name.Should().Be("checkone");
            check.ID.Should().Be("id1:checkone");
        }
    }
}