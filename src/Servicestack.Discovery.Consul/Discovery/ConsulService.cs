namespace ServiceStack.Discovery.Consul
{
    public class ConsulService
    {
        public ConsulService(ConsulServiceResponse response)
        {
            Id = response.ServiceID;
            Name = response.ServiceName;
            Tags = response.ServiceTags;
            Address = response.ServiceAddress;
            Version = response.ServiceTags.CreateVersion();
        }
        
        public string Id { get; set; }

        public string Address { get; set; }

        public string Name { get; set; }

        public string[] Tags { get; set; }

        /// <summary>
        /// apphost version, different from dto version
        /// </summary>
        public decimal Version { get; set; }
    }
}