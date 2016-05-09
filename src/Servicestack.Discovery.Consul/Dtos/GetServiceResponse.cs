// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace ServiceStack.Discovery.Consul
{
    public class GetServiceResponse
    {
        public GetServiceResponse(ConsulService response)
        {
            Id = response.Id;
            Name = response.Name;
            Address = response.Address;
            Tags = response.Tags;
            Version = response.Version;
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public string[] Tags { get; set; }

        public string Address { get; set; }

        public decimal Version { get; set; }
    }
}