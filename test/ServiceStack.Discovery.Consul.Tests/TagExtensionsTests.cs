// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace ServiceStack.Discovery.Consul.Tests
{
    using ServiceStack.Host;
    using Xunit;

    public class TagExtensionsTests
    {
        [Fact]
        public void FactMethodName()
        {
            var testDto = typeof(TestDto);
            
            //testDto.CreateFromOperation(operation);
        }

        public class TestDto { }
    }
}