// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Discovery.Consul.Tests
{
    using FluentAssertions;

    using ServiceStack.FluentValidation.TestHelper;

    using Xunit;

    public class ConsulRegisterServiceTests
    {
        private readonly ConsulRegisterServiceValidator validator;

        public ConsulRegisterServiceTests()
        {
            validator = new ConsulRegisterServiceValidator();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void Name_Is_Required(string name)
        {
            validator.ShouldHaveValidationErrorFor(x => x.Name, new ConsulRegisterService(name, "1"));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Port_IfSpecified_ShouldBeGreater_ThanZero(int port)
        {
            validator.ShouldHaveValidationErrorFor(
                x => x.Port,
                new ConsulRegisterService("name", "1") { Port = port });
        }

        [Fact]
        public void RegisterService_Creates_Correct_Url()
        {
            var url = new ConsulRegisterService("name", "1") {AclToken = "1234" }.ToPutUrl();
            url.Should().Be("/v1/agent/service/register?token=1234");
        }

        [Fact]
        public void RegisterService_IsSerialized_Correctly()
        {
            var model = new ConsulRegisterService("name", "1")
                          {
                              Port = 1,
                              AclToken = "1234",
                              Address = "addr",
                              ID = "override",
                              Tags = new[] { "a", "b" }
                          };
            model.ToJson().Should().Be("{\"ID\":\"override\",\"Name\":\"name\",\"Tags\":[\"a\",\"b\"],\"Address\":\"addr\",\"Port\":1}");
        }
    }
}