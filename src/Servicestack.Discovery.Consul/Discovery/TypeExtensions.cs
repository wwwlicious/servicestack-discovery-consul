// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace ServiceStack.Discovery.Consul
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ServiceStack.DataAnnotations;

    public static class TypeExtensions
    {
        /// <summary>
        /// Filters out types with <see cref="ExcludeAttribute"/> 
        /// containing values of <see cref="Feature.Metadata"/> or <see cref="Feature.ServiceDiscovery"/>
        /// </summary>
        /// <param name="types">the types to filter</param>
        /// <returns>the filtered types</returns>
        public static IEnumerable<Type> WithServiceDiscoveryAllowed(this IEnumerable<Type> types)
        {
            return types
                .Where(x => x.AllAttributes<ExcludeAttribute>().All(a => (a.Feature & Feature.Metadata) != Feature.Metadata))
                .Where(x => x.AllAttributes<ExcludeAttribute>().All(a => (a.Feature & Feature.ServiceDiscovery) != Feature.ServiceDiscovery));
        }

        /// <summary>
        /// Filters out ServiceStack native type namespaces and types
        /// </summary>
        /// <param name="types">the types to filter</param>
        /// <param name="nativeTypes">the NativeTypesFeature</param>
        /// <returns>the filtered types</returns>
        public static IEnumerable<Type> WithoutNativeTypes(this IEnumerable<Type> types, NativeTypesFeature nativeTypes)
        {
            return nativeTypes == null

                ? types
                : types
                    .Where(x => !nativeTypes.MetadataTypesConfig.IgnoreTypes.Contains(x))
                    .Where(x => !nativeTypes.MetadataTypesConfig.IgnoreTypesInNamespaces.Contains(x.Namespace));
        }

        /// <summary>
        /// Filters out types without the <see cref="RestrictAttribute.ExternalOnly"/> flag
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public static IEnumerable<Type> WithoutExternalRestrictions(this IEnumerable<Type> types)
        {
            return types.Where(x => x.AllAttributes<RestrictAttribute>().All(a => a.CanShowTo(RequestAttributes.External)));
        }
    }
}