// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 

using FluentAssertions;
using Xunit;

namespace ServiceStack.Discovery.Consul.Tests
{
    public class ConsulUrisTests
    {
        [Fact]
        public void GetServicesUri_IsCorrect()
        {
            ConsulUris.GetServices.Should().Be("http://127.0.0.1:8500/v1/agent/services");
        }

        [Fact]
        public void DeregisterServiceUri_IsCorrect()
        {
            ConsulUris.DeregisterService("grommet").Should().Be("http://127.0.0.1:8500/v1/agent/service/deregister/grommet");
        }
    }
}