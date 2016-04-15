// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace ServiceStack.Discovery.Consul.Tests
{
    using FluentAssertions;

    using Xunit;

    [Collection("AppHost")]
    public class ConsulTests
    {
        private readonly AppHostFixture fixture;

        public ConsulTests(AppHostFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void Resolver_ReturnsDefaultRoute_ByDTOTypeName()
        {
            var remoteuri = "http://remoteuri:1234/api/";
            var expected = remoteuri.CombineWith("/json/reply/ConsulTests.TestDTO");

            var discovery = new TestDiscovery();
            discovery.DtoTypes.Add(typeof(TestDTO), remoteuri);
            ConsulClient.DiscoveryRequestResolver = discovery;

            Consul.ResolveTypedUrl(new JsonServiceClient("http://localhost/"), "GET", new TestDTO()).Should().Be(expected);
        }

        [Fact]
        public void Resolver_ReturnsBaseUri_ForUnregisteredType()
        {
            var baseUri = "http://testuri/";
            var expected = baseUri.CombineWith("/json/reply/ConsulTests.TestDTO");
            ConsulClient.DiscoveryRequestResolver = new TestDiscovery();

            Consul.ResolveTypedUrl(new JsonServiceClient(baseUri), null, new TestDTO()).Should().Be(expected);
        }

        [Fact]
        public void Resolver_ReturnsNull_ForUnregisteredType()
        {
            ConsulClient.DiscoveryRequestResolver = new TestDiscovery();

            Consul.ResolveTypedUrl(null, null, new TestDTO()).Should().BeNull();
        }

        [Theory]
        [InlineData("GET", "/get/dto2")]
        [InlineData("POST", "/post/dto2")]
        [InlineData("PUT", "/put/dto2")]
        [InlineData("DELETE", "/delete/dto2")]
        [InlineData("PATCH", "/patch/dto2")]
        public void Resolver_ReturnsRegisteredType_ByName(string method, string expected)
        {
            var discovery = new TestDiscovery();
            var remoteuri = "http://remoteUri:1234/";
            discovery.DtoTypes.Add(typeof(TestDTO2), remoteuri);
            ConsulClient.DiscoveryRequestResolver = discovery;

            Consul.ResolveTypedUrl(new JsonServiceClient("http://localhost"), method, new TestDTO2()).Should().Be(remoteuri.CombineWith(expected));
        }

        private class TestDTO : IReturnVoid { }

        [Route("/post/dto2", "POST")]
        [Route("/get/dto2", "GET")]
        [Route("/put/dto2", "PUT")]
        [Route("/delete/dto2", "DELETE")]
        [Route("/patch/dto2", "PATCH")]
        private class TestDTO2 : IReturn<int> { }
    }
}