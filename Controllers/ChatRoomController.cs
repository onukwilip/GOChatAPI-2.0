using GOChatAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;

namespace GOChatAPI.Controllers
{
    /// <summary>
    /// Handles all API's and routes concerning chatrooms
    /// </summary>
    [Authorize]
    [RoutePrefix("api/chatroom")]
    public class ChatRoomController : ApiController
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["GOChat"].ConnectionString);
        General general = new General();
        /// <summary>
        /// Gets all chatRooms of type group in the database
        /// </summary>
        /// <returns>Response object</returns>
        [Route("group")]
        [HttpGet]
        public ResponseModel GetChatRooms_Group()
        {
            ResponseModel response = new ResponseModel();
            List<object> chatRooms = new List<object>();

            SqlCommand cmd = new SqlCommand("GetChatRoom_Group", con);
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);

            int i = dt.Rows.Count;

            if (i > 0)
            {
                for (int j = 0; j < i; j++)
                {
                    ChatRoomModel chatRoom = new ChatRoomModel();

                    string imgSrc = String.Empty;

                    if (dt.Rows[j]["ChatRoomPicture"].ToString() != null && dt.Rows[j]["ChatRoomPicture"].ToString() != "")
                    {
                        var base64 = Convert.ToBase64String((byte[])dt.Rows[j]["ChatRoomPicture"]);
                        imgSrc = String.Format("data:image/png;base64, {0}", base64);
                    }

                    chatRoom.ChatRoomID = dt.Rows[j]["ChatRoomID"].ToString();
                    chatRoom.ChatRoomName = dt.Rows[j]["ChatRoomName"].ToString();
                    chatRoom.ProfilePicture = imgSrc;
                    chatRoom.Type = dt.Rows[j]["ChatRoomType"].ToString();

                    chatRooms.Add(chatRoom);
                }

                int success = (int)ResponseCodes.Successfull;
                response.ResponseCode = success;
                response.ResponseMessage = ResponseCodes.Successfull.ToString();
                response.Data = chatRooms;
            }
            else
            {
                int noChatRoom = (int)ResponseCodes.NoChatRoom;
                response.ResponseCode = noChatRoom;
                response.ResponseMessage = ResponseCodes.NoChatRoom.ToString();
            }

            return response;
        }

        /// <summary>
        /// Gets all groups created by a user in the database
        /// </summary>
        /// <returns>Response object</returns>
        [Route("user-groups")]
        [HttpGet]
        public ResponseModel GetUserGroups()
        {
            ResponseModel response = new ResponseModel();
            List<object> chatRooms = new List<object>();

            SqlCommand cmd = new SqlCommand("Get_User_Groups", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@userid", User.Identity.Name);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);

            int i = dt.Rows.Count;

            if (i > 0)
            {
                for (int j = 0; j < i; j++)
                {
                    ChatRoomModel chatRoom = new ChatRoomModel();

                    string imgSrc = String.Empty;

                    if (dt.Rows[j]["ChatRoomPicture"].ToString() != null && dt.Rows[j]["ChatRoomPicture"].ToString() != "")
                    {
                        var base64 = Convert.ToBase64String((byte[])dt.Rows[j]["ChatRoomPicture"]);
                        imgSrc = String.Format("data:image/png;base64, {0}", base64);
                    }

                    chatRoom.ChatRoomID = dt.Rows[j]["ChatRoomID"].ToString();
                    chatRoom.ChatRoomName = dt.Rows[j]["ChatRoomName"].ToString();
                    chatRoom.ProfilePicture = imgSrc;
                    chatRoom.Type = dt.Rows[j]["ChatRoomType"].ToString();

                    chatRooms.Add(chatRoom);
                }

                int success = (int)ResponseCodes.Successfull;
                response.ResponseCode = success;
                response.ResponseMessage = ResponseCodes.Successfull.ToString();
                response.Data = chatRooms;
            }
            else
            {
                int noChatRoom = (int)ResponseCodes.NoChatRoom;
                response.ResponseCode = noChatRoom;
                response.ResponseMessage = ResponseCodes.NoChatRoom.ToString();
            }

            return response;
        }


        /// <summary>
        /// Gets all fellas associated to a user except those who are already members of this group with the given chatroomId in the database
        /// </summary>
        /// <param name="chatroomId">The id of the group</param>
        /// <returns>Response object</returns>
        [Route("{chatroomId}/fellas/group")]
        [HttpGet]
        public ResponseModel GetUser_Fellas_Minus_Group_Members(string chatroomId)
        {
            ResponseModel response = new ResponseModel();
            List<object> users = new List<object>();

            SqlCommand cmd = new SqlCommand("Get_User_Fellas_Minus_Group_Members", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@userid", User.Identity.Name);
            cmd.Parameters.AddWithValue("@chatroomid", chatroomId);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);

            int i = dt.Rows.Count;

            if (i > 0)
            {
                for (int j = 0; j < i; j++)
                {
                    UserModel user = new UserModel();

                    string imgSrc = String.Empty;

                    if (dt.Rows[j]["ProfilePicture"].ToString() != null && dt.Rows[j]["ProfilePicture"].ToString() != "")
                    {
                        var base64 = Convert.ToBase64String((byte[])dt.Rows[j]["ProfilePicture"]);
                        imgSrc = String.Format("data:image/png;base64, {0}", base64);
                    }

                    user.UserID = dt.Rows[j]["UserID"].ToString();
                    user.UserName = dt.Rows[j]["UserName"].ToString();
                    user.ProfilePicture = imgSrc;
                    user.Email = dt.Rows[j]["Email"].ToString();
                    user.Description = dt.Rows[j]["Description"].ToString();
                    user.IsOnline = Convert.ToBoolean(dt.Rows[j]["IsOnline"]);
                    user.LastSeen = Convert.ToDateTime(dt.Rows[j]["LastSeen"]); 

                    users.Add(user);
                }

                int success = (int)ResponseCodes.Successfull;
                response.ResponseCode = success;
                response.ResponseMessage = ResponseCodes.Successfull.ToString();
                response.Data = users;
            }
            else
            {
                int notFound = (int)ResponseCodes.NotFound;
                response.ResponseCode = notFound;
                response.ResponseMessage = ResponseCodes.NotFound.ToString();
            }

            return response;
        }

        /// <summary>
        /// Gets all users except those who are already members of this group with the given chatroomId in the database
        /// </summary>
        /// <param name="chatroomId">The id of the group</param>
        /// <returns>Response object</returns>
        [Route("{chatroomId}/users/group")]
        [HttpGet]
        public ResponseModel GetUser_Users_Minus_Group_Members(string chatroomId)
        {
            ResponseModel response = new ResponseModel();
            List<object> users = new List<object>();

            SqlCommand cmd = new SqlCommand("Get_All_Users_Minus_Group_Member", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@chatroomid", chatroomId);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);

            int i = dt.Rows.Count;

            if (i > 0)
            {
                for (int j = 0; j < i; j++)
                {
                    UserModel user = new UserModel();

                    string imgSrc = String.Empty;

                    if (dt.Rows[j]["ProfilePicture"].ToString() != null && dt.Rows[j]["ProfilePicture"].ToString() != "")
                    {
                        var base64 = Convert.ToBase64String((byte[])dt.Rows[j]["ProfilePicture"]);
                        imgSrc = String.Format("data:image/png;base64, {0}", base64);
                    }

                    user.UserID = dt.Rows[j]["UserID"].ToString();
                    user.UserName = dt.Rows[j]["UserName"].ToString();
                    user.ProfilePicture = imgSrc;
                    user.Email = dt.Rows[j]["Email"].ToString();
                    user.Description = dt.Rows[j]["Description"].ToString();
                    user.IsOnline = Convert.ToBoolean(dt.Rows[j]["IsOnline"]);
                    user.LastSeen = Convert.ToDateTime(dt.Rows[j]["LastSeen"]);

                    users.Add(user);
                }

                int success = (int)ResponseCodes.Successfull;
                response.ResponseCode = success;
                response.ResponseMessage = ResponseCodes.Successfull.ToString();
                response.Data = users;
            }
            else
            {
                int notFound = (int)ResponseCodes.NotFound;
                response.ResponseCode = notFound;
                response.ResponseMessage = ResponseCodes.NotFound.ToString();
            }

            return response;
        }

        /// <summary>
        /// Deletes OR blocks private chatroom users, chats and the chatroom itself
        /// </summary>
        /// <param name="UserID">The ID of the user sending the request</param>
        /// <param name="Base64Password">The base64 password of the user sending the request</param>
        /// <param name="Base64IPAddress">The IP address of the user sending this request</param>
        /// <param name="RecipientID">The ID of the recipient</param>
        /// <returns>Response object</returns>
        [Route("{UserID}/{RecipientID}/{Base64IPAddress}/{Base64Password}/block")]
        [HttpDelete]
        public ResponseModel Block_Fella(string UserID, string RecipientID, string Base64IPAddress, string Base64Password)
        {
            ResponseModel response = new ResponseModel();

            var ByteCode = Convert.FromBase64String(Base64IPAddress);
            string IPAddress = Encoding.UTF8.GetString(ByteCode);

            var PaswordByteCode = Convert.FromBase64String(Base64Password);
            string Password = Encoding.UTF8.GetString(PaswordByteCode);

            SqlCommand cmdUser = new SqlCommand("GetUserByID", con);
            cmdUser.CommandType = CommandType.StoredProcedure;
            cmdUser.Parameters.AddWithValue("@UserID", UserID);
            cmdUser.Parameters.AddWithValue("@IPAddress", IPAddress);
            SqlDataAdapter sda = new SqlDataAdapter(cmdUser);
            DataTable dt = new DataTable();
            sda.Fill(dt);

            int row = dt.Rows.Count;

            if (row > 0)
            {
                string encryptedPassword = dt.Rows[0]["PWord"].ToString();
                string decryptedPassword = general.Decrypt(encryptedPassword);

                if (Password == decryptedPassword)
                {
                    SqlCommand cmd = new SqlCommand("Block_Fella", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@From_ID", UserID);
                    cmd.Parameters.AddWithValue("@To_ID", RecipientID);
                    cmd.Parameters.AddWithValue("@UserID", UserID);
                    cmd.Parameters.AddWithValue("@IPAddress", IPAddress);

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
                else
                {
                    response.ResponseCode = (int)ResponseCodes.Unsuccessfull;
                    response.ResponseMessage = ResponseCodes.Unsuccessfull.ToString();
                }
            }

            else
            {
                response.ResponseCode = (int)ResponseCodes.Unsuccessfull;
                response.ResponseMessage = ResponseCodes.Unsuccessfull.ToString();
            }

            return response;
        }

        /// <summary>
        /// Temporary blocks or ignores a user in a chatroom
        /// </summary>
        /// <param name="UserID">ID of the user sending the request</param>
        /// <param name="RecipientID">ID of the user to be ignored</param>
        /// <param name="Base64IPAddress">Base64 IP address of user sending the request</param>
        /// <returns>Response object</returns>
        [HttpDelete]
        [Route("{UserID}/{RecipientID}/{Base64IPAddress}/ignore")]
        public ResponseModel Ignore_Fella(string UserID, string RecipientID, string Base64IPAddress)
        {
            ResponseModel response = new ResponseModel();

            var ByteCode = Convert.FromBase64String(Base64IPAddress);
            string IPAddress = Encoding.UTF8.GetString(ByteCode);

            SqlCommand cmd = new SqlCommand("Ignore_Fella", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserID", UserID);
            cmd.Parameters.AddWithValue("@IPAddress", IPAddress);
            cmd.Parameters.AddWithValue("@RecipientID", RecipientID);

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
        /// Removes a user as a member of a chatroom
        /// </summary>
        /// <param name="groupid">ID of the group to be exited</param>
        /// <returns>Response object</returns>
        [HttpDelete]
        [Route("{groupid}")]
        public ResponseModel Exit_Group(string groupid)
        {
            ResponseModel response = new ResponseModel();

            SqlCommand cmd = new SqlCommand("Exit_Group", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SenderID", User.Identity.Name);
            cmd.Parameters.AddWithValue("@groupid", groupid);

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
        /// Removes a user as a member of a chatroom
        /// </summary>
        /// <param name="userid">ID of the user to be removed</param>
        /// <param name="groupid">ID of the group to be exited</param>
        /// <returns>Response object</returns>
        [HttpDelete]
        [Route("{userid}/{groupid}")]
        public ResponseModel Remove_Member(string groupid, string userid)
        {
            ResponseModel response = new ResponseModel();

            SqlCommand cmd = new SqlCommand("Remove_Group_Member", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@senderid", User.Identity.Name);
            cmd.Parameters.AddWithValue("@userid", userid);
            cmd.Parameters.AddWithValue("@groupid", groupid);

            var returnValue = cmd.Parameters.Add("@returnValue", SqlDbType.Int);
            returnValue.Direction = ParameterDirection.ReturnValue;

            con.Open();

            cmd.ExecuteNonQuery();
            int i = (int)returnValue.Value;

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
        /// Restore Ignored fella into a chatroom
        /// </summary>
        /// <param name="UserID">ID of the user sending the request</param>
        /// <param name="RecipientID">ID of the user to be restored</param>
        /// <param name="Base64IPAddress">Base64 IP address of user sending the request</param>
        /// <returns>Response object</returns>
        [HttpPost]
        [Route("{UserID}/{RecipientID}/{Base64IPAddress}/unignore")]
        public ResponseModel Unignore_Fella(string UserID, string RecipientID, string Base64IPAddress)
        {
            ResponseModel response = new ResponseModel();

            var ByteCode = Convert.FromBase64String(Base64IPAddress);
            string IPAddress = Encoding.UTF8.GetString(ByteCode);

            SqlCommand cmd = new SqlCommand("UnIgnore_fella", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserID", UserID);
            cmd.Parameters.AddWithValue("@IPAddress", IPAddress);
            cmd.Parameters.AddWithValue("@RecipientID", RecipientID);

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
        /// Gets all chatRooms associated to a user
        /// </summary>
        /// <param name="UserID">The ID of the user sending the request</param>
        /// <param name="Base64IPAddress">The IP address of the user sending the request</param>
        /// <returns>Response object</returns>
        [HttpGet]
        public ResponseModel GetUserChatRooms()
        {
            ResponseModel response = new ResponseModel();
            List<object> chatRooms = new List<object>();

            SqlCommand cmd = new SqlCommand("GetUserChatrooms", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserID", User.Identity.Name);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);

            int i = dt.Rows.Count;

            if (i > 0)
            {
                for (int j = 0; j < i; j++)
                {
                    ChatRoomModel chatRoom = new ChatRoomModel();

                    string imgSrc = String.Empty;

                    if (dt.Rows[j]["ChatRoomPicture"].ToString() != null && dt.Rows[j]["ChatRoomPicture"].ToString() != "")
                    {
                        var base64 = Convert.ToBase64String((byte[])dt.Rows[j]["ChatRoomPicture"]);
                        imgSrc = String.Format("data:image/png;base64, {0}", base64);
                    }

                    chatRoom.ChatRoomID = dt.Rows[j]["ChatRoomID"].ToString();
                    chatRoom.ChatRoomName = dt.Rows[j]["ChatRoomName"].ToString();
                    chatRoom.ProfilePicture = imgSrc;
                    chatRoom.Type = dt.Rows[j]["ChatRoomType"].ToString();
                    chatRoom.IsOnline = Convert.ToBoolean(dt.Rows[j]["IsOnline"]);
                    chatRoom.LastSeen = Convert.ToDateTime(dt.Rows[j]["LastSeen"]);
                    chatRoom.Description = dt.Rows[j]["_Description"].ToString();

                    chatRooms.Add(chatRoom);
                }

                int success = (int)ResponseCodes.Successfull;
                response.ResponseCode = success;
                response.ResponseMessage = ResponseCodes.Successfull.ToString();
                response.Data = chatRooms;
            }
            else
            {
                int noChatRoom = (int)ResponseCodes.NoChatRoom;
                response.ResponseCode = noChatRoom;
                response.ResponseMessage = ResponseCodes.NoChatRoom.ToString();
            }

            return response;
        }

        /// <summary>
        /// Get a chatroom related to a user using it's ID
        /// </summary>
        /// <param name="Base64ChatRoomID">The ID of the Chatroom to be returned</param>
        /// <param name="UserID">The ID of the user sending the request</param>
        /// <param name="Base64IPAddress">The IP address of the user sending the request</param>
        /// <returns>Response object</returns>
        [HttpGet]
        [Route("{Base64ChatRoomID}/chatroom")]
        public ResponseModel GetChatRoom(string Base64ChatRoomID)
        {
            //DECLARE INITIAL VARIABLES AND OBJECTS
            ResponseModel response = new ResponseModel();
            List<object> chatRooms = new List<object>();
            List<ChatsModel> chats = new List<ChatsModel>();
            List<UserModel> members = new List<UserModel>();
            ChatsController chatsController = new ChatsController();

            con.Open();

            var ChatroomByteCode = Convert.FromBase64String(Base64ChatRoomID);
            string ChatRoomID = Encoding.UTF8.GetString(ChatroomByteCode);
                    
            //GET ALL CHATROOM MEMBERS
            SqlCommand cmdChatRoomMembers = new SqlCommand("GetChatRoomMembers", con);
            cmdChatRoomMembers.CommandType = CommandType.StoredProcedure;
            cmdChatRoomMembers.Parameters.AddWithValue("@ChatRoomID", ChatRoomID);
            SqlDataAdapter sdaChatRoomMembers = new SqlDataAdapter(cmdChatRoomMembers);
            DataTable dtChatRoomMembers = new DataTable();
            sdaChatRoomMembers.Fill(dtChatRoomMembers);

            //GET A SPECIFIC CHATROOM
            SqlCommand cmd = new SqlCommand("GetChatRoom", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserID", User.Identity.Name);
            cmd.Parameters.AddWithValue("@ChatRoomID", ChatRoomID);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);

            int i = dt.Rows.Count;

            //IF CHATROOM EXISTS
            if (i > 0)
            {
                //MAP OBJECTS FROM DATABASE INTO CHATROOM OBJECT
                ChatRoomModel chatRoom = new ChatRoomModel();

                string imgSrc = String.Empty;

                if (dt.Rows[0]["ChatRoomPicture"].ToString() != null && dt.Rows[0]["ChatRoomPicture"].ToString() != "")
                {
                    var base64 = Convert.ToBase64String((byte[])dt.Rows[0]["ChatRoomPicture"]);
                    imgSrc = String.Format("data:image/png;base64, {0}", base64);
                }

                chatRoom.ChatRoomID = dt.Rows[0]["ChatRoomID"].ToString();
                chatRoom.ChatRoomName = dt.Rows[0]["ChatRoomName"].ToString();
                chatRoom.ProfilePicture = imgSrc;
                chatRoom.Type = dt.Rows[0]["ChatRoomType"].ToString();
                chatRoom.IsOnline = Convert.ToBoolean(dt.Rows[0]["IsOnline"]);
                chatRoom.LastSeen = Convert.ToDateTime(dt.Rows[0]["LastSeen"]);
                chatRoom.Description = dt.Rows[0]["_Description"].ToString();
                              
                //GET ALL CHATROOM MEMBERS FROM DATABASE AND MAP INTO MEMBER ARRAY
                for (int chm = 0; chm < dtChatRoomMembers.Rows.Count; chm++)
                {
                    UserModel user = new UserModel();

                    string memberBytes = dtChatRoomMembers.Rows[chm]["ProfilePicture"].ToString(), memberImage = String.Empty;

                    if (memberBytes != null && memberBytes != "")
                    {
                        var base64 = Convert.ToBase64String((byte[])dtChatRoomMembers.Rows[chm]["ProfilePicture"]);
                        memberImage = String.Format("data:image/png;base64, {0}", base64);
                    }

                    user.UserID = dtChatRoomMembers.Rows[chm]["MemberId"].ToString();
                    user.UserName = dtChatRoomMembers.Rows[chm]["UserName"].ToString();
                    user.Email = dtChatRoomMembers.Rows[chm]["Email"].ToString();
                    user.Description = dtChatRoomMembers.Rows[chm]["Description"].ToString();
                    user.ProfilePicture = memberImage;

                    members.Add(user);
                }

                chats = chatsController.GetChatRoomChats(Base64ChatRoomID);
                chatRoom.Chats = chats;
                chatRoom.Members = members;

                chatRooms.Add(chatRoom);

                int success = (int)ResponseCodes.Successfull;
                response.ResponseCode = success;
                response.ResponseMessage = ResponseCodes.Successfull.ToString();
                response.Data = chatRooms;
            }
            else
            {
                int noChatRoom = (int)ResponseCodes.NoChatRoom;
                response.ResponseCode = noChatRoom;
                response.ResponseMessage = ResponseCodes.NoChatRoom.ToString();
            }

            con.Close();

            return response;
        }

        /*  [HttpGet]
          [Route("{chatroomid}/profile")]
          public ResponseModel GetChatroom(string chatroomid)
          {
              ResponseModel response = new ResponseModel();
              ChatRoomProfileModel chatroom = new ChatRoomProfileModel();
              List<object> list = new List<object>();
              List<ChatsModel> chats = new List<ChatsModel>();
              ChatsController chatsController = new ChatsController();
              byte[] toEncodeChatroomIDAsBytes = ASCIIEncoding.ASCII.GetBytes(chatroomid);
              string Base64ChatRoomID = Convert.ToBase64String(toEncodeChatroomIDAsBytes);

              //GET'S CHATROOM DETAILS
              string query = "SELECT * FROM ChatRoom WHERE ChatRoomID=@chatroomid";
              SqlCommand cmd = new SqlCommand(query, con);
              cmd.Parameters.AddWithValue("@chatroomid", chatroomid);
              con.Open();
              SqlDataReader read = cmd.ExecuteReader();

              //GET ALL CHATROOM MEMBERS
              SqlCommand cmdChatRoomMembers = new SqlCommand("GetChatRoomMembers", con);
              cmdChatRoomMembers.CommandType = CommandType.StoredProcedure;
              cmdChatRoomMembers.Parameters.AddWithValue("@ChatRoomID", chatroomid);
              SqlDataAdapter sdaChatRoomMembers = new SqlDataAdapter(cmdChatRoomMembers);
              DataTable dtChatRoomMembers = new DataTable();

              //GET'S CHATROOM CHATS
              chats = chatsController.GetChatRoomChats(Base64ChatRoomID);

              if (read.HasRows && read.Read())
              {
                  var imgSrc = String.Empty;
                  if (read["ChatRoomPicture"].ToString() != null && read["ChatRoomPicture"].ToString() != "")
                  {
                      var base64 = Convert.ToBase64String((byte[])read["ChatRoomPicture"]);
                      imgSrc = String.Format("data:image/png;base64, {0}", base64);
                  }

                  chatroom.ChatRoomID = read["ChatRoomID"].ToString();
                  chatroom.ChatRoomName = read["ChatRoomName"].ToString();
                  chatroom.Type = read["ChatRoomType"].ToString();
                  chatroom.ChatRoom_Owner = read["ChatRoom_Owner"].ToString();

                  read.Close();

                  sdaChatRoomMembers.Fill(dtChatRoomMembers);

                  chatroom.MembersCount = dtChatRoomMembers.Rows.Count;
                  chatroom.ChatsCount = chats.Count;
                  chatroom.ProfilePicture = imgSrc;

                  list.Add(chatroom);

                  response.ResponseCode = (int)ResponseCodes.Successfull;
                  response.ResponseMessage = ResponseCodes.Successfull.ToString();
                  response.Data = list;
              }
              else
              {
                  response.ResponseCode = (int)ResponseCodes.NotFound;
                  response.ResponseMessage = ResponseCodes.NotFound.ToString();
              }

              con.Close();

              return response;
          } */

        /// <summary>
        /// Get a chatroom related to a user using it's ID (optimized)
        /// </summary>
        /// <param name="chatroomid">The ID of the Chatroom to be returned</param>
        /// <returns>Response object</returns>
        [HttpGet]
        [Route("{chatroomid}/profile")]
        public ResponseModel GetChatroom(string chatroomid)
        {
            ResponseModel response = new ResponseModel();
            ChatRoomModel chatroom = new ChatRoomModel();
            List<object> list = new List<object>();
            List<ChatsModel> chats = new List<ChatsModel>();
            ChatsController chatsController = new ChatsController();
            List<UserModel> members = new List<UserModel>();
            byte[] toEncodeChatroomIDAsBytes = ASCIIEncoding.ASCII.GetBytes(chatroomid);
            string Base64ChatRoomID = Convert.ToBase64String(toEncodeChatroomIDAsBytes);

            //GET'S CHATROOM DETAILS
            string query = "SELECT * FROM ChatRoom WHERE ChatRoomID=@chatroomid";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@chatroomid", chatroomid);
            con.Open();
            SqlDataReader read = cmd.ExecuteReader();

            //GET ALL CHATROOM MEMBERS
            SqlCommand cmdChatRoomMembers = new SqlCommand("GetChatRoomMembers", con);
            cmdChatRoomMembers.CommandType = CommandType.StoredProcedure;
            cmdChatRoomMembers.Parameters.AddWithValue("@ChatRoomID", chatroomid);
            SqlDataAdapter sdaChatRoomMembers = new SqlDataAdapter(cmdChatRoomMembers);
            DataTable dtChatRoomMembers = new DataTable();

            //GET'S CHATROOM CHATS
            chats = chatsController.GetChatRoomChats(Base64ChatRoomID);

            if (read.HasRows && read.Read())
            {
                var imgSrc = String.Empty;
                if (read["ChatRoomPicture"].ToString() != null && read["ChatRoomPicture"].ToString() != "")
                {
                    var base64 = Convert.ToBase64String((byte[])read["ChatRoomPicture"]);
                    imgSrc = String.Format("data:image/png;base64, {0}", base64);
                }

                chatroom.ChatRoomID = read["ChatRoomID"].ToString();
                chatroom.ChatRoomName = read["ChatRoomName"].ToString();
                chatroom.Type = read["ChatRoomType"].ToString();
                chatroom.ChatRoom_Owner = read["ChatRoom_Owner"].ToString();

                read.Close();

                sdaChatRoomMembers.Fill(dtChatRoomMembers);

                //GET ALL CHATROOM MEMBERS FROM DATABASE AND MAP INTO MEMBER ARRAY
                for (int chm = 0; chm < dtChatRoomMembers.Rows.Count; chm++)
                {
                    UserModel user = new UserModel();

                    string memberBytes = dtChatRoomMembers.Rows[chm]["ProfilePicture"].ToString(), memberImage = String.Empty;

                    if (memberBytes != null && memberBytes != "")
                    {
                        var base64 = Convert.ToBase64String((byte[])dtChatRoomMembers.Rows[chm]["ProfilePicture"]);
                        memberImage = String.Format("data:image/png;base64, {0}", base64);
                    }

                    user.UserID = dtChatRoomMembers.Rows[chm]["MemberId"].ToString();
                    user.UserName = dtChatRoomMembers.Rows[chm]["UserName"].ToString();
                    user.Email = dtChatRoomMembers.Rows[chm]["Email"].ToString();
                    user.Description = dtChatRoomMembers.Rows[chm]["Description"].ToString();
                    user.IsOnline = Convert.ToBoolean(dtChatRoomMembers.Rows[chm]["IsOnline"]);
                    user.ProfilePicture = memberImage;

                    members.Add(user);
                }

                chatroom.Members = members;
                chatroom.Chats = chats;
                chatroom.ProfilePicture = imgSrc;
                chatroom.LastSeen = DateTime.Now;
                chatroom.IsOnline = true;

                list.Add(chatroom);

                response.ResponseCode = (int)ResponseCodes.Successfull;
                response.ResponseMessage = ResponseCodes.Successfull.ToString();
                response.Data = list;
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
        /// Post's a new Chatroom in the database of type 'Group'
        /// </summary>
        /// <param name="group">Object in which JSON body will be mapped into </param>
        /// <returns>Response object</returns>
        [HttpPost]
        public ResponseModel Post(ChatRoomModel group)
        {
            ResponseModel response = new ResponseModel();

            SqlCommand cmd = new SqlCommand("Create_Chatroom_Group", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@chatroomid", group.ChatRoomID);
            cmd.Parameters.AddWithValue("@chatroomname", group.ChatRoomName);
            cmd.Parameters.AddWithValue("@chatroom_owner", User.Identity.Name);
            var returnValue = cmd.Parameters.Add("@returnValue", SqlDbType.Int);
            returnValue.Direction = ParameterDirection.ReturnValue;

            con.Open();

            cmd.ExecuteNonQuery();
            int i = (int)returnValue.Value;

            con.Close();

            if (i > 0)
            {
                var success = ResponseCodes.Successfull;
                response.ResponseCode = (int)success;
                response.ResponseMessage = ResponseCodes.Successfull.ToString();
            }
            else if (i < 0)
            {
                var fail = ResponseCodes.Unsuccessfull;
                response.ResponseCode = (int)fail;
                response.ResponseMessage = ResponseCodes.Unsuccessfull.ToString();
            }

            return response;
        }

        /// <summary>
        /// Updates the details of a chattroom
        /// </summary>
        /// <param name="group">Object in which JSON body will be mapped into </param>
        /// <param name="chatroomid">The ID of the chatroom to be updated</param>
        /// <returns>Response object</returns>
        [HttpPut]
        [Route("{chatroomid}/details")]
        public ResponseModel Update_ChatRoom_Details(string chatroomid,ChatRoomModel group)
        {
            ResponseModel response = new ResponseModel();

            SqlCommand cmd = new SqlCommand("Update_Chatroom_Details", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@chatroomid", chatroomid);
            cmd.Parameters.AddWithValue("@chatroomname", group.ChatRoomName);

            con.Open();

            int i = cmd.ExecuteNonQuery();

            con.Close();

            if (i > 0)
            {
                var success = ResponseCodes.Successfull;
                response.ResponseCode = (int)success;
                response.ResponseMessage = ResponseCodes.Successfull.ToString();
            }
            else if (i < 0)
            {
                var fail = ResponseCodes.Unsuccessfull;
                response.ResponseCode = (int)fail;
                response.ResponseMessage = ResponseCodes.Unsuccessfull.ToString();
            }

            return response;
        }

        // POST: api/Chats
        /// <summary>
        /// Updates chatroom picture in the database
        /// </summary>
        /// <param name="chatroomid">ID of the chatroom to update</param>
        ///<returns>Response object</returns>
        [HttpPut]
        [Route("{chatroomid}")]
        public async Task<ResponseModel> PostFile(string chatroomid)
        {
            ResponseModel response = new ResponseModel();
            bool res = false;

            var ctx = HttpContext.Current;
            var root = ctx.Server.MapPath("~/App_Data");
            var provider = new MultipartFileStreamProvider(root);

            try
            {
                await Request.Content.ReadAsMultipartAsync(provider);
                foreach (var file in provider.FileData)
                {
                    var name = file.Headers.ContentDisposition.FileName;
                    int size = (int)new FileInfo(file.LocalFileName).Length;
                    name = name.Trim('"');
                    var localFileName = file.LocalFileName;
                    res = SaveFile(localFileName, User.Identity.Name, chatroomid);
                }

                if (res)
                {
                    var success = ResponseCodes.Successfull;
                    response.ResponseCode = (int)success;
                    response.ResponseMessage = ResponseCodes.Successfull.ToString();
                }

                else
                {
                    var fail = ResponseCodes.Unsuccessfull;
                    response.ResponseCode = (int)fail;
                    response.ResponseMessage = ResponseCodes.Unsuccessfull.ToString();
                }
            }

            catch (Exception e)
            {
                var fail = ResponseCodes.Unsuccessfull;
                response.ResponseCode = (int)fail;
                response.ResponseMessage = e.ToString();//ResponseCodes.Unsuccessfull.ToString();
            }


            return response;
        }

        public bool SaveFile(string localFile, string chatroom_owner, string chatRoomID)
        {
            byte[] fileBytes;

            using (var fs = new FileStream(localFile, FileMode.Open, FileAccess.Read))
            {
                fileBytes = new byte[fs.Length];
                fs.Read(fileBytes, 0, Convert.ToInt32(fs.Length));
            }

            SqlCommand cmd = new SqlCommand("Update_Chatroom_Picture", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@chatroomid", chatRoomID);
            cmd.Parameters.AddWithValue("@chatroom_picture", fileBytes);
            cmd.Parameters.AddWithValue("@chatroom_owner", chatroom_owner);

            con.Open();
            int i = cmd.ExecuteNonQuery();
            con.Close();

            if (i > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
