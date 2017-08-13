// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Discovery.Consul
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface used for service discovery
    /// </summary>
    /// <typeparam name="TServiceModel">The DTO for service information</typeparam>
    /// <typeparam name="TServiceRegistration">The DTO for the AppHost registration</typeparam>
    public interface IServiceDiscovery<out TServiceModel, out TServiceRegistration> where TServiceModel : class where TServiceRegistration : class
    {
        /// <summary>
        /// Holds the current service registration
        /// </summary>
        TServiceRegistration Registration { get; }

        /// <summary>
        /// Registers the service for discovery
        /// </summary>
        /// <param name="host"></param>
        void Register(IAppHost host);

        /// <summary>
        /// Unregisters the service from discovery
        /// </summary>
        /// <param name="host">the apphost</param>
        void Unregister(IAppHost host);

        /// <summary>
        /// Returns a list of available services
        /// </summary>
        /// <param name="consulAddress">the address of the consul server</param>
        /// <param name="serviceName">the service name</param>
        /// <returns>the matching services</returns>
        TServiceModel[] GetServices(string consulAddress, string serviceName);

        /// <summary>
        /// Returns a single service for a dto
        /// </summary>
        /// <param name="consulAddress">the address of the consul server</param>
        /// <param name="serviceName">the service name</param>
        /// <param name="dtoName">the request dto name</param>
        /// <returns>the service dto</returns>
        TServiceModel GetService(string consulAddress, string serviceName, string dtoName);

        /// <summary>
        /// Inspects the IAppHost and returns a list of strings that will represent the RequestDTO types
        /// These strings are used by <see cref="ResolveBaseUri(object)"/> to find the AppHost's BaseUri
        /// </summary>
        /// <param name="host">the apphost</param>
        /// <returns>list of types to register for discovery</returns>
        HashSet<Type> GetRequestTypes(IAppHost host);

        /// <summary>
        /// Takes a dto object and returns the correct BaserUri for the gateway to send it to
        /// </summary>
        /// <param name="consulAddress">the address of the consul server</param>
        /// <param name="dto">the request dto</param>
        /// <returns>the BaseUri that will serve this request</returns>
        string ResolveBaseUri(string consulAddress,object dto);

        /// <summary>
        /// Takes a dto type and returns the correct BaseUri for the gateway to send it to
        /// </summary>
        /// <param name="dtoType">The request dto type</param>
        /// <param name="consulRemoteAddress"></param>
        /// <returns>the BaserUri that will serve this request</returns>
        string ResolveBaseUri(string consulRemoteAddress, Type dtoType);
    }
}