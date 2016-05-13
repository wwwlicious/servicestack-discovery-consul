// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace ServiceStack.Discovery.Consul.Tests
{
    using FluentAssertions;
    using Xunit;

    public class ConsulUrisTests
    {
        [Fact]
        public void GetServicesUri_IsCorrect()
        {
            ConsulUris.GetServices("servicestack").Should().Be("http://127.0.0.1:8500/v1/catalog/service/servicestack?near=_agent&passing");
        }

        [Fact]
        public void DeregisterServiceUri_IsCorrect()
        {
            ConsulUris.DeregisterService("grommet").Should().Be("http://127.0.0.1:8500/v1/agent/service/deregister/grommet");
        }
    }
}