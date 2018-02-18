// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Discovery.Consul.Tests
{
    using FluentAssertions;

    using ServiceStack.FluentValidation.TestHelper;
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
            validator.ShouldHaveValidationErrorFor(x => x.Name, new ConsulServiceRegistration("id", name));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Port_IfSpecified_ShouldBeGreater_ThanZero(int port)
        {
            validator.ShouldHaveValidationErrorFor(
                x => x.Port,
                new ConsulServiceRegistration("id", "name") { Port = port });
        }

        [Fact]
        public void RegisterService_Creates_Correct_Url()
        {
            var url = new ConsulServiceRegistration("id", "name") { AclToken = "1234" }.ToPutUrl();

            url.Should().Be("/v1/agent/service/register?token=1234");
        }

        [Fact]
        public void RegisterService_IsSerialized_Correctly()
        {
            var model = new ConsulServiceRegistration("override", "name")
                          {
                              Port = 1,
                              AclToken = "1234",
                              Address = "addr",
                              Tags = new[] { "a", "b" }
                          };
            model.ToJson().Should().Be("{\"ID\":\"override\",\"Name\":\"name\",\"Tags\":[\"a\",\"b\"],\"Address\":\"addr\",\"Port\":1}");
        }


        [Fact]
        public void DeSerialize_Consul_Service_Json()
        {
            var result = "{\"Node\":\"X1-Win10\",\"Address\":\"127.0.0.1\",\"ServiceID\":\"ss-ServiceA-7f96fc1c-ab72-4471-bc90-a39cd5591545\",\"ServiceName\":\"api\",\"ServiceTags\":[\"ss-version-2.0\",\"EchoA\",\"one\",\"two\",\"three\"],\"ServiceAddress\":\"http://127.0.0.1:8091/\",\"ServicePort\":8091,\"ServiceEnableTagOverride\":false,\"CreateIndex\":7,\"ModifyIndex\":7}";
            var service = result.FromJson<ConsulServiceResponse>();

            service.ServiceID.Should().Be("ss-ServiceA-7f96fc1c-ab72-4471-bc90-a39cd5591545");
            service.ServiceTags.Should().BeEquivalentTo("ss-version-2.0", "EchoA", "one", "two", "three");
            service.ServiceAddress.Should().Be("http://127.0.0.1:8091/");
            service.ServiceName.Should().Be("api");
            service.ServicePort.Should().Be(8091);
        }
    }
}