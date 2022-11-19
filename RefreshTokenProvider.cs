using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin.Security.Infrastructure;
using System.Threading.Tasks;
using GOChatAPI.Models;
using Microsoft.Owin;
using Microsoft.Owin.Cors;

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

                ////Stores the refresh token in http only cookie
                //context.Response.Cookies.Append("refresh_token",
                //     refreshTokenId, new CookieOptions
                //     {
                //         Secure = true,//context.OwinContext.Request.IsSecure, 
                //         Expires = token.ExpiredTime,
                //         Path = context.Request.Uri.LocalPath,
                //         HttpOnly = true,
                //         SameSite = Microsoft.Owin.SameSiteMode.None,
                //     });
            }

        }

        /// <summary>
        /// Get's the refresh token Id from httpOnly cookie.
        /// Uses the retrieved refresh token id to get it's corresponding refresh token from the database, 
        /// if exists it deletes the token in the database in order to add a new one. 
        /// It also deserializes the protected ticket from the token if found inorder to build a new ticket 
        /// and identity for  the user mapped to this token. This proceeds to call the 'GrantRefreshToken()' method
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            //Get's the refresh token from httpOnly cookie
            //var refreshTokenId = !string.IsNullOrEmpty(context.Request.Cookies["refresh_token"]) && !(context.Request.Cookies["refresh_token"] == "undefined") ? context.Request.Cookies["refresh_token"] : context.Token;
            var refreshTokenId = context.Token;

            var allowedOrigin = context.OwinContext.Get<string>("ta:clientAllowedOrigin");
            //context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { allowedOrigin });
            string hashedToken = General.GetHash(refreshTokenId); //General.GetHash(context.Token);

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