// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 

using Xunit;

namespace ServiceStack.Discovery.Consul.Tests
{
    [Collection("AppHost")]
    public class GatewayServiceDiscoveryExceptionpeResolverTests
    {
        private readonly AppHostFixture fixture;

        public GatewayServiceDiscoveryExceptionpeResolverTests(AppHostFixture fixture)
        {
            this.fixture = fixture;
        }
    }
}