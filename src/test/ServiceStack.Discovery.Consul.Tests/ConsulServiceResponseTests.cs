// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace ServiceStack.Discovery.Consul.Tests
{
    using FluentAssertions;
    using Xunit;

    public class ConsulServiceResponseTests
    {
        [Fact]
        public void Create_ReturnsNull_IfNullPassed()
        {
            ConsulServiceResponse.Create(null).Should().BeNull();
        }

        [Fact]
        public void Create_ReturnsNull_IfNodeNull()
        {
            var response = new ConsulHealthResponse { Service = new ConsulHealthService() };
            ConsulServiceResponse.Create(response).Should().BeNull();
        }

        [Fact]
        public void Create_ReturnsNull_IfServiceNull()
        {
            var response = new ConsulHealthResponse { Node = new Node() };
            ConsulServiceResponse.Create(response).Should().BeNull();
        }

        [Fact]
        public void Create_CorrectValues()
        {
            var response = new ConsulHealthResponse
            {
                Node = new Node { NodeName = "name" },
                Service =
                    new ConsulHealthService
                    {
                        Address = "127.0.0.2",
                        ID = "foo",
                        Port = 8081,
                        Service = "bar",
                        Tags = new[] { "tag1" }
                    }
            };

            var serviceResponse = ConsulServiceResponse.Create(response);
            serviceResponse.Node.Should().Be("name");
            serviceResponse.ServiceID.Should().Be("foo");
            serviceResponse.ServiceAddress.Should().Be("127.0.0.2");
            serviceResponse.ServiceName.Should().Be("bar");
            serviceResponse.ServicePort.Should().Be(8081);
            serviceResponse.ServiceTags.Should().Contain("tag1");
        }
    }
}
