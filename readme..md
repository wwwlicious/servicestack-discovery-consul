# ServiceStack.Discovery.Consul

A plugin for ServiceStack that registers and deregisters Services with [Consul.io](http://consul.io)

##Requirements

A consul agent must be running on the same machine as the AppHost.

##Getting started

1. Get consul.exe
2. Run consul.exe agent -dev
3. Add the plugin to your app host

```csharp
public override void Configure(Container container)
{
    SetConfig(new HostConfig
    {
      WebHostUrl = Program.ServiceUrl
    });

    Plugins.Add(new ConsulFeature(this));
}
```

## Running your services

Before you start your services, make sure you have an active cluster running on the host machine.

#### Consul Cluster

If you are new to Consul, you can bootstrap your test environment using this command to create an in-memory server agent:

```bat
consul agent -dev -advertise=127.0.0.1
```

This will give you a single server Consul cluster, this is not recommended for production usage, but it will allow you to use service discovery on your dev machine.

You should now be able to view the Consul UI @ [http://127.0.0.1:8500/ui](http://127.0.0.1:8500/UI)


## Service Registration
When an AppHost starts, the plugin will register the service with Consul agent. 

When the AppHost is shutdown, it will deregister the service.

### Health checks
The default health checks will create an endpoint in your service [http://locahost:1234/heartbeat](http://locahost:1234/heartbeat)

If redis has been configured in the apphost, it will also check it's availability.

## Extending health checks

You can add your own health checks

```csharp
using ConsulFeature(this, new AgentServiceCheck());
```  
or
to turn off these defaults, set the ConsulFeature(this) { IncludeDefaultServiceHealth = false }

## Discovery
The aim to enable discovery of a service by it's RequestDTO's

  