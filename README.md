# ServiceStack.Discovery.Consul

A plugin for [ServiceStack](https://servicestack.net/) that registers and deregisters Services with [Consul.io](http://consul.io) and provides service discovery.

[![Build status](https://ci.appveyor.com/api/projects/status/55830emag9ksyasf?svg=true)](https://ci.appveyor.com/project/wwwlicious/servicestack-discovery-consul)

## Requirements

A consul agent must be running on the same machine as the AppHost.

## Quick Start

Install the package [https://www.nuget.org/packages/ServiceStack.Discovery.Consul](https://www.nuget.org/packages/ServiceStack.Discovery.Consul/)
```bash
Install-Package ServiceStack.Discovery.Consul
```

Add the following to your `AppHost`

```csharp
public override void Configure(Container container)
{
    SetConfig(new HostConfig
    {
        // the url:port that other services will use to access this one
        WebHostUrl = "http://api.acme.com:1234" 
        
        // optional 
        ApiVersion = "2.0"
        HandlerFactoryPath = "/api/"
    });

    Plugins.Add(new ConsulFeature(this));
}
```
Use `TryGetClientFor<T>()` with your remote RequestDTO on any ServiceClient and it will
be configured for the correct remote service.

```csharp
var client = new JsonServiceClient().TryGetClientFor<ExternalDTO>();
var response = client.Send(new ExternalDTO { Custom = "bob" });
```

## Running your services

Before you start your services, you'll need to [download consul](https://www.consul.io/) and start the agent running on your machine.


The following will create an in-memory instance which is useful for testing

```bash

consul.exe agent -dev -advertise=127.0.0.1

```

You should now be able to view the [Consul UI](http://127.0.0.1:8500/ui)

Once you have added the plugin to your ServiceStack AppHost, you should see it appear
in the Consol UI when you start it.

### Service Registration

The plugin will register the service with a Consul agent. 
When the AppHost is shutdown, it will deregister the service.

#### Health checks

Each service can have a number of health checks. This allows service discovery to filter out failing instances of your services.

By default the plugin creates 2 health checks

1. Heartbeat : Creates an endpoint in your service [http://locahost:1234/reply/json/heartbeat](http://locahost:1234/reply/json/heartbeat) that expects a 200 response
2. If Redis has been configured in the AppHost, it will check Redis is responding

To turn off the default checks use the following:
```csharp
Plugins.Add(new ConsulFeature() { IncludeDefaultServiceHealth = false });
```

#### Custom health checks

You can add your own health checks

```csharp
using ConsulFeature() { ServiceChecks.Add(new ConsulRegisterCheck()) };
```  

### Discovery

The default discovery mechanism uses the ServiceStack request type names to resolve all of the services capable of processing the request. This means that you should always use unique names across all your services for each of your RequestDTO's

To resolve the correct service for a request, pass the request type to the extension method: 
```csharp
ServiceClientBase.TryGetClientFor<T>();
```
This will query consul for healthy services that can process the type and return a client

```csharp
// Go and find out where we need to send ExternalDTO and return the client for it.
var client = new JsonServiceClient().TryGetClientFor<ExternalDTO>();

// Query our remote service without knowing where it is
var result = client.Send(new ExternalDTO());
```


 


  
