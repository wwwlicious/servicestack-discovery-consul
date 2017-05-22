// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Discovery.Consul
{
  using System;
  using ServiceStack.FluentValidation;

  public class ConsulRegisterCheckValidator : AbstractValidator<ConsulRegisterCheck>
  {
    public ConsulRegisterCheckValidator()
    {
      RuleFor(x => x.Name).NotEmpty();

      // at least one out of http, script, tcp must be specified
      RuleFor(x => x.HTTP)
        .NotEmpty()
        .WithName("Method")
        .WithMessage("One of Http, Script or Tcp must be defined")
        .Unless(x => !x.Script.IsNullOrEmpty() || !x.TCP.IsNullOrEmpty());

      RuleFor(x => x.HTTP)
        .Must(x =>
        {
          return x.IsNullOrEmpty() || Uri.IsWellFormedUriString(x, UriKind.Absolute);
        })
        .WithName("InvalidUrl")
        .WithMessage("The http check is not a valid absolute url");

      // if script, http or tcp is set, interval is required
      RuleFor(x => x.IntervalInSeconds)
                .NotEmpty()
                .GreaterThan(0)
                .When(
                    x =>
                    !x.HTTP.IsNullOrEmpty() || !x.TCP.IsNullOrEmpty() || !x.Script.IsNullOrEmpty());

      // if specified, must be positive value
      RuleFor(x => x.DeregisterCriticalServiceAfterInMinutes)
          .GreaterThan(0).When(x => x.DeregisterCriticalServiceAfterInMinutes.HasValue);

      // If docker container id is provided, script is evaluated using the specified shell
      RuleFor(x => x.Shell).NotEmpty().When(x => !x.DockerContainerID.IsNullOrEmpty());

    }
  }
}