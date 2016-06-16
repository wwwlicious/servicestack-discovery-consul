// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace ServiceStack.Discovery.Consul.Tests
{
    using System;
    using FluentAssertions;
    using Xunit;

    [Collection("AppHost")]
    public class ConsulClientTests
    {
        private readonly AppHostFixture fixture;

        public ConsulClientTests(AppHostFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void GetServices_ParsesExpectedJsonCorrectly()
        {
            using (new HttpResultsFilter("[{\"Node\":\"X1-Win10\",\"Address\":\"127.0.0.1\",\"ServiceID\":\"ss-ServiceA-7f96fc1c-ab72-4471-bc90-a39cd5591545\",\"ServiceName\":\"api\",\"ServiceTags\":[\"ss-version-2.0\",\"EchoA\",\"one\",\"two\",\"three\"],\"ServiceAddress\":\"http://127.0.0.1:8091/\",\"ServicePort\":8091,\"ServiceEnableTagOverride\":false,\"CreateIndex\":7,\"ModifyIndex\":7},{\"Node\":\"X1-Win10\",\"Address\":\"127.0.0.1\",\"ServiceID\":\"ss-ServiceB-73dff66c-bc91-43f3-92f5-6ee7677b2756\",\"ServiceName\":\"api\",\"ServiceTags\":[\"ss-version-1.0\",\"EchoB\"],\"ServiceAddress\":\"http://localhost:8092/api/\",\"ServicePort\":8092,\"ServiceEnableTagOverride\":false,\"CreateIndex\":6,\"ModifyIndex\":6}]"))
            {
                var services = ConsulClient.GetServices("ServiceStack");

                services.Should()
                    .HaveCount(2)
                    .And.ContainSingle(x => x.ServiceID == "ss-ServiceA-7f96fc1c-ab72-4471-bc90-a39cd5591545")
                    .And.ContainSingle(x => x.ServiceID == "ss-ServiceB-73dff66c-bc91-43f3-92f5-6ee7677b2756");
            }
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void GetService_ThrowsException_WhenResponseIsEmpty(string response)
        {
            using (new HttpResultsFilter(response))
            {
                Action action = () => ConsulClient.GetService("service", "three");

                action.ShouldThrow<GatewayServiceDiscoveryException>()
                    .WithMessage("No healthy services are currently registered to process the request of type 'three'")
                    .WithInnerException<WebServiceException>()
                    .WithInnerMessage("Expected json but received empty or null reponse from http://127.0.0.1:8500/v1/health/service/service?near=_agent&passing&tag=three");
            }
        }

        [Fact]
        public void GetServices_ThrowsException_IfRequestThrowsException()
        {
            using (new HttpResultsFilter { StringResultFn = (request, s) => { throw new Exception("unexpected"); }})
            {
                Action action = () => ConsulClient.GetServices("servicestack");

                var ex = action.ShouldThrow<GatewayServiceDiscoveryException>();
                ex.WithMessage("Unable to retrieve services from Consul");
                ex.WithInnerException<Exception>().WithInnerMessage("unexpected");
            }
        }

        [Fact]
        public void GetService_WithNoMatchingTag_ThrowException()
        {
            using (new HttpResultsFilter("[]"))
            {
                Action action = () => ConsulClient.GetService("ServiceStack", "three");

                action.ShouldThrow<GatewayServiceDiscoveryException>().WithMessage("No healthy services are currently registered to process the request of type 'three'");
            }
        }

        [Fact]
        public void GetService_ReturnsCorrectly_WhenHealthyService()
        {
            using (new HttpResultsFilter
            {
                StringResultFn = (request, s) =>
                {
                    if (request.RequestUri.AbsoluteUri == "http://127.0.0.1:8500/v1/health/service/api?near=_agent&passing&tag=EchoA")
                    {
                        return @"[{""Node"":""X1-Win10"",""Address"":""127.0.0.1"",""ServiceID"":""ss-ServiceA-7f96fc1c-ab72-4471-bc90-a39cd5591545"",""ServiceName"":""api"",""ServiceTags"":[""ss-version-2.0"",""EchoA"",""one"",""two"",""three""],""ServiceAddress"":""http://127.0.0.1:8091/"",""ServicePort"":8091,""ServiceEnableTagOverride"":false,""CreateIndex"":7,""ModifyIndex"":7},{""Node"":""X1-Win10"",""Address"":""127.0.0.1"",""ServiceID"":""ss-ServiceB-73dff66c-bc91-43f3-92f5-6ee7677b2756"",""ServiceName"":""api"",""ServiceTags"":[""ss-version-1.0"",""EchoB""],""ServiceAddress"":""http://localhost:8092/api/"",""ServicePort"":8092,""ServiceEnableTagOverride"":false,""CreateIndex"":6,""ModifyIndex"":6}]";
                    }
                    return null;
                }
            })
            {
                var response = ConsulClient.GetService("api", "EchoA");

                response.ServiceName.Should().Be("api");
                response.ServiceID.Should().Be("ss-ServiceA-7f96fc1c-ab72-4471-bc90-a39cd5591545");
                response.ServiceAddress.Should().Be("http://127.0.0.1:8091/");
                response.ServicePort.Should().Be(8091);
                response.ServiceTags.Should().BeEquivalentTo("ss-version-2.0", "EchoA", "one", "two", "three");
            }
        }
    }
}