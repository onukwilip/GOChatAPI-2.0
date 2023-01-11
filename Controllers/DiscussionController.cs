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
    /// Handles all endpoints concerning discussions
    /// </summary>
    [Authorize]
    [RoutePrefix("api/discussion")]
    public class DiscussionController : ApiController
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["GOChat"].ConnectionString);
        General general = new General();

        /// <summary>
        /// Gets all discussions pertaining to a user
        /// </summary>
        /// <returns>Response object</returns>
        [HttpGet]
        public ResponseModel Get()
        {
            ResponseModel response = new ResponseModel();
            List<object> discussions = new List<object>();

            SqlCommand headCmd = new SqlCommand("Get_Disscussion_Heads", con);
            headCmd.CommandType = CommandType.StoredProcedure;
            headCmd.Parameters.AddWithValue("@MemberID", User.Identity.Name);
            SqlDataAdapter headSda = new SqlDataAdapter(headCmd);
            DataTable headDt = new DataTable();
            headSda.Fill(headDt);

            con.Open();

            int i = headDt.Rows.Count;

            if (i > 0)
            {
                foreach (DataRow headRow in headDt.Rows)
                {
                    DiscussionModel discussion = new DiscussionModel();

                    string imgSrc = String.Empty;

                    if (headRow["ChatRoomPicture"].ToString() != null && headRow["ChatRoomPicture"].ToString() != "")
                    {
                        string route = headRow["ChatRoomType"].ToString() == "Private" ? "user" : "chatroom";
                        imgSrc = $"{general.domain}/api/{route}/{headRow["IdentityToRender"].ToString()}/image";
                    }

                    discussion.ChatRoomID = headRow["ChatRoomID"].ToString();
                    discussion.ChatRoomName = headRow["ChatRoomName"].ToString();
                    discussion.MemberID = headRow["MemberID"].ToString();
                    discussion.Type = headRow["ChatRoomType"].ToString();
                    discussion.LastSeen = Convert.ToDateTime(headRow["LastSeen"]);
                    discussion.IsOnline= Convert.ToBoolean(headRow["IsOnline"]);
                    discussion.ProfilePicture = imgSrc;

                    SqlCommand memberCountCmd = new SqlCommand("Get_Disscussion_Members", con);
                    memberCountCmd.CommandType = CommandType.StoredProcedure;
                    memberCountCmd.Parameters.AddWithValue("@MemberID", User.Identity.Name);
                    memberCountCmd.Parameters.AddWithValue("@ChatRoomID", headRow["ChatRoomID"].ToString());

                    int memberCount = (int)memberCountCmd.ExecuteScalar();

                    discussion.Count = memberCount;

                    SqlCommand lastMessageCmd = new SqlCommand("Get_Latest_Discusion_Member", con);
                    lastMessageCmd.CommandType = CommandType.StoredProcedure;
                    lastMessageCmd.Parameters.AddWithValue("@MemberID", User.Identity.Name);
                    lastMessageCmd.Parameters.AddWithValue("@ChatRoomID", headRow["ChatRoomID"].ToString());
                    SqlDataReader lastMessageReader = lastMessageCmd.ExecuteReader();
                    if(lastMessageReader.Read())
                    {
                        discussion.LastMessage = lastMessageReader["Message"].ToString();
                    }
                    lastMessageReader.Close();

                    discussions.Add(discussion);
                }

                response.ResponseCode = (int)ResponseCodes.Successfull;
                response.ResponseMessage = ResponseCodes.Successfull.ToString();
                response.Data = discussions;
            }

            else
            {
                response.ResponseCode = (int)ResponseCodes.NotFound;
                response.ResponseMessage = ResponseCodes.NotFound.ToString();
            }

            con.Close();

            return response;
        }

        /// <summary>
        /// Posts a new discussion pertaining to a user
        /// </summary>
        /// <param name="body"></param>
        /// <returns>Response object</returns>
        [HttpPost]
        public ResponseModel Post(DiscussionModel body)
        {
            ResponseModel response = new ResponseModel();

            SqlCommand membersCmd = new SqlCommand("Select * from ChatRoomMembers where ChatRoomID=@chatroomid", con);
            membersCmd.Parameters.AddWithValue("@chatroomid", body.ChatRoomID);
            SqlDataAdapter membersSda = new SqlDataAdapter(membersCmd);
            DataTable membersDt = new DataTable();
            membersSda.Fill(membersDt);

            int membersCount = membersDt.Rows.Count;

            if (membersCount > 0)
            {
                foreach (DataRow member in membersDt.Rows)
                {
                    SqlCommand headCmd = new SqlCommand("Insert_Discussion", con);
                    headCmd.CommandType = CommandType.StoredProcedure;
                    headCmd.Parameters.AddWithValue("@ChatID", body.ChatID);
                    headCmd.Parameters.AddWithValue("@ChatRoomID", body.ChatRoomID);
                    headCmd.Parameters.AddWithValue("@MemberID", member["MemberID"].ToString());
                    headCmd.Parameters.AddWithValue("@Message", body.LastMessage);

                    con.Open();

                    int i = headCmd.ExecuteNonQuery();

                    con.Close();
                }

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
        /// Delete discussion group from database pertaining to a user
        /// </summary>
        /// <returns>Response object</returns>
        [HttpDelete]
        [Route("{base64chatroomid}")]
        public ResponseModel Delete(string base64chatroomid)
        {
            ResponseModel response = new ResponseModel();

            var ChatRoomByteCode = Convert.FromBase64String(base64chatroomid);
            string ChatRoomID = Encoding.UTF8.GetString(ChatRoomByteCode);

            SqlCommand cmd = new SqlCommand("Delete_Discussions", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ChatRoomID", ChatRoomID);
            cmd.Parameters.AddWithValue("@MemberID", User.Identity.Name);

            con.Open();

            int i = cmd.ExecuteNonQuery();

            con.Close();

            if (i > 0)
            {
                var success = ResponseCodes.Deleted;
                response.ResponseCode = (int)success;
                response.ResponseMessage = ResponseCodes.Deleted.ToString();
            }
            else
            {
                var fail = ResponseCodes.Unsuccessfull;
                response.ResponseCode = (int)fail;
                response.ResponseMessage = ResponseCodes.Unsuccessfull.ToString();
            }

            return response;
        }
    }
}
