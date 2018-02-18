// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace ServiceStack.Discovery.Consul.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class TestServiceDiscovery : IServiceDiscovery<ConsulService, ServiceRegistration>
    {
        public TestServiceDiscovery(params KeyValuePair<Type, string>[] dtoTypes)
        {
            DtoTypes = dtoTypes.ToDictionary(x => x.Key, y => y.Value);
        }

        public Dictionary<Type, string> DtoTypes { get; }

        public ServiceRegistration Registration { get; }

        public void Register(IAppHost host)
        {
            throw new NotImplementedException();
        }

        public void Unregister(IAppHost host)
        {
            throw new NotImplementedException();
        }

        public ConsulService[] GetServices(string serviceName)
        {
            throw new NotImplementedException();
        }

        public ConsulService GetService(string serviceName, string dtoName)
        {
            throw new NotImplementedException();
        }

        public HashSet<Type> GetRequestTypes(IAppHost host)
        {
            return DtoTypes.Keys.ToHashSet();
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