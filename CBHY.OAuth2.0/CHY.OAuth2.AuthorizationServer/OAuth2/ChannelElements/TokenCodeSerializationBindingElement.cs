using CHY.OAuth2.AuthorizationServer.OAuth2.Messages;
using CHY.OAuth2.ClientAuthorization.OAuth2.Messages;
using CHY.OAuth2.Core.Messaging;
using CHY.OAutho2.Core.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CHY.OAuth2.AuthorizationServer.OAuth2.ChannelElements
{
    public class TokenCodeSerializationBindingElement:AuthServerBindingElementBase
    {
        public override MessageProtections Protection
        {
            get { return MessageProtections.None; }
        }

        public override Task<MessageProtections?> ProcessOutgoingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken)
        {
            var directResponse = message as IDirectResponseProtocolMessage;
            var request = directResponse != null ? directResponse.OriginatingRequest as IAccessTokenRequestInternal : null;
            var authCodeCarrier = message as IAuthorizationCodeCarryingRequest;
            if(authCodeCarrier != null)
            {
                var codeFormatter = AuthorizationCode.CreateFormatter(this.AuthorizationServer);
                var code = authCodeCarrier.AuthorizationDescription;
                authCodeCarrier.Code = codeFormatter.Serialize(code);
                return MessageProtectionTasks.None;
            }

            var refreshTokenResponse = message as AccessTokenSuccessResponse;
            if(refreshTokenResponse != null && refreshTokenResponse.HasRefreshToken)
            {
                var refreshTokenCarrier = (IAuthorizationCodeCarryingRequest)message;
                var refreshToken = new RefreshToken(refreshTokenCarrier.AuthorizationDescription);
                var refreshTokenFormatter = RefreshToken.CreateFormatter(this.AuthorizationServer.CryptoKeyStore);
                refreshTokenResponse.RefreshToken = refreshTokenFormatter.Serialize(refreshToken);
            }

            var accessTokenResponse = message as IAccessTokenIssuingResponse;
            if (accessTokenResponse != null && accessTokenResponse.AuthorizationDescription != null)
            {
                ErrorUtilities.VerifyInternal(request != null, "We should always have a direct request message for this case.");
                accessTokenResponse.AccessToken = accessTokenResponse.AuthorizationDescription.Serialize();
            }
            return MessageProtectionTasks.Null;
        }

        public override Task<MessageProtections?> ProcessIncomingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken)
        {
            var authCodeCarrier = message as IAuthorizationCodeCarryingRequest;
            if(authCodeCarrier != null)
            {
                var authorizationCodeFormatter = AuthorizationCode.CreateFormatter(this.AuthorizationServer);
                var authorizationCode = new AuthorizationCode();
                authorizationCodeFormatter.Deserialize(authorizationCode, authCodeCarrier.Code, message, Protocol.code);
                authCodeCarrier.AuthorizationDescription = authorizationCode;
            }
            var refreshTokenCarrier = message as IRefreshTokenCarryingRequest;
            if(refreshTokenCarrier != null)
            {
                var refreshTokenFormatter = RefreshToken.CreateFormatter(this.AuthorizationServer.CryptoKeyStore);
                var refreshToken = new RefreshToken();
                refreshTokenFormatter.Deserialize(refreshToken, refreshTokenCarrier.RefreshToken, message, Protocol.refresh_token);
                refreshTokenCarrier.AuthorizationDescription = refreshToken;
            }

            return MessageProtectionTasks.Null;
        }
    }
}
