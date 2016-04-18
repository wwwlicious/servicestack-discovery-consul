// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace ServiceStack.Discovery.Consul.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using ServiceStack.DataAnnotations;
    using Xunit;

    public class TypeExtensionsTests
    {
        private static HashSet<Type> Types => new HashSet<Type> { typeof(Test1), typeof(Test2), typeof(Test3), typeof(Test4) };

        [Fact]
        public void WithServiceDiscoveryAllowed_FiltersExpectedTypes()
        {
            var result = Types.WithServiceDiscoveryAllowed().ToArray();

            result.Should().HaveCount(1).And.OnlyContain(x => x == typeof(Test4));
        }

        [Fact]
        public void NativeTypes_FiltersExpectedTypes()
        {
            var nativeTypes = new NativeTypesFeature();

            var result =
                Types.Concat(nativeTypes.MetadataTypesConfig.ExportTypes)
                    .Concat(new[] {typeof(Test5)})
                    .WithoutNativeTypes(nativeTypes);

            result.Should().BeEquivalentTo(Types);
        }

        [Fact]
        public void WithoutExternalRestrictions_FiltersExpectedTypes()
        {
            var result = Types.WithoutExternalRestrictions();

            result.Should().HaveCount(1).And.OnlyContain(x => x == typeof(Test3));
        }

        [Exclude(Feature.Metadata)]
        [Restrict(RequestAttributes.InternalNetworkAccess)]
        public class Test1 { }

        [Exclude(Feature.ServiceDiscovery)]
        [Restrict(RequestAttributes.LocalSubnet)]
        public class Test2 { }

        [Exclude(Feature.All)]
        [Restrict(RequestAttributes.External)]
        public class Test3 { }

        [Restrict(VisibleInternalOnly = true)]
        public class Test4 { }
    }
}

namespace ServiceStack
{
    public class Test5 { }
}