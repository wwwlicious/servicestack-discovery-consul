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

            Plugins.Add(new ConsulFeature { IncludeDefaultServiceHealth = false });
            Plugins.Add(new MetadataFeature());

            // set up localhost redis to enable health check
            container.Register<IRedisClientsManager>(c => new RedisManagerPool("localhost:6379"));
        }
    }
     
    public class EchoService : Service
    {
        public EchoAReply Any(EchoA echo)
        {
            if (!echo.CallRemoteService)
            {
                return new EchoAReply { Message = "Hello from service A" };
            }

            var remoteResponse = new JsonServiceClient().TryGetClientFor<EchoB>()?.Send(new EchoB());
            return new EchoAReply { Message = remoteResponse?.Message };
        }
    }

    public class EchoA : IReturn<EchoAReply>
    {
        public bool CallRemoteService { get; set; }
    }

    public class EchoAReply
    {
        public string Message { get; set; }
    }
}
