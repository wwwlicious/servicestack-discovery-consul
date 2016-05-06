// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace ServiceStack.Discovery.Consul
{
    using System;
    using System.Linq;
    using ServiceStack.Host;

    public static class TagExtensions
    {
        public static decimal CreateVersion(this string[] tags)
        {
            var versionTagPrefix = "ss-version-";
            var versionTag = tags.SingleOrDefault(x => x.StartsWith(versionTagPrefix));
            if (versionTag == null) return -1;

            return decimal.Parse(versionTag.Substring(versionTagPrefix.Length));
        }
    }
}