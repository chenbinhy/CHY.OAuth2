using CHY.OAuth2.ClientAuthorization.Messages;
using CHY.OAuth2.Core.Messaging;
using CHY.OAuth2.OAuth2.ChannelElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.AuthorizationServer.OAuth2.ChannelElements
{
    public class AggregatingClientCredentialReader:ClientAuthenticationModule
    {
        private readonly IEnumerable<ClientAuthenticationModule> authenticators;

        public AggregatingClientCredentialReader(IEnumerable<ClientAuthenticationModule> authenticators)
        {
            this.authenticators = authenticators;
        }

        public override string AuthenticateHeader
        {
            get
            {
                var builder = new StringBuilder();
                foreach(var authenticator in this.authenticators)
                {
                    string scheme = authenticator.AuthenticateHeader;
                    if(scheme != null)
                    {
                        if(builder.Length > 0)
                        {
                            builder.Append(", ");
                        }
                        builder.Append(scheme);
                    }
                }
                return builder.Length > 0 ? builder.ToString() : null;
            }
        }

        public override ClientAuthenticationResult TryAuthenticateClient(IAuthorizationServerHost authorizationServerHost, AuthenticatedClientRequestBase requestMessage, out string clientIdentifier)
        {
            ClientAuthenticationModule authenticator = null;
            ClientAuthenticationResult result = ClientAuthenticationResult.NoAuthenticationRecognized;
            clientIdentifier = null;
            foreach(var candidateAuthenticator in this.authenticators)
            {
                string candidateClientIdentifier;
                var resultCandidate = candidateAuthenticator.TryAuthenticateClient(authorizationServerHost, requestMessage, out candidateClientIdentifier);
                ErrorUtilities.VerifyProtocol(
                    result == ClientAuthenticationResult.NoAuthenticationRecognized || resultCandidate == ClientAuthenticationResult.NoAuthenticationRecognized,
                    "Message rejected because multiple forms of client authentication ({0} and {1}) were detected, which is forbidden by the OAuth 2 Protocol Framework specification.",
                    authenticator,
                    candidateAuthenticator);

                if(resultCandidate != ClientAuthenticationResult.NoAuthenticationRecognized)
                {
                    authenticator = candidateAuthenticator;
                    result = resultCandidate;
                    clientIdentifier = candidateClientIdentifier;
                }
            }

            return result;
        }
    }
}
