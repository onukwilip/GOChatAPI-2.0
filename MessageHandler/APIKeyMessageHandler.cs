using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Data.SqlClient;
using System.Configuration;
using GOChatAPI.Models;

namespace GOChatAPI.MessageHandler
{
    public class APIKeyMessageHandler: DelegatingHandler
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["GOChat"].ConnectionString);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
        {
            bool validKey = false;
            IEnumerable<string> requestIPHeaders;
            IEnumerable<string> requestUserIDHeaders;

            var ipExists = httpRequestMessage.Headers.TryGetValues("IPAddress", out requestIPHeaders);
            var UserIDExists = httpRequestMessage.Headers.TryGetValues("UserID", out requestUserIDHeaders);

            if (ipExists && UserIDExists)
            {
                var IPAddress = requestIPHeaders.FirstOrDefault();
                var UserID = requestUserIDHeaders.FirstOrDefault();

                string query = @"SELECT COUNT(*) FROM LogUser WHERE UserID=@userid AND IPAddress=@ipaddress";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ipaddress", IPAddress);
                cmd.Parameters.AddWithValue("@userid", UserID);

                con.Open();

                int count = (int) cmd.ExecuteScalar();

                con.Close();

                if (count > 0)
                {
                    validKey = true;
                }
            }

            if (!validKey)
            {
                ResponseModel response1 = new ResponseModel();

                response1.ResponseCode = 401;
                response1.ResponseMessage = "UnAuthorized";

                return httpRequestMessage.CreateResponse(HttpStatusCode.Forbidden, response1);
            }

            var response = await base.SendAsync(httpRequestMessage, cancellationToken);
            return response;
        }
    }
}