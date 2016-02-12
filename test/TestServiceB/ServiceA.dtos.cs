/* Options:
Date: 2016-02-12 18:09:08
Version: 4.052
Tip: To override a DTO option, remove "//" prefix before updating
BaseUrl: http://127.0.0.1:8091/

//GlobalNamespace: 
//MakePartial: True
//MakeVirtual: True
//MakeDataContractsExtensible: False
//AddReturnMarker: True
//AddDescriptionAsComments: True
//AddDataContractAttributes: False
//AddIndexesToDataMembers: False
//AddGeneratedCodeAttributes: False
//AddResponseStatus: False
//AddImplicitVersion: 
//InitializeCollections: True
//IncludeTypes: 
//ExcludeTypes: 
//AddDefaultXmlNamespace: http://schemas.servicestack.net/types
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ServiceStack;
using ServiceStack.DataAnnotations;
using TestServiceA;


namespace TestServiceA
{

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

