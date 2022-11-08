using GOChatAPI.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace GOChatAPI.MessageHandler
{
    public class AuthHandler : DelegatingHandler
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["GOChat"].ConnectionString);

        string _userName = "";
        string _ip = "";

        //Method to validate credentials from Authorization
        //header value
        private bool ValidateCredentials(AuthenticationHeaderValue authenticationHeaderVal)
        {
            bool msg = false;

            try
            {
                if (authenticationHeaderVal != null
                    && !String.IsNullOrEmpty(authenticationHeaderVal.Parameter))
                {

                    string[] decodedCredentials
                    = Encoding.ASCII.GetString(Convert.FromBase64String(
                    authenticationHeaderVal.Parameter))
                    .Split(new[] { ':' });

                    //now decodedCredentials[0] will contain
                    //username and decodedCredentials[1] will
                    //contain password.

                    string query = @"SELECT * FROM LogUser WHERE UserID=@userid AND IPAddress=@ipaddress";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@ipaddress", decodedCredentials[1]);
                    cmd.Parameters.AddWithValue("@userid", decodedCredentials[0]);

                    con.Open();

                    SqlDataReader read = cmd.ExecuteReader();

                    bool count = read.HasRows;

                    if (count && read.Read())
                    {
                        _userName = read["UserID"].ToString();
                        _ip = read["IPAddress"].ToString();
                        General.IPAddress = read["IPAddress"].ToString();
                                               
                        msg = true;//request authenticated.
                    }
                    else
                    {
                        General.IPAddress = "";
                        msg = false;//request not authenticated.
                    }

                    read.Close();
                    con.Close();

                }
            }
            catch(Exception ex)
            {
               msg = false;
            }

            return msg;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //if the credentials are validated,
            //set CurrentPrincipal and Current.User
            if (ValidateCredentials(request.Headers.Authorization))
            {
                Thread.CurrentPrincipal = new TestAPIPrincipal(_userName, _ip);
                HttpContext.Current.User = new TestAPIPrincipal(_userName, _ip);
            }
            //Execute base.SendAsync to execute default
            //actions and once it is completed,
            //capture the response object and add
            //WWW-Authenticate header if the request
            //was marked as unauthorized.

            //Allow the request to process further down the pipeline
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized
                && !response.Headers.Contains("WwwAuthenticate"))
            {
                response.Headers.Add("WwwAuthenticate", "Basic");
            }

            return response;
        }

    }
}