using CHY.OAuth2.ClientAuthorization.OAuth2.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBHY.OAuth2.AuthorizetionServer.Models
{
    public class AccountAuthorizeModel
    {
        public string ClientApp { get; set; }

        public HashSet<string> Scope { get; set; }

        public EndUserAuthorizationRequest AuthorizationRequest { get; set; }
    }
}