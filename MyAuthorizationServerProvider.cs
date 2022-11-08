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

        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated(); // 
        }

       /* public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var identity = new ClaimsIdentity(context.Options.AuthenticationType);

            string query = @"SELECT * FROM Users WHERE UserID=@userid AND PWord=@pword";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@pword", context.Password);
            cmd.Parameters.AddWithValue("@userid", context.UserName);

            //con.Open();
            using(SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["GOChat"].ConnectionString))
            {
                SqlDataReader read = cmd.ExecuteReader();

                bool count = read.HasRows;

                if (count && read.Read())
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, "admin"));
                    identity.AddClaim(new Claim("username", read["UserID"].ToString()));
                    identity.AddClaim(new Claim(ClaimTypes.Name, read["UserID"].ToString()));
                    context.Validated(identity);
                }
                else
                {
                    context.SetError("invalid_grant", "Provided username and password is incorrect");
                    read.Close();
                    con.Close();
                    return;
                }

                read.Close();
            }

            
            //con.Close();
        } */

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var identity = new ClaimsIdentity(context.Options.AuthenticationType);

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
                        context.Validated(identity);
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
    }
}