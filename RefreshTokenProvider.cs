using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin.Security.Infrastructure;
using System.Threading.Tasks;
using GOChatAPI.Models;

namespace GOChatAPI
{
    public class RefreshTokenProvider : IAuthenticationTokenProvider
    {
        /// <summary>
        /// Creates a new Refresh token, adds it to the database and the response body if the client id exists, else returns Invalid
        /// </summary>
        /// <param name="context">The context</param>
        /// <returns></returns>
        public async Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            //Get the client ID from the Ticket properties
            var clientid = context.Ticket.Properties.Dictionary["client_id"];
            if (string.IsNullOrEmpty(clientid))
            {
                return;
            }
            //Generating a Uniqure Refresh Token ID
            var refreshTokenId = Guid.NewGuid().ToString("n");

            // Getting the Refesh Token Life Time From the Owin Context
            var refreshTokenLifeTime = context.OwinContext.Get<string>("ta:clientRefreshTokenLifeTime");
            //Creating the Refresh Token object
            var token = new RefreshToken()
            {
                //storing the RefreshTokenId in hash format
                Token = General.GetHash(refreshTokenId),
                ClientId = clientid,
                UserId = context.Ticket.Identity.Name,
                IssuedTime = DateTime.UtcNow,
                ExpiredTime = DateTime.UtcNow.AddMinutes(Convert.ToDouble(refreshTokenLifeTime))
            };
            //Setting the Issued and Expired time of the Refresh Token
            context.Ticket.Properties.IssuedUtc = token.IssuedTime;
            context.Ticket.Properties.ExpiresUtc = token.ExpiredTime;
            token.ProtectedTicket = context.SerializeTicket();
            var result = General.ValidateRefreshToken(token);
            if (result)
            {
                context.SetToken(refreshTokenId);
            }

        }

        /// <summary>
        /// Get's previous refresh token by id from database, if exists it deletes 
        /// it in order to add a new one. It also deserializes the protected ticket 
        /// from the token if found inorder to build a new ticket and identity for 
        /// the user mapped to this token. This proceeds to call the 'GrantRefreshToken()' method
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            var allowedOrigin = context.OwinContext.Get<string>("ta:clientAllowedOrigin");
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { allowedOrigin });
            string hashedToken = General.GetHash(context.Token);

            var refreshToken = General.GetRefreshTokenByID(hashedToken);
            if (refreshToken != null)
            {
                //Get protectedTicket from refreshToken class
                context.DeserializeTicket(refreshToken.ProtectedTicket);
                var result = General.RemoveRefreshTokenByID(hashedToken);
            }
        }
        public void Create(AuthenticationTokenCreateContext context)
        {
            throw new NotImplementedException();
        }
        public void Receive(AuthenticationTokenReceiveContext context)
        {
            throw new NotImplementedException();
        }
    }
}