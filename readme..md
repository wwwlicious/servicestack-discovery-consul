# ServiceStack.Discovery.Consul

A plugin for ServiceStack that registers and deregisters Services with [Consul.io](http://consul.io) and provides RequestDTO service discovery

## Requirements

A consul agent must be running on the same machine as the AppHost.

## Quick Start

Add the following to your AppHost, WebHostUrl and the Plugin

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
Pass the resolver the DTO from an external service using the plugin.
and an empty client, the client will find the correct service url or return null
if no clients are 

```csharp
var client = new JsonServiceClient().TryGetClientFor<ExternalDTO>();
var response = client.Send(new ExternalDTO { Custom = "bob" });
```

## Running your services

Before you start your services, you'll need to [download consul](https://www.consul.io/) and start the agent running on your machine.


The following will create an in-memory instance which is useful for testing

```shell

consul.exe agent -dev -advertise=127.0.0.1

```

You should now be able to view the [Consul UI](http://127.0.0.1:8500/ui)

Once you have added the plugin to your ServiceStack AppHost, you should see it appear
in the Consol UI when you start it.

### Service Registration

The plugin will register the service with a Consul agent. 
When the AppHost is shutdown, it will deregister the service.

#### Health checks

Each service can have a number of health checks.  

By default the plugin creates 2 health checks

1. Heartbeat : Creates an endpoint in your service [http://locahost:1234/reply/json/heartbeat](http://locahost:1234/reply/json/heartbeat) that expects a 200 response
2. If Redis has been configured in the AppHost, it will check Redis is responding

To turn off the default checks use the following:+1:
```csharp
Plugins.Add(new ConsulFeature(this) { IncludeDefaultServiceHealth = false });
```

#### Custom health checks

You can add your own health checks

```csharp
using ConsulFeature(this, new AgentServiceCheck());
```  
or
to turn off these defaults, set the ConsulFeature(this) { IncludeDefaultServiceHealth = false }

### Discovery

The default discovery mechanism uses the RequestDTO type names to resolve the services capable of processing the request.
This means that you should always use unique names across all your services for each of your RequestDTO's

To resolve the correct service use the following client extension

```csharp
var client = new JsonServiceClient().TryGetClientFor<ExternalDTO>();
var result = client.Send(new ExternalDTO());
```


 


  