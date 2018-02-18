// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace TestServiceB
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceStack;
    using ServiceStack.Discovery.Consul;
    using Container = Funq.Container;

    public class Program
    {
        public static void Main(string[] args)
        {
            var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ??
                       "http://localhost:5000/";

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseUrls(urls)
                .UseSetting(WebHostDefaults.ServerUrlsKey, urls)
                .Build();

            host.Run();
        }
    }

    public class Startup
    {
        private readonly IConfiguration config;

        public Startup(IConfiguration config)
        {
            this.config = config;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var baseUrl = config[WebHostDefaults.ServerUrlsKey];
            app.UseServiceStack(new AppHost(baseUrl));

            app.Run(context =>
            {
                context.Response.Redirect("/metadata");
                return Task.FromResult(0);
            });
        }
    }

    public class AppHost : AppHostBase
    {
        private readonly string serviceUrl;

        public AppHost(string serviceUrl) : base("ServiceB", typeof(EchoService).Assembly)
        {
            this.serviceUrl = serviceUrl;
        }

        public override void Configure(Container container)
        {
            // supports handler subpaths 
            SetConfig(new HostConfig {WebHostUrl = serviceUrl}); //, HandlerFactoryPath = "/api/" });
            Plugins.Add(new MetadataFeature());
            Plugins.Add(new ConsulFeature());
        }
    }

    public class EchoService : Service
    {
        public EchoBReply Any(EchoB echo)
        {
            if ( !echo.CallRemoteService )
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