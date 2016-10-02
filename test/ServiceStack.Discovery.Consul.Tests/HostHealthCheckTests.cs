// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 

using System;
using FluentAssertions;
using Xunit;

namespace ServiceStack.Discovery.Consul.Tests
{
    [Collection("AppHost")]
    public class HostHealthCheckTests
    {
        private readonly AppHostFixture fixture;

        public HostHealthCheckTests(AppHostFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void CanAssignDelegate()
        {
            var healthCheck = new HealthCheck(ServiceHealth.Ok, "testing123");
            var check = new HostHealthCheck(host => healthCheck);

            var result = check.HealthCheckDelegate(fixture.Host);

            result.Should().Be(healthCheck);
        }

        [Fact]
        public void HostHealthCheck_ThrowsExceptionIfNull()
        {
            Action action = () => new HostHealthCheck(null);

            action.ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("healthCheckDelegate");
        }

        [Fact]
        public void CanAssignInterval()
        {
            var healthCheck = new HealthCheck(ServiceHealth.Ok, "ok");
            var check = new HostHealthCheck(host => healthCheck, 23);

            check.IntervalInSeconds.Should().Be(23);
        }

        [Fact]
        public void CanAssignDeregister()
        {
            var healthCheck = new HealthCheck(ServiceHealth.Ok, "ok");
            var check = new HostHealthCheck(host => healthCheck,deregisterIfCriticalAfterInMinutes: 23);

            check.DeregisterIfCriticalAfterInMinutes.Should().Be(23);
        }
    }
}