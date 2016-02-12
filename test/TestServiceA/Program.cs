using System;

namespace TestServiceA
{
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

            Plugins.Add(new ConsulFeature());
            Plugins.Add(new MetadataFeature());

            // set up localhost redis to enable health check
            container.Register<IRedisClientsManager>(c => new RedisManagerPool("localhost:6379"));
        }
    }
     
    public class EchoService : Service
    {
        public EchoAReply Any(EchoA echo)
        {
            //if (echo.CallRemoteService)
            //{
            //    return new JsonServiceClient().TryGetClientFor<EchoB>();
            //}
            return new EchoAReply { Message = "Hello from service A" };
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
