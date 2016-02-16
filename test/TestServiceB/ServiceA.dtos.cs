// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace TestServiceB
{
    using ServiceStack;

    public partial class EchoA
        : IReturn<EchoAReply>
    {
        public virtual bool CallRemoteService { get; set; }
    }

    public partial class EchoAReply
    {
        public virtual string Message { get; set; }
    }
}

