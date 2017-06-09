using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging
{
    public enum HttpDeliveryMethods
    {
        None = 0x0,
        AuthorizationHeaderRequest = 0x1,
        PostRequest = 0x2,
        GetRequest = 0x4,
        PutRequest = 0x8,
        DeleteRequest = 0x10,
        HeadRequest = 0x20,
        PatchRequest = 0x40,
        OptionsRequest = 0x80,
        HttpVerbMask = PostRequest | GetRequest | PutRequest | DeleteRequest | HeadRequest | PatchRequest | OptionsRequest,
    }
}
