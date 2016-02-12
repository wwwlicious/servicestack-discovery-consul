// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace ServiceStack.Discovery.Consul
{
    using System.Runtime.Serialization;

    using ServiceStack.FluentValidation;

    [Route("/v1/agent/service/register", "PUT")]
    public class ConsulRegisterService : IUrlFilter
    {
        public ConsulRegisterService(string name, string version)
        {
            ID = name + version;
            Name = name;
        }

        public string ID { get; set; }

        public string Name { get; private set; }

        public string[] Tags { get; set; } 

        public string Address { get; set; }

        public int? Port { get; set; }

        [IgnoreDataMember]
        public string AclToken { get; set; }

        public string ToUrl(string absoluteUrl)
        {
            return string.IsNullOrWhiteSpace(AclToken) ? absoluteUrl : absoluteUrl.AddQueryParam("token", AclToken);
        }
    }

    public class ConsulRegisterServiceValidator : AbstractValidator<ConsulRegisterService>
    {
        public ConsulRegisterServiceValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Port).GreaterThan(0);
        }
    }
}