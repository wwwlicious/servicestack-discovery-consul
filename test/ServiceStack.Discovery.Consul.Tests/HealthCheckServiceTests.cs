// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Net;

namespace ServiceStack.Discovery.Consul.Tests
{
    using FluentAssertions;

    using ServiceStack.Testing;

    using Xunit;

    [Collection("AppHost")]
    public class HealthCheckServiceTests
    {
        private readonly AppHostFixture fixture;
        private readonly ConsulFeature consulFeature;
        private readonly HealthCheckService service;

        public HealthCheckServiceTests(AppHostFixture fixture)
        {
            this.fixture = fixture;
            this.fixture.Host.Container.RegisterAutoWired<HealthCheckService>();
            consulFeature = fixture.Host.GetPlugin<ConsulFeature>();


            var mockHttpRequest = new MockHttpRequest("Heartbeat", "GET", "json", "heartbeat", null, null, null);

            service = new HealthCheckService { Request = mockHttpRequest };
        }

        [Fact]
        public void Heartbeat_Url_Is_Empty()
        {
            new Heartbeat().Url.Should().BeNullOrWhiteSpace();
        }

        [Fact]
        public void Heartbeat_Should_Return_Url()
        {
            var req = new Heartbeat();

            var res = service.Get(req);

            res.Should().BeOfType<Heartbeat>().Which.Url.Should().Be("http://localhost");
        }

        [Fact]
        public void HealthCheck_Should_Return_200()
        {
            var req = new HealthCheck();

            var res = service.Get(req);

            var check = res.Should().BeOfType<HealthCheck>().Subject;
            check.HealthResult.Should().Be(ServiceHealth.Ok);
            check.Message.Should().Be("Not implemented");
        }

        [Fact]
        public void HealthCheck_Should_Return_200_With_Message()
        {
            consulFeature.Settings.AddServiceCheck(host => new HealthCheck(ServiceHealth.Ok, "My message"));
            var req = new HealthCheck();

            var res = service.Get(req);

            var check = res.Should().BeOfType<HealthCheck>().Subject;
            check.HealthResult.Should().Be(ServiceHealth.Ok);
            check.Message.Should().Be("My message");
        }

        [Fact]
        public void Warning_HealthCheck_Should_Return_429_With_Message()
        {
            consulFeature.Settings.AddServiceCheck(host => new HealthCheck(ServiceHealth.Warning, "My warning"));
            var req = new HealthCheck();

            var res = service.Get(req);

            var check = res.Should().BeOfType<HttpError>().Subject;
            check.StatusCode.Should().Be((HttpStatusCode)429);
            check.Message.Should().Be("My warning");
        }

        [Fact]
        public void Critical_HealthCheck_Should_Return_503_With_Message()
        {
            consulFeature.Settings.AddServiceCheck(host => new HealthCheck(ServiceHealth.Critical, "My fatal"));
            var req = new HealthCheck();

            var res = service.Get(req);

            var check = res.Should().BeOfType<HttpError>().Subject;
            check.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
            check.Message.Should().Be("My fatal");
        }

        [Fact]
        public void Exception_HealthCheck_Should_Return_503_With_Message()
        {
            consulFeature.Settings.AddServiceCheck(host => { throw new ApplicationException("oh dear"); });
            var req = new HealthCheck();

            var res = service.Get(req);

            var check = res.Should().BeOfType<HttpError>().Subject;
            check.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
            check.Message.Should().StartWith(@"Health check threw unexpected exception System.ApplicationException: oh dear");
        }
    }
}
