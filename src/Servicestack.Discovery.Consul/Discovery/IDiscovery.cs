namespace ServiceStack.Discovery.Consul
{
    public interface IDiscovery
    {
        ServiceRegistration Registration { get; }
        void Register();
        void Unregister();
        ConsulService[] GetServices(string serviceName);
        ConsulService GetService(string serviceName, string dtoName);
    }
}