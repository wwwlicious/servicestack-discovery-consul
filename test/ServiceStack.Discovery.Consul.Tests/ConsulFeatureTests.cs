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
            Action action = () => new ConsulFeature().Register(new BasicAppHost());
            action.ShouldThrow<ApplicationException>().Which.Message.Should().Be("appHost.Config.WebHostUrl must be set to use the Consul plugin so that the service can sent it's full http://url:port to Consul");
        }

        [Fact]
        public void ServiceChecks_Should_Be_Empty()
        {
            var host = new BasicAppHost { Config = new HostConfig { WebHostUrl = "http://localhost" } };

            var plugin = new ConsulFeature();

            plugin.ServiceChecks.Should().BeEmpty();
        }

        [Fact]
        public void FactMethodName()
        {
            var serviceClient = new JsonServiceClient();
            var response = serviceClient.Send(new ExternalDTO(), true);
        }

        [Route("/external/Dto")]
        public class ExternalDTO : IReturn<ExternalDTO>
        {
        }
    }

    public static class ServiceClientConsulExtensions
    {
        public static TResponse Send<TResponse>(this ServiceClientBase client, IReturn<TResponse> request, bool test)
        {
            var type = request.GetType();
            return client.TryGetClientFor(type).Send<TResponse>((object)request);
        }

        public static void Send(this ServiceClientBase client, IReturnVoid request)
        {
            client.SendOneWay((object)request);
        }
    }
}