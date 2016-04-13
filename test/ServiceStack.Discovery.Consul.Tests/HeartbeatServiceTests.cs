// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Discovery.Consul.Tests
{
    using FluentAssertions;

    using ServiceStack.Testing;

    using Xunit;

    [Collection("AppHost collection")]
    public class HeartbeatServiceTests
    {
        private readonly AppHostFixture fixture;

        private readonly HeartbeatService service;

        public HeartbeatServiceTests(AppHostFixture fixture)
        {
            fixture.Host.Container.RegisterAutoWired<HeartbeatService>();
            
            var mockHttpRequest = new MockHttpRequest("Heartbeat", "GET", "json", "heartbeat", null, null, null);

            service = new HeartbeatService { Request = mockHttpRequest };
        }

        [Fact]
        public void Heartbeat_Status_Is_Empty()
        {
            new Heartbeat().StatusCode.Should().Be(0);
        }

        [Fact]
        public void Heartbeat_Url_Is_Empty()
        {
            new Heartbeat().Url.Should().BeNullOrWhiteSpace();
        }

        [Fact]
        public void Heartbeat_Should_Return_200()
        {
            var req = new Heartbeat();

            var res = service.Any(req);

            res.StatusCode.Should().Be(200);
        }

        [Fact]
        public void Heartbeat_Should_Return_Url()
        {
            var req = new Heartbeat();

            var res = service.Any(req);

            res.Url.Should().Be("http://localhost");
        }
    }
}