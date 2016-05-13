// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Discovery.Consul
{
    using ServiceStack.FluentValidation;

    public class ConsulRegisterServiceValidator : AbstractValidator<ConsulServiceRegistration>
    {
        public ConsulRegisterServiceValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Port).GreaterThan(0);
        }
    }
}