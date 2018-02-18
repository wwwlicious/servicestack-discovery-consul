// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace TestServiceA
{
    using System;
    using System.Diagnostics;
    using Funq;
    using ServiceStack;
    using ServiceStack.Discovery.Consul;
    using ServiceStack.Redis;
    using ServiceStack.Text;

    class Program
    {
        static void Main(string[] args)
        {
            var serviceUrl = "http://127.0.0.1:8091/";
            new AppHost(serviceUrl).Init().Start("http://*:8091/");
            $"ServiceStack SelfHost listening at {serviceUrl} ".Print();
            Process.Start(serviceUrl);

            Console.ReadLine();
        }
    }

    public class AppHost : AppSelfHostBase
    {
        private readonly string serviceUrl;

        public AppHost(string serviceUrl) : base("ServiceA", typeof(EchoService).Assembly)
        {
            this.serviceUrl = serviceUrl;
        }

        public override void Configure(Container container)
        {
            SetConfig(new HostConfig
            {
                WebHostUrl = serviceUrl,
                ApiVersion = "2.0"
            });

            Plugins.Add(new ConsulFeature(settings =>
            {
                settings.AddServiceCheck(host =>
                    {
                        // custom logic for checking service health
                        // return new HealthCheck(ServiceHealth.Critical, "Out of disk space");
                        // return new HealthCheck(ServiceHealth.Warning, "Query times are slower than expected");
                        return new HealthCheck(ServiceHealth.Ok, "working normally");
                    },
                    intervalInSeconds: 60);
                settings.AddTags("one", "two", "three");
                settings.SetDefaultGateway(url => new CsvServiceClient(url));
            }));
        }
    }

    public class EchoService : Service
    {
        public object Any(EchoA echo)
        {
            if (!echo.CallRemoteService)
            {
                // local call
                return new EchoAReply { Message = "Hello from service A" };
            }

            // call remote service
            var remoteResponse = Gateway.Send(new EchoB());
            return new EchoAReply { Message = remoteResponse?.Message };
        }
    }

    public class EchoA : IReturn<EchoAReply>
    {
        [ApiMember(Description = "when specified, ServiceA will call ServiceB")]
        public bool CallRemoteService { get; set; }
    }

    public class EchoAReply
    {
        public string CurrentService { get; } = "ServiceA";

        public string Message { get; set; }
    }
}
