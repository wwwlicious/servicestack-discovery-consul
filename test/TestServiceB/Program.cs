// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace TestServiceB
{
    using System;
    using System.Diagnostics;
    using ServiceStack;
    using ServiceStack.Discovery.Consul;
    using ServiceStack.Text;

    using Container = Funq.Container;

    public class Program
    {
        static void Main(string[] args)
        {
            var serviceUrl = "http://localhost:8092/";
            new AppHost(serviceUrl).Init().Start("http://*:8092/");
            $"ServiceStack SelfHost listening at {serviceUrl} ".Print();
            Process.Start(serviceUrl);

            Console.ReadLine();
        }
    }

    public class AppHost : AppSelfHostBase
    {
        private readonly string serviceUrl;

        public AppHost(string serviceUrl) : base("ServiceB", typeof(EchoService).Assembly)
        {
            this.serviceUrl = serviceUrl;
        }

        public override void Configure(Container container)
        {
            // supports handler subpaths 
            SetConfig(new HostConfig { WebHostUrl = serviceUrl, HandlerFactoryPath = "/api/" });
            Plugins.Add(new ConsulFeature());
        }
    }

    public class EchoService : Service
    {
        public EchoBReply Any(EchoB echo)
        {
            if (!echo.CallRemoteService)
            {
                // local call
                return new EchoBReply { Message = "Hello from service B" };
            }

            // call remote service 
            var remoteResponse = Gateway.Send(new EchoA());
            return new EchoBReply { Message = remoteResponse?.Message };
        }
    }

    [Route("/echo/b", "POST")] // test reverse lookup route
    public class EchoB : IReturn<EchoBReply>
    {
        [ApiMember(Description = "when specified, ServiceB will call ServiceA")]
        public bool CallRemoteService { get; set; }
    }

    public class EchoBReply
    {
        public string CurrentService { get; } = "ServiceB";

        public string Message { get; set; }
    }
}
