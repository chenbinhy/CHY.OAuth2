using CHY.OAuth2.ClientAuthorization.Messages;
using CHY.OAuth2.Core.Messaging;
using CHY.OAuth2.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Client.OAuth2
{
    public abstract class ClientCredentialApplicator
    {
        protected ClientCredentialApplicator()
        { 
        }

        public static ClientCredentialApplicator PostParameter(string clientSecret)
        {
            return new PostParameterApplicator(clientSecret);
        }

        public static ClientCredentialApplicator NetworkCredential(NetworkCredential credential)
        {
            return new NetworkCredentialApplicator(credential);
        }

        public static ClientCredentialApplicator NetworkCredential(string clientSecret)
        {
            return new NetworkCredentialApplicator(clientSecret);
        }

        public static ClientCredentialApplicator NoSecret()
        {
            return null;
        }

        public virtual void ApplyClientCredential(string clientIdentifier, AuthenticatedClientRequestBase request)
        { 
        }

        public virtual void ApplyClientCredential(string clientIdentifier, HttpRequestMessage request)
        { 
        }

        private class NetworkCredentialApplicator : ClientCredentialApplicator 
        {
            private readonly NetworkCredential credential;

            private readonly string clientSecret;

            internal NetworkCredentialApplicator(string clientSecret)
            {
                this.clientSecret = clientSecret;
            }

            internal NetworkCredentialApplicator(NetworkCredential credential)
            {
                this.credential = credential;
            }

            public override void ApplyClientCredential(string clientIdentifier, AuthenticatedClientRequestBase request)
            {
                request.ClientIdentifier = null;
                request.ClientSecret = null;
            }

            public override void ApplyClientCredential(string clientIdentifier, HttpRequestMessage request)
            {
                if(clientIdentifier != null)
                {
                    if (this.credential != null)
                    {
                        ErrorUtilities.VerifyHost(
                            string.Equals(this.credential.UserName, clientIdentifier, StringComparison.Ordinal),
                            "Client identifiers \"{0}\" and \"{1}\" do not match" ,
                            this.credential.UserName,
                            clientIdentifier
                            );
                    }
                }
                OAuthUtilities.ApplyHttpBasicAuth(request.Headers, clientIdentifier, this.clientSecret);
            }
        }
    }

    public class PostParameterApplicator : ClientCredentialApplicator
    {
        private readonly string secret;

        internal PostParameterApplicator(string clientSecret)
        {
            this.secret = clientSecret;
        }

        public override void ApplyClientCredential(string clientIdentifier, AuthenticatedClientRequestBase request)
        {
            if(clientIdentifier != null)
            {
                request.ClientSecret = this.secret;
            }
        }
    }
}
