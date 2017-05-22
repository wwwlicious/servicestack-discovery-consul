// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Discovery.Consul
{
  using System;
  using Funq;

  using ServiceStack;
  using ServiceStack.Web;
  using Text;

  /// <summary>
  /// Enables remote service calls by dynamically looking up remote service url
  /// </summary>
  public class ConsulFeature : IPlugin
  {
    private IServiceDiscovery<ConsulService, ServiceRegistration> ServiceDiscovery { get; set; }

    public ConsulFeatureSettings Settings { get; }

    /// <summary>
    /// Enables service discovery using consul to resolve the correct url for a remote RequestDTO
    /// </summary>
    public ConsulFeature(ConsulSettings settings = null)
    {
      Settings = new ConsulFeatureSettings();
      settings?.Invoke(Settings);
    }

    public void Register(IAppHost appHost)
    {
      // register callbacks
      appHost.OnDisposeCallbacks.Add(UnRegisterService);
      appHost.AfterInitCallbacks.Add(RegisterService);

      // HACK: not great but unsure how to improve
      // throws exception if WebHostUrl isn't set as this is how we get endpoint url:port
      if (appHost.Config?.WebHostUrl != null)
      {


        appHost.RegisterService<HealthCheckService>();
        appHost.RegisterService<DiscoveryService>();

        // register plugin link
        appHost.GetPlugin<MetadataFeature>()?.AddPluginLink(ConsulUris.LocalAgent.CombineWith("ui"), "Consul Agent WebUI");
      }

    }


    private void RegisterService(IAppHost host)
    {
      ServiceDiscovery = Settings.GetDiscoveryClient() ?? new ConsulDiscovery();
      if (host.Config?.WebHostUrl != null)
      {
        ServiceDiscovery.Register(host);
      }
      // register servicestack discovery services
      host.Register(ServiceDiscovery);
      host.GetContainer()
          .Register<IServiceGatewayFactory>(x => new ConsulServiceGatewayFactory(Settings.GetGateway(), ServiceDiscovery))
          .ReusedWithin(ReuseScope.None);
    }

    private void UnRegisterService(IAppHost host = null)
    {
      ServiceDiscovery.Unregister(host);
    }
  }

  public delegate HealthCheck HealthCheckDelegate(IAppHost appHost);

  public delegate IServiceGateway DefaultGatewayDelegate(ConsulService service);

  public delegate void ConsulSettings(ConsulFeatureSettings settings);
}