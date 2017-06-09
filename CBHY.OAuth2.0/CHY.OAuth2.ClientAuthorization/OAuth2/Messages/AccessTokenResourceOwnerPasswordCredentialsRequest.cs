using CHY.OAuth2.Core.Messaging.Reflection;
using CHY.OAuth2.OAuth2.ChannelElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.ClientAuthorization.OAuth2.Messages
{
    public class AccessTokenResourceOwnerPasswordCredentialsRequest:ScopedAccessTokenRequest, IAuthorizationCarryingRequest, IAuthorizationDescription
    {
        public AccessTokenResourceOwnerPasswordCredentialsRequest(Uri accessTokenEndpoint, Version version)
            :base(accessTokenEndpoint, version)
        {

        }

        IAuthorizationDescription IAuthorizationCarryingRequest.AuthorizationDescription
        {
            get { return this.CredentialsValidated ? this : null; }
        }

        DateTime IAuthorizationDescription.UtcIssued
        {
            get { return DateTime.UtcNow; }
        }

        string IAuthorizationDescription.User
        {
            get { return this.RequestingUserName; }
        }

        HashSet<string> IAuthorizationDescription.Scope
        {
            get { return this.Scope; }
        }

        public override string UserName
        {
            get
            {
                return base.UserName??this.RequestingUserName;
            }
        }

        public override GrantType GrantType
        {
            get { return Messages.GrantType.Password; }
        }

        [MessagePart(Protocol.username, IsRequired=true)]
        public string RequestingUserName { get; set; }

        [MessagePart(Protocol.password, IsRequired=true, IsSecuritySensitive =true)]
        public string Password { get; set; }

        public bool CredentialsValidated { get; set; }
    }
}
