// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Discovery.Consul.Tests
{
    using System.Linq;

    using FluentAssertions;

    using ServiceStack.FluentValidation.TestHelper;
    using ServiceStack.Text;

    using Xunit;

    public class ConsulServiceRegistrationTests
    {
        private readonly ConsulRegisterServiceValidator validator;

        public ConsulServiceRegistrationTests()
        {
            validator = new ConsulRegisterServiceValidator();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void Name_Is_Required(string name)
        {
            validator.ShouldHaveValidationErrorFor(x => x.Name, new ConsulServiceRegistration(name, "1"));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Port_IfSpecified_ShouldBeGreater_ThanZero(int port)
        {
            validator.ShouldHaveValidationErrorFor(
                x => x.Port,
                new ConsulServiceRegistration("name", "1") { Port = port });
        }

        [Fact]
        public void RegisterService_Creates_Correct_Url()
        {
            var url = new ConsulServiceRegistration("name", "1") {AclToken = "1234" }.ToPutUrl();
            url.Should().Be("/v1/agent/service/register?token=1234");
        }

        [Fact]
        public void RegisterService_IsSerialized_Correctly()
        {
            var model = new ConsulServiceRegistration("name", "1")
                          {
                              Port = 1,
                              AclToken = "1234",
                              Address = "addr",
                              ID = "override",
                              Tags = new[] { "a", "b" }
                          };
            model.ToJson().Should().Be("{\"ID\":\"override\",\"Name\":\"name\",\"Tags\":[\"a\",\"b\"],\"Address\":\"addr\",\"Port\":1}");
        }


        [Fact]
        public void DeSerialize_Consul_Service_Json()
        {
            var result = "{\"ServiceAv2 - 0\":{\"ID\":\"ServiceAv2-0\",\"Service\":\"ServiceA\",\"Tags\":[\"v2-0\",\"EchoA\"],\"Address\":\"http://127.0.0.1:8091/\",\"Port\":1,\"EnableTagOverride\":false,\"CreateIndex\":0,\"ModifyIndex\":0}}";
            var jsonObject = JsonObject.Parse(result);
            var services = jsonObject.Values.Select(x => x.FromJson<ConsulServiceResponse>()).ToArray();

            services.Should().HaveCount(1);
            services.First().ID.Should().Be("ServiceAv2-0");
            services.First().Tags.Should().BeEquivalentTo("v2-0", "EchoA");
            services.First().Address.Should().Be("http://127.0.0.1:8091/");
            services.First().Service.Should().Be("ServiceA");
            services.First().Port.Should().Be(1);
        }
    }
}