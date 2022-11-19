using GOChatAPI.Models;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace GOChatAPI
{
    public class MyAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        //SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["GOChat"].ConnectionString);
        General general = new General();

        /// <summary>
        /// Called to validate that the origin of the request is a registered "client_id",
        ///     and that the correct credentials for that client are present on the request.
        ///     If the web application accepts Basic authentication credentials, context.TryGetBasicCredentials(out
        ///     clientId, out clientSecret) may be called to acquire those values if present
        ///     in the request header. If the web application accepts "client_id" and "client_secret"
        ///     as form encoded POST parameters, context.TryGetFormCredentials(out clientId,
        ///     out clientSecret) may be called to acquire those values if present in the request
        ///     body. If context.Validated is not called the request will not proceed further.
        /// Validate client basic auth before proceeding to generating the tokens
        /// </summary>
        /// <param name="context">The context</param>
        /// <returns></returns>

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            //context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            string clientId = string.Empty;
            string clientSecret = string.Empty;

            // The TryGetBasicCredentials method checks the Authorization header and
            // Return the ClientId and clientSecret

            if (!context.TryGetBasicCredentials(out clientId, out clientSecret))
            {
                context.SetError("invalid_client", "Client credentials could not be retrieved through the Authorization header, please make sure this client is registered with us.");

                return Task.FromResult<object>(null);
            }

            //Check the existence of by calling the ValidateClient method
            var client = General.ValidateClient(clientId, clientSecret);

            if (client == null)
            {
                // Client could not be validated.
                context.SetError("invalid_client", "Client credentials are invalid.");
                return Task.FromResult<object>(null);
            }
            else
            {
                if (client.Active < 1)
                {
                    context.SetError("invalid_client", "Client is inactive.");
                    return Task.FromResult<object>(null);
                }

                context.OwinContext.Set<ClientModel>("ta:client", client);
                context.OwinContext.Set<string>("ta:clientAllowedOrigin", client.AllowOrigin);
                context.OwinContext.Set<string>("ta:clientRefreshTokenLifeTime", client.RefreshTokenLifeTime.ToString());

                context.Validated();
                return Task.FromResult<object>(null);
            }
        }

        /// <summary>
        ///  Called when a request to the Token endpoint arrives with a "grant_type" of "password".
        ///     This occurs when the user has provided name and password credentials directly
        ///     into the client application's user interface, and the client application is using
        ///     those to acquire an "access_token" and optional "refresh_token". If the web application
        ///     supports the resource owner credentials grant type it must validate the context.Username
        ///     and context.Password as appropriate. To issue an access token the context.Validated
        ///     must be called with a new ticket containing the claims about the resource owner
        ///     which should be associated with the access token. The application should take
        ///     appropriate measures to ensure that the endpoint isn’t abused by malicious callers.
        ///     The default behavior is to reject this grant type. See also http://tools.ietf.org/html/rfc6749#section-4.3.2
        /// If client basic auth is validated, generate the access_token
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns></returns>

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {

            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            ClientModel client = context.OwinContext.Get<ClientModel>("ta:client");
            var allowedOrigin = context.OwinContext.Get<string>("ta:clientAllowedOrigin");

            if (allowedOrigin == null)
            {
                allowedOrigin = "*";
            }

            //context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { allowedOrigin });

            //con.Open();
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["GOChat"].ConnectionString))
            {

                string query = @"SELECT * FROM Users WHERE UserID=@userid";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@userid", context.UserName);

                con.Open();
                SqlDataReader read = cmd.ExecuteReader();

                bool count = read.HasRows;

                if (count && read.Read())
                {
                    string password = general.Decrypt(read["PWord"].ToString());
                    //string enteredPassword = general.Decrypt(context.Password);

                    if (context.Password == password)
                    {

                        identity.AddClaim(new Claim(ClaimTypes.Role, "admin"));
                        identity.AddClaim(new Claim("username", read["UserID"].ToString()));
                        identity.AddClaim(new Claim(ClaimTypes.Name, read["UserID"].ToString()));
                        var props = new AuthenticationProperties(new Dictionary<string, string> {
                            {
                                "client_id", (context.ClientId==null) ? string.Empty : context.ClientId
                            },
                            {
                                "UserId", context.UserName
                            }
                        });
                        var ticket = new AuthenticationTicket(identity, props);
                        context.Validated(ticket);
                    }
                    else
                    {
                        context.SetError("invalid_grant", "Provided password is incorrect");
                        read.Close();
                        con.Close();
                        return;
                    }

                }
                else
                {
                    context.SetError("invalid_grant", "Provided username is incorrect");
                    read.Close();
                    con.Close();
                    return;
                }

                read.Close();
            }


            //con.Close();
        }

        /// <summary>
        /// Called at the final stage of a successful Token endpoint request. An application
        ///     may implement this call in order to do any final modification of the claims being
        ///     used to issue access or refresh tokens. This call may also be used in order to
        ///     add additional response parameters to the Token endpoint's json response body.
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns></returns>

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }

        /// <summary>
        ///   Called when a request to the Token endpoint arrives with a "grant_type" of "refresh_token".
        ///     This occurs if your application has issued a "refresh_token" along with the "access_token",
        ///     and the client is attempting to use the "refresh_token" to acquire a new "access_token",
        ///     and possibly a new "refresh_token". To issue a refresh token the an Options.RefreshTokenProvider
        ///     must be assigned to create the value which is returned. The claims and properties
        ///    associated with the refresh token are present in the context.Ticket. The application
        ///     must call context.Validated to instruct the Authorization Server middleware to
        ///     issue an access token based on those claims and properties. The call to context.Validated
        ///     may be given a different AuthenticationTicket or ClaimsIdentity in order to control
        ///     which information flows from the refresh token to the access token. The default
        ///     behavior when using the OAuthAuthorizationServerProvider is to flow information
        ///     from the refresh token to the access token unmodified. See also http://tools.ietf.org/html/rfc6749#section-6
        /// </summary>
        /// <param name="context"> The context of the event carries information in and results out.</param>
        /// <returns>Task to enable asynchronous execution</returns>

        public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            var originalClient = context.Ticket.Properties.Dictionary["client_id"];
            var currentCLient = context.ClientId;

            if (originalClient!=currentCLient)
            {
                context.SetError("Invalid ClientId", "Refresh token is issued to a different client id");
                return Task.FromResult<object>(null);
            }
            // Change auth ticket for refresh token requests
            var newIdentity = new ClaimsIdentity(context.Ticket.Identity);
            newIdentity.AddClaim(new Claim("newClaim", "newValue"));

            var newTicket = new AuthenticationTicket(newIdentity, context.Ticket.Properties);
            context.Validated(newTicket);

            return Task.FromResult<object>(null);
        }
    }
}