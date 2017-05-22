// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Discovery.Consul
{
  using System;

  public static class ConsulUris
  {
    /// <summary>
    /// Local agent uri
    /// </summary>
    public const string LocalAgent = "http://127.0.0.1:8500";

    /// <summary>
    /// Uri for deregistering a service
    /// </summary>
    public static readonly Func<string, string> DeregisterService = serviceId => $"{LocalAgent}/v1/agent/service/deregister/{serviceId}";

    /// <summary>
    /// Uri for retrieving a list of servicestack services 
    /// </summary>
    /// <remarks><see cref="https://www.consul.io/docs/agent/http/catalog.html#catalog_services"/></remarks>
    //        public static readonly Func<string, string> GetServices = (service) => $"{LocalAgent}/v1/health/service/{service}?near=_agent&passing";
    public static readonly string GetServices = $"{LocalAgent}/v1/catalog/services";

    /// <summary>
    /// Uri for retrieving active instances of a service
    /// </summary>
    public static readonly Func<string, string, string> GetService = (service, tagName) => $"{LocalAgent}/v1/health/service/{service}?near=_agent&passing&tag={tagName}";
  }
}