# ServiceStack.Discovery.Consul 
[![Build status](https://ci.appveyor.com/api/projects/status/55830emag9ksyasf/branch/master?svg=true)](https://ci.appveyor.com/project/wwwlicious/servicestack-discovery-consul)
[![NuGet version](https://badge.fury.io/nu/ServiceStack.Discovery.Consul.svg)](https://badge.fury.io/nu/ServiceStack.Discovery.Consul)

A plugin for [ServiceStack](https://servicestack.net/) that provides transparent service discovery using [Consul.io](http://consul.io) with automatic service registration and health checking.

This enables your servicestack instances to call one another, without either knowing where the other is, based solely on a copy of the requestDTO. Your services will not need to take any dependencies on each other and as you deploy updates to your services and they will automatically be registered and used without reconfiguing the existing services.

The customisable health checks for each service will also ensure that failing services will not be used, or if you run multiple instances of a service, only the healthy and most responsive service will be returned. 

![RequestDTO Service Discovery](assets/RequestDTOServiceDiscovery.png)

## Requirements

A consul agent must be running on the same machine as the AppHost.

## Quick Start

Install the package [https://www.nuget.org/packages/ServiceStack.Discovery.Consul](https://www.nuget.org/packages/ServiceStack.Discovery.Consul/)
```bash
PM> Install-Package ServiceStack.Discovery.Consul
```
Add the following to your `AppHost.Configure` method

```csharp

    public override void Configure(Container container)
    {
        SetConfig(new HostConfig
        {
            // the url:port that other services will use to access this one
            WebHostUrl = "http://api.acme.com:1234",

            // optional
            ApiVersion = "2.0",             
            HandlerFactoryPath = "/api/"
        });

        // Pass in any ServiceClient and it will be autowired with Func
        Plugins.Add(new ConsulFeature(new JsonServiceClient()));
    }
}
```
and use as follows
```csharp
public class MyService : Service
{
    // autowiring will inject configured client
    public IServiceClient Client { get; set; }

    public void Any(RequestDTO dto)
    {
        // will automatically find the correct uri endpoint for the the request using consul
        var response = Client.Post(new ExternalDTO { Custom = "bob" });
    }
}
```
## Running your services

Before you start your services, you'll need to [download consul](https://www.consul.io/) and start the agent running on your machine.


The following will create an in-memory instance which is useful for testing

```bash
consul.exe agent -dev -advertise="127.0.0.1"
```
You should now be able to view the [Consul UI](http://127.0.0.1:8500/ui)

### Automatic Service Registration

![Automatic Service Registration](assets/ServiceRegistration.png)

Once you have added the plugin to your ServiceStack AppHost, you should see it appear
in the Consul UI when you start it.

* Registers the service with a Consul agent once the AppHost has been initialised.
* Deregisters the service when the AppHost is shutdown.

#### Health checks

![Default Health Checks](assets/HealthChecks.png)

Each service can have a number of health checks. This allows service discovery to filter out failing instances of your services.

By default the plugin creates 2 health checks

1. Heartbeat : Creates an endpoint in your service [http://locahost:1234/reply/json/heartbeat](http://locahost:1234/reply/json/heartbeat) that expects a 200 response
2. If Redis has been configured in the AppHost, it will check Redis is responding

You can turn off the default health checks by setting the following property:
```csharp
Plugins.Add(new ConsulFeature() { IncludeDefaultServiceHealth = false });
```
#### Custom health checks

You can add your own health checks

```csharp
new ConsulFeature() { ServiceChecks.Add(new ConsulRegisterCheck()) };
```
### Discovery

The default discovery mechanism uses the ServiceStack request type names to resolve all of the services capable of processing the request. This means that you should always use unique names across all your services for each of your RequestDTO's

To override the default behaviour, you can implement your own `IDiscoveryRequestTypeResolver`

```csharp
public class CustomDiscoveryRequestTypeResolver : IDiscoveryRequestTypeResolver
{
    public string[] GetRequestTypes(IAppHost host)
    {
        // register dto's for reverse lookup below ...
    }

    public string ResolveBaseUri(object dto)
    {
        // reverse lookup service base uri from dto ...
    }
}
```

#### Autowiring Client

To autowire a client, pass it into the plugin constructor: `new ConsulFeature(new JsvServiceClient())` 
You can then use an `IServiceClient` in your service and everything will just work 

```csharp
public class EchoService : Service
{
    // will autowire the JsvServiceClient passed to the plugin constructor
    public IServiceClient Client { get; set; }

    public void Any(int num)
    {
        // this will resolve the correct remote uri using consul for the external DTO
        var remoteResponse = Client.Post(new RemoteDTO());
    }
}
```
#### Manual Client

If you dont want the service client to be autowired, don't pass a client to the plugin constructor and set the following client property

```csharp
var client = new JsonServiceClient { TypedUrlResolver = Consul.ResolveTypedUrl };
```

### Example

The following shows the services registered with consul and passing health checks and the services running on different IP:Port/Paths

![Services](assets/Services.png)




