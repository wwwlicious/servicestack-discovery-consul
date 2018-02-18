// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace TestServiceA
{
    using ServiceStack;

    [Route("/echo/b", "POST")]
    public partial class EchoB : IReturn<EchoBReply>
    {
        public virtual bool CallRemoteService { get; set; }
    }

    public partial class EchoBReply
    {
        public virtual string Message { get; set; }
    }
}

