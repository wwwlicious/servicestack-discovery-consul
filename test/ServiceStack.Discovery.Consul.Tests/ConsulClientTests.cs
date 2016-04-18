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
            using (new HttpResultsFilter(@"{""SS-S1"":[""req-one"", ""ServiceStack""],""SS-S2"":[""req-two"", ""ServiceStack""],}"))
            {
                var services = ConsulClient.GetServices();

                services.Should()
                    .HaveCount(2)
                    .And.ContainSingle(x => x.ID == "SS-S1")
                    .And.ContainSingle(x => x.ID == "SS-S2");
            }
        }

        [Fact]
        public void CanFilterServicesByTag()
        {
            using (new HttpResultsFilter(@"{""SS-S1"":[""req-one"", ""ServiceStack""],""SS-S2"":[""req-two"", ""ServiceStack""],}"))
            {
                var services = ConsulClient.FindService("two");
                services.Should().HaveCount(1).And.ContainSingle(x => x.ID == "SS-S2");
            }
        }

        [Fact]
        public void FindService_ReturnsEmpty_WhenNoIsTagMatch()
        {
            using (new HttpResultsFilter(@"{""SS-S1"":[""req-one"", ""ServiceStack""],""SS-S2"":[""req-two"", ""ServiceStack""],}"))
            {
                var services = ConsulClient.FindService("three");

                services.Should().BeEmpty();
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void FindService_ThrowsException_WhenResponseIsEmpty(string response)
        {
            using (new HttpResultsFilter(response))
            {
                Action action = () => ConsulClient.FindService("three");

                var ex = action.ShouldThrow<GatewayServiceDiscoveryException>();
                ex.WithMessage("Unable to retrieve services from Consul");
                ex.WithInnerException<WebServiceException>()
                    .WithInnerMessage("Expected json but received empty or null reponse from http://127.0.0.1:8500/v1/catalog/services?near=_agent");
            }
        }

        [Fact]
        public void GetServices_ThrowsException_IfRequestThrowsException()
        {
            using (new HttpResultsFilter { StringResultFn = (request, s) => { throw new Exception("unexpected"); }})
            {
                Action action = () => ConsulClient.GetServices();

                var ex = action.ShouldThrow<GatewayServiceDiscoveryException>();
                ex.WithMessage("Unable to retrieve services from Consul");
                ex.WithInnerException<Exception>().WithInnerMessage("unexpected");
            }
        }

        [Fact]
        public void GetService_WithNoMatchingTag_ThrowException()
        {
            using (new HttpResultsFilter(@"{""SS-S1"":[""req-one"", ""ServiceStack""],""SS-S2"":[""req-two"", ""ServiceStack""],}"))
            {
                Action action = () => ConsulClient.GetService("three");

                action.ShouldThrow<GatewayServiceDiscoveryException>().WithMessage("No services are currently registered to process the request of type 'three'");
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void GetService_ThrowsException_WhenServiceRequestResponseIsEmpty(string response)
        {
            using (new HttpResultsFilter
            {
                StringResultFn = (request, s) =>
                {
                    if (request.RequestUri.AbsoluteUri == "http://127.0.0.1:8500/v1/catalog/services?near=_agent")
                    {
                        return @"{""SS-S1"":[""req-one"", ""ServiceStack""],""SS-S2"":[""req-two"", ""ServiceStack""],}";
                    }
                    return response;
                }
            })
            {
                Action action = () => ConsulClient.GetService("one");
                action.ShouldThrow<GatewayServiceDiscoveryException>()
                    .WithMessage("No healthy services are currently registered to process the request of type 'one'")
                    .WithInnerException<WebServiceException>()
                    .WithInnerMessage("Expected json but received empty or null reponse from http://127.0.0.1:8500/v1/health/service/SS-S1?near=_agent&passing&tag=req-one");
            }
        }

        [Fact]
        public void GetService_ReturnsCorrectly_WhenHealthyService()
        {
            using (new HttpResultsFilter
            {
                StringResultFn = (request, s) =>
                {
                    if (request.RequestUri.AbsoluteUri == "http://127.0.0.1:8500/v1/catalog/services?near=_agent")
                    {
                        return @"{""SS-S1"":[""req-EchoA"", ""ServiceStack""],""SS-S2"":[""req-two"", ""ServiceStack""],}";
                    }
                    if (request.RequestUri.AbsoluteUri == "http://127.0.0.1:8500/v1/health/service/SS-S1?near=_agent&passing&tag=req-EchoA")
                    {
                        return
                            @"[{""Node"":{""Node"":""X1-Win10"",""Address"":""127.0.0.1"",""TaggedAddresses"":{""wan"":""127.0.0.1""},""CreateIndex"":3,""ModifyIndex"":1612},""Service"":{""ID"":""SS-ServiceAv2-0"",""Service"":""SS-ServiceA"",""Tags"":[""v2-0"",""ServiceStack"",""req-EchoA"",""one"",""two"",""three""],""Address"":""http://127.0.0.1:8091/"",""Port"":0,""EnableTagOverride"":false,""CreateIndex"":1607,""ModifyIndex"":1612},""Checks"":[{""Node"":""X1-Win10"",""CheckID"":""serfHealth"",""Name"":""Serf Health Status"",""Status"":""passing"",""Notes"":"""",""Output"":""Agent alive and reachable"",""ServiceID"":"""",""ServiceName"":"""",""CreateIndex"":3,""ModifyIndex"":3},{""Node"":""X1-Win10"",""CheckID"":""SS-ServiceAv2-0:SS-HealthCheck"",""Name"":""SS-HealthCheck"",""Status"":""passing"",""Notes"":""This check is an HTTP GET request which expects the service to return 200 OK"",""Output"":"""",""ServiceID"":""SS-ServiceAv2-0"",""ServiceName"":""SS-ServiceA"",""CreateIndex"":1609,""ModifyIndex"":1612},{""Node"":""X1-Win10"",""CheckID"":""SS-ServiceAv2-0:SS-HeartBeat"",""Name"":""SS-HeartBeat"",""Status"":""passing"",""Notes"":""A heartbeat service to check if the service is reachable, expects 200 response"",""Output"":"""",""ServiceID"":""SS-ServiceAv2-0"",""ServiceName"":""SS-ServiceA"",""CreateIndex"":1608,""ModifyIndex"":1611}]}]";
                    }
                    return null;
                }
            })
            {
                var response = ConsulClient.GetService("EchoA");

                response.Service.Should().Be("SS-ServiceA");
                response.ID.Should().Be("SS-ServiceAv2-0");
                response.Address.Should().Be("http://127.0.0.1:8091/");
                response.Port.Should().Be(0);
                response.Tags.Should().BeEquivalentTo("v2-0", "ServiceStack", "req-EchoA", "one", "two", "three");
            }
        }
    }
}