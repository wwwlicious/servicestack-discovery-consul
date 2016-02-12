// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace ServiceStack.Discovery.Consul.Tests
{
    using System;

    using FluentAssertions;

    using ServiceStack.FluentValidation.TestHelper;
    using ServiceStack.Text;

    using Xunit;

    public class ConsulRegisterCheckTests
    {
        private readonly ConsulRegisterCheckValidator validator;

        public ConsulRegisterCheckTests()
        {
            validator = new ConsulRegisterCheckValidator();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Name_Is_Mandatory(string name)
        {
            validator.ShouldHaveValidationErrorFor(x => x.Name, new ConsulRegisterCheck(name));
        }

        [Fact]
        public void Method_Is_Mandatory()
        {
            validator.ShouldHaveValidationErrorFor(x => x.HTTP, new ConsulRegisterCheck("a"));
        }

        [Fact]
        public void Interval_Is_Required_When_Tcp_Is_Specified()
        {
            validator.ShouldHaveValidationErrorFor(x => x.Interval, new ConsulRegisterCheck("a") { TCP = "a" });
        }

        [Fact]
        public void Interval_Is_Required_When_Http_Is_Specified()
        {
            validator.ShouldHaveValidationErrorFor(x => x.Interval, new ConsulRegisterCheck("a") { HTTP = "a" });
        }

        [Fact]
        public void Interval_Is_Required_When_Script_Is_Specified()
        {
            validator.ShouldHaveValidationErrorFor(x => x.Interval, new ConsulRegisterCheck("a") { Script = "a" });
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Interval_Must_Be_Greater_Than_Zero_If_Specified(int seconds)
        {
            validator.ShouldHaveValidationErrorFor(x => x.Interval, new ConsulRegisterCheck("a") { Script = "a", IntervalInSeconds = seconds });
        }

        [Fact]
        public void Shell_Is_Required_If_DockerContainerId_Is_Specified()
        {
            validator.ShouldHaveValidationErrorFor(x => x.Shell, new ConsulRegisterCheck("a") { DockerContainerID = "a" });
        }

        [Theory]
        [InlineData("a", null, "a")]
        [InlineData("a", "", "a")]
        [InlineData("a", "  ", "a")]
        [InlineData("a", "b", "b:a")]
        public void Check_Id_Is_Generated(string name, string serviceName, string expected)
        {
            new ConsulRegisterCheck(name, serviceName).ID.Should().Be(expected);
        }

        [Fact]
        public void TokenIsSerializedAsQuerystring()
        {
            var check = new ConsulRegisterCheck("test") { AclToken = "1234" };

            check.ToPutUrl().Should().Be("/v1/agent/check/register?token=1234");
            check.ToUrl("PUT", null).Should().Be("/v1/agent/check/register?token=1234");
        }

        [Fact]
        public void Check_Is_Serialized_Correctly()
        {
            var check = new ConsulRegisterCheck(
                "test", "ServiceA")
                            {
                                HTTP = "http",
                                IntervalInSeconds = 1,
                                Notes = "Custom notes",
                                DockerContainerID = "1",
                                AclToken = "1234",
                                ID = "override",
                                Script = "script",
                                Shell = "shell",
                                TCP = "tcp"
                            };
            
            check.ToJson().Should().Be("{\"ID\":\"override\",\"Name\":\"test\",\"ServiceID\":\"ServiceA\",\"Notes\":\"Custom notes\",\"Script\":\"script\",\"DockerContainerID\":\"1\",\"Shell\":\"shell\",\"HTTP\":\"http\",\"TCP\":\"tcp\",\"Interval\":\"1s\"}");
        }
    }
}