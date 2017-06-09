using CHY.OAuth2.ClientAuthorization.OAuth2;
using CHY.OAuth2.Core.Messaging;
using OAuth2.Models.CHYAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBHY.OAuth2.AuthorizetionServer.Code
{
    public class ClientExt:Client,IClientDescription
    {
        public ClientExt(Client Info)
        {

            this.ClientType = Info.ClientType;
            this.Callback = Info.Callback;
            this.ClientSecret = Info.ClientSecret;
            this.ClientIdentifier = Info.ClientIdentifier;
            this.ClientId = Info.ClientId;
            this.Name = Info.Name;
        }

        ClientType IClientDescription.ClientType
        {
            get { return (ClientType)this.ClientType; }
        }

        public Uri DefaultCallback
        {
            get { return string.IsNullOrEmpty(Callback) ? null : new Uri(this.Callback); }
        }

        public bool HasNonEmptySecret
        {
            get { return !string.IsNullOrEmpty(this.ClientSecret); }
        }

        public bool IsCallbackAllowed(Uri callback)
        {
            if(string.IsNullOrEmpty(this.Callback))
            {
                return true;
            }

            Uri acceptableCallbackPattern = new Uri(this.Callback);
            if(string.Equals(acceptableCallbackPattern.GetLeftPart(UriPartial.Authority), callback.GetLeftPart(UriPartial.Authority), StringComparison.Ordinal))
            {
                return true;
            }

            return false;
        }

        public bool IsValidClientSecret(string secret)
        {
            return MessagingUtilities.EqualsConstantTime(secret, this.ClientSecret);
        }
    }
}