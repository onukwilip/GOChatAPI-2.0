using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Configuration;
using System.Data;
using GOChatAPI.Models;
using System.Data.SqlClient;
using System.Text;

namespace GOChatAPI.Controllers
{
    /// <summary>
    /// Handles all API endpoints concerning notifications
    /// </summary>
    [RoutePrefix("api/notification")]
    public class NotificationController : ApiController
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["GOChat"].ConnectionString);
        General general = new General();

        /// <summary>
        /// Gets all notifications concerning a user
        /// </summary>
        /// <returns>Response object</returns>
        // GET: api/Notification
        [HttpGet]
        public ResponseModel Get()
        {
            ResponseModel response = new ResponseModel();

            List<object> notifications = new List<object>();

            SqlCommand cmd = new SqlCommand("Get_Notifications", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@userid", User.Identity.Name);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);

            int i = dt.Rows.Count;

            if (i > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    NotificationModel notification = new NotificationModel();
                    IdentityToRenderModel identityToRender = new IdentityToRenderModel();
                    string imgSrc = String.Empty;

                    if (row["IdentityToRenderProfilePicture"].ToString() != null && row["IdentityToRenderProfilePicture"].ToString() != "")
                    {
                        var base64 = Convert.ToBase64String((byte[])row["IdentityToRenderProfilePicture"]);
                        imgSrc = String.Format("data:image/png;base64, {0}", base64);
                    }

                    notification.ID = (int)row["ID"];
                    notification.UserID = row["UserID"].ToString();
                    notification.Message = row["Message"].ToString();
                    notification.Type = row["Type"].ToString();
                    notification.Target = row["Target"].ToString();
                    notification.Viewed = row["Viewed"] != null ? Convert.ToBoolean(row["Viewed"]) : false;
                    identityToRender.IdentityToRenderID = row["IdentityToRenderID"].ToString();
                    identityToRender.IdentityToRenderName = row["IdentityToRenderName"].ToString();
                    identityToRender.IdentityToRenderProfilePicture = imgSrc;
                    identityToRender.IsOnline = Convert.ToBoolean(row["IsOnline"]);
                    notification.IdentityToRender = identityToRender;

                    notifications.Add(notification);
                }

                response.ResponseCode = (int)ResponseCodes.Successfull;
                response.ResponseMessage = ResponseCodes.Successfull.ToString();
                response.Data = notifications;
            }
            else
            {
                response.ResponseCode = (int)ResponseCodes.NotFound;
                response.ResponseMessage = ResponseCodes.NotFound.ToString();
            }

            return response;
        }


        /// <summary>
        /// Gets all notifications concerning a group
        /// </summary>
        /// <param name="groupid">The Id of the chatroom</param>
        /// <returns>Response object</returns>
        // GET: api/Notification
        [HttpGet]
        [Route("{groupid}/group")]
        public ResponseModel Get_Group(string groupid)
        {
            ResponseModel response = new ResponseModel();

            List<object> notifications = new List<object>();

            SqlCommand cmd = new SqlCommand("Get_Notifications", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@userid", groupid);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);

            int i = dt.Rows.Count;

            if (i > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    NotificationModel notification = new NotificationModel();
                    IdentityToRenderModel identityToRender = new IdentityToRenderModel();
                    string imgSrc = String.Empty;

                    if (row["IdentityToRenderProfilePicture"].ToString() != null && row["IdentityToRenderProfilePicture"].ToString() != "")
                    {
                        var base64 = Convert.ToBase64String((byte[])row["IdentityToRenderProfilePicture"]);
                        imgSrc = String.Format("data:image/png;base64, {0}", base64);
                    }

                    notification.ID = (int)row["ID"];
                    notification.UserID = row["UserID"].ToString();
                    notification.Message = row["Message"].ToString();
                    notification.Type = row["Type"].ToString();
                    notification.Target = row["Target"].ToString();
                    notification.Viewed = row["Viewed"] != null ? Convert.ToBoolean(row["Viewed"]) : false;
                    identityToRender.IdentityToRenderID = row["IdentityToRenderID"].ToString();
                    identityToRender.IdentityToRenderName = row["IdentityToRenderName"].ToString();
                    identityToRender.IdentityToRenderProfilePicture = imgSrc;
                    identityToRender.IsOnline = Convert.ToBoolean(row["IsOnline"]);
                    notification.IdentityToRender = identityToRender;

                    notifications.Add(notification);
                }

                response.ResponseCode = (int)ResponseCodes.Successfull;
                response.ResponseMessage = ResponseCodes.Successfull.ToString();
                response.Data = notifications;
            }
            else
            {
                response.ResponseCode = (int)ResponseCodes.NotFound;
                response.ResponseMessage = ResponseCodes.NotFound.ToString();
            }

            return response;
        }


        /// <summary>
        /// Get's one notification
        /// </summary>
        /// <param name="id">id of the notification</param>
        /// <returns></returns>
        // GET: api/Notification/5
        [HttpGet]
        public ResponseModel Get(string id)
        {
            ResponseModel response = new ResponseModel();
           
            return response;
        }

        /// <summary>
        /// Posts a new notification into the database
        /// </summary>
        /// <param name="notification">The object in which the JSON body will be mapped into</param>
        /// <returns>Response object</returns>
        // POST: api/Notification
        [HttpPost]
        public ResponseModel Post(List<NotificationModel> notifications)
        {
            ResponseModel response = new ResponseModel();

            foreach (NotificationModel notification in notifications)
            {
                SqlCommand cmd = new SqlCommand("Insery_Notification", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@message", notification.Message);
                cmd.Parameters.AddWithValue("@identityToRender", notification.IdentityToRender.IdentityToRenderID);
                cmd.Parameters.AddWithValue("@userid", notification.UserID);
                cmd.Parameters.AddWithValue("@type", notification.Type);
                cmd.Parameters.AddWithValue("@target", notification.Target);

                con.Open();

                int i = cmd.ExecuteNonQuery();

                con.Close();

                if (i > 0)
                {
                    response.ResponseCode = (int)ResponseCodes.Successfull;
                    response.ResponseMessage = ResponseCodes.Successfull.ToString();
                }

                else
                {
                    response.ResponseCode = (int)ResponseCodes.Unsuccessfull;
                    response.ResponseMessage = ResponseCodes.Unsuccessfull.ToString();
                }
            }

            return response;
        }

        /// <summary>
        /// Update's a notification from the database
        /// </summary>
        /// <param name="id">The ID of the notification to be updated</param>
        /// <returns>Response object</returns>
        // UPDATE: api/Notification/5
        [HttpPut]
        [Route("{id}")]
        public ResponseModel Put(int id)
        {
            ResponseModel response = new ResponseModel();
            SqlCommand cmd = new SqlCommand("Update_Notification_Viewed_Status", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@id", id);
           
            con.Open();

            int i = cmd.ExecuteNonQuery();

            con.Close();

            if (i > 0)
            {
                response.ResponseCode = (int)ResponseCodes.Successfull;
                response.ResponseMessage = ResponseCodes.Successfull.ToString();
            }

            else
            {
                response.ResponseCode = (int)ResponseCodes.Unsuccessfull;
                response.ResponseMessage = ResponseCodes.Unsuccessfull.ToString();
            }

            return response;
        }

        /// <summary>
        /// Delete's a notification from the database
        /// </summary>
        /// <param name="id">The ID of the notification to be deleted</param>
        /// <returns>Response object</returns>
        // DELETE: api/Notification/5
        [HttpDelete]
        [Route("{id}")]
        public ResponseModel Delete(int id)
        {
            ResponseModel response = new ResponseModel();

            SqlCommand cmd = new SqlCommand("Delete_Notification", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@id", id);

            con.Open();

            int i = cmd.ExecuteNonQuery();

            con.Close();

            if (i > 0)
            {
                response.ResponseCode = (int)ResponseCodes.Deleted;
                response.ResponseMessage = ResponseCodes.Deleted.ToString();
            }

            else
            {
                response.ResponseCode = (int)ResponseCodes.Unsuccessfull;
                response.ResponseMessage = ResponseCodes.Unsuccessfull.ToString();
            }

            return response;
        }
    }
}
