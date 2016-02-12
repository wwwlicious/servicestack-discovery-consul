namespace ServiceStack.Discovery.Consul
{
    using System;
    using System.Runtime.Serialization;

    using ServiceStack.FluentValidation;

    [Route("/v1/agent/check/register", "PUT")]
    public class ConsulRegisterCheck : IUrlFilter
    {
        public ConsulRegisterCheck(string name, string serviceId = null)
        {
            Name = name;
            ServiceID = serviceId;

            ID = string.IsNullOrWhiteSpace(serviceId) ? Name : $"{ServiceID}:{Name}";
        }

        /// <summary>
        /// Defaults to the ServiceName:Name
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// The name of the agent check
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The service name to associate the check with, leave blank for agent check
        /// </summary>
        public string ServiceID { get; }

        /// <summary>
        /// User notes on the check
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Check to execute a script on the host
        /// </summary>
        public string Script { get; set; }

        public string DockerContainerID { get; set; }

        public string Shell { get; set; }

        public string HTTP { get; set; }

        public string TCP { get; set; }

        [IgnoreDataMember]
        public double? IntervalInSeconds { get; set; }

        public string Interval => IntervalInSeconds?.ToString("0s");

        [IgnoreDataMember]
        public string AclToken { get; set; }

        public string ToUrl(string absoluteUrl)
        {
            return string.IsNullOrWhiteSpace(AclToken) ? absoluteUrl : absoluteUrl.AddQueryParam("token", AclToken);
        }
    }

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

            // if script, http or tcp is set, interval is required
            RuleFor(x => x.IntervalInSeconds)
                .NotEmpty()
                .GreaterThan(0)
                .When(
                    x =>
                    !x.HTTP.IsNullOrEmpty() || !x.TCP.IsNullOrEmpty() || !x.Script.IsNullOrEmpty());

            // If docker container id is provided, script is evaluated using the specified shell
            RuleFor(x => x.Shell).NotEmpty().When(x => !x.DockerContainerID.IsNullOrEmpty());

        }
    }
}