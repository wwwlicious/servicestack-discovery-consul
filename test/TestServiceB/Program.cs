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

    class Program
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
            // run from a handler path
            SetConfig(new HostConfig { WebHostUrl = serviceUrl, HandlerFactoryPath = "/api/" });

            Plugins.Add(new ConsulFeature());
            Plugins.Add(new MetadataFeature());
        }
    }

    public class EchoService : Service
    {
        public EchoBReply Any(EchoB echo)
        {
            if (!echo.CallRemoteService)
            {
                return new EchoBReply { Message = "Hello from service B" };
            }

            var remoteResponse = new JsonServiceClient().TryGetClientFor<EchoA>()?.Send(new EchoA());
            return new EchoBReply { Message = remoteResponse?.Message };
        }
    }

    [Route("/echo/b", "POST")] // test reverse lookup route
    public class EchoB : IReturn<EchoBReply>
    {
        public bool CallRemoteService { get; set; }
    }

    public class EchoBReply
    {
        public string Message { get; set; }
    }
}
