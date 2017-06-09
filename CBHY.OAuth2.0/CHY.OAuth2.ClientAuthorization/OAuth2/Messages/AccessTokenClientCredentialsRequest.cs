using CHY.OAuth2.OAuth2.ChannelElements;
using CHY.OAuth2.Core.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.ClientAuthorization.OAuth2.Messages
{
    public class AccessTokenClientCredentialsRequest:ScopedAccessTokenRequest, IAuthorizationCarryingRequest, IAuthorizationDescription
    {
        public AccessTokenClientCredentialsRequest(Uri tokenEndpoint, Version version)
            :base(tokenEndpoint, version)
        {
            this.HttpMethods = HttpDeliveryMethods.PostRequest;
        }

        IAuthorizationDescription IAuthorizationCarryingRequest.AuthorizationDescription
        {
            get { return this.ClientAuthenticated ? this : null; }
        }

        DateTime IAuthorizationDescription.UtcIssued
        {
            get { return DateTime.UtcNow; }
        }

        string IAuthorizationDescription.User
        {
            get { return null; }
        }
        HashSet<string> IAuthorizationDescription.Scope
        {
            get { return this.Scope; }
        }

        public override GrantType GrantType
        {
            get { return Messages.GrantType.ClientCredentials; }
        }
    }
}
