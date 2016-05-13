// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace ServiceStack.Discovery.Consul.Tests
{
    using System;
    using System.Collections.Generic;

    using FluentAssertions;

    using Xunit;

    [Collection("AppHost")]
    public class ConsulServiceGatewayFactoryTests
    {
        [Fact]
        public void Ctor_Requires_DefaultGatewayDelegate()
        {
            Action action = () => new ConsulServiceGatewayFactory(null, new TestDiscovery());
            action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("defaultGateway");
        }

        [Fact]
        public void Ctor_Requires_DefaultDiscovery()
        {
            Action action = () => new ConsulServiceGatewayFactory(uri => new JsonServiceClient(uri), null);
            action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("discoveryClient");
        }

        [Fact]
        public void Gateway_Ignores_LocalTypes()
        {
            var gateway = new ConsulServiceGatewayFactory(uri => new CsvServiceClient(uri), new TestDiscovery());
            gateway.LocalTypes.Add(typeof(ConsulServiceGatewayFactoryTests));

            var serviceGateway = gateway.GetGateway(typeof(ConsulServiceGatewayFactoryTests));

            serviceGateway.Should().BeNull();
        }

        [Fact]
        public void Gateway_ReturnsCorrectly_ForNonLocalTypes()
        {
            var resolver = new TestDiscovery(new KeyValuePair<Type, string>(typeof(ConsulServiceGatewayFactoryTests), "http://banana"));
            var gateway = new ConsulServiceGatewayFactory(uri => new CsvServiceClient(uri) { Version = 123 }, resolver);
            gateway.LocalTypes.Clear();

            var serviceGateway = gateway.GetGateway(typeof(ConsulServiceGatewayFactoryTests));

            var client = serviceGateway.Should().BeOfType<CachedServiceClient>().Subject;
            client.Version.Should().Be(123);
        }
    }
}