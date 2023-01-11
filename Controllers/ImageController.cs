using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace GOChatAPI.Controllers
{
    /// <summary>
    /// Controller for returning images
    /// </summary>
    [RoutePrefix("api/image")]
    public class ImageController : ApiController
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["GOChat"].ConnectionString);
        General general = new General();

        [HttpGet]
        [AllowAnonymous]
        [Route("{table}/{column}/{id}")]
        public HttpResponseMessage GetUserImage(string table, string column, string id)
        {
            SqlCommand cmd = new SqlCommand("GetUserImage", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@id", id);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    var response = new HttpResponseMessage();

                    if (row["ProfilePicture"].ToString() != null && row["ProfilePicture"].ToString() != "")
                    {
                        response.StatusCode = HttpStatusCode.OK;
                        response.Content = new ByteArrayContent((byte[])row["ProfilePicture"]);
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    }
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }
            }
            return null;
        }
    }
}
