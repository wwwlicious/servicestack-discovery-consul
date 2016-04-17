// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Discovery.Consul
{
    using System;

    public interface IDiscoveryRequestTypeResolver
    {
        /// <summary>
        /// Inspects the IAppHost and returns a list of strings that will represent the RequestDTO types
        /// These strings are used by <see cref="ResolveBaseUri(object)"/> to find the AppHost's BaseUri
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        string[] GetRequestTypes(IAppHost host);

        /// <summary>
        /// Takes a dto object and returns the correct BaserUri for the gateway to send it to
        /// </summary>
        /// <param name="dto">the request dto</param>
        /// <returns>the BaseUri that will serve this request</returns>
        string ResolveBaseUri(object dto);

        /// <summary>
        /// Takes a dto type and returns the correct BaseUri for the gateway to send it to
        /// </summary>
        /// <param name="dtoType">The request dto type</param>
        /// <returns>the BaserUri that will serve this request</returns>
        string ResolveBaseUri(Type dtoType);
    }
}