// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace ServiceStack.Discovery.Consul.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class TestDiscovery : IDiscoveryRequestTypeResolver
    {
        public TestDiscovery(params KeyValuePair<Type, string>[] dtoTypes)
        {
            DtoTypes = dtoTypes.ToDictionary(x => x.Key, y => y.Value);
        }

        public Dictionary<Type, string> DtoTypes { get; }

        public string[] GetRequestTypes(IAppHost host)
        {
            return DtoTypes.Keys.Select(x => x.GetType().Name).ToArray();
        }

        public string ResolveBaseUri(object dto)
        {
            return ResolveBaseUri(dto.GetType());
        }

        public string ResolveBaseUri(Type dtoType)
        {
            return DtoTypes.ContainsKey(dtoType) ? DtoTypes[dtoType] : null;
        }
    }
}