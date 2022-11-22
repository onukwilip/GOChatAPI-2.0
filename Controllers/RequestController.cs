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

namespace GOChatAPI.Controllers
{
    /// <summary>
    /// Handles all API's responsible for sending, validating and recieving requests(Friend and chatroom requests)
    /// </summary>
    [Authorize]
    [RoutePrefix("api/requests")]
    public class RequestController : ApiController
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["GOChat"].ConnectionString);
        General general = new General();

        /// <summary>
        /// Creates a new request in the database
        /// </summary>
        /// <returns>Response object</returns>
        /// <param name="request">Maps the body into a request object</param>
        [HttpPost]
        public ResponseModel CreateRequest(RequestModel request)
        {
            ResponseModel response = new ResponseModel();

            SqlCommand cmd = new SqlCommand("CreateRequest", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@From_ID", request.From.UserID);
            cmd.Parameters.AddWithValue("@To_ID", request.To.UserID);
            cmd.Parameters.AddWithValue("@Message", request.Message);
            cmd.Parameters.AddWithValue("@From_Type", request.From_Type);
            cmd.Parameters.AddWithValue("@To_Type", request.To_Type);
            var returnValue = cmd.Parameters.AddWithValue("@returnVal", SqlDbType.Int);
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
            else
            {
                var fail = ResponseCodes.Unsuccessfull;
                response.ResponseCode = (int)fail;
                response.ResponseMessage = ResponseCodes.Unsuccessfull.ToString();
            }

            return response;
        }
                              
        /// <summary>
        /// Verifies if a user is allowed to send a request to another user or chatroom(group)
        /// </summary>
        /// <param name="UserID">The ID of the sender</param>
        /// <param name="Recipient">The ID of the recipient</param>
        /// <returns>Response object</returns>
        [HttpGet]
        [Route("verify/{UserID}/{Recipient}")]
        public ResponseModel VerifyRequestStatus(string UserID, string Recipient)
        {
            ResponseModel response = new ResponseModel();

            string checkIfRequestExists = "select count(*) from Requests where From_ID = @From_ID and To_ID = @To_ID";
            string checkIfChatRoomMemberexists = @"select
			count(*)
			from ChatRoomMembers
			where (MemberId=@From_ID and IdentityToRender=@To_ID) 
			or (MemberId=@To_ID and IdentityToRender=@From_ID)";

            string checkIfUserIsIgnored = "select count(*) from IgnoredFellas where MemberID = @From_ID and IdentityToRender = @To_ID";

            SqlCommand requestExistsCommand = new SqlCommand(checkIfRequestExists, con);
            requestExistsCommand.Parameters.AddWithValue("@From_ID", UserID);
            requestExistsCommand.Parameters.AddWithValue("@To_ID", Recipient);

            SqlCommand chatRoomMemberExistsCommand = new SqlCommand(checkIfChatRoomMemberexists, con);
            chatRoomMemberExistsCommand.Parameters.AddWithValue("@From_ID", UserID);
            chatRoomMemberExistsCommand.Parameters.AddWithValue("@To_ID", Recipient);

            SqlCommand userIgnoredCommand = new SqlCommand(checkIfUserIsIgnored, con);
            userIgnoredCommand.Parameters.AddWithValue("@From_ID", UserID);
            userIgnoredCommand.Parameters.AddWithValue("@To_ID", Recipient);

            con.Open();

            int requestCount = (int)requestExistsCommand.ExecuteScalar();
            int chatRoomCount = (int)chatRoomMemberExistsCommand.ExecuteScalar();
            int userIsIgnoredCount = (int)userIgnoredCommand.ExecuteScalar();

            con.Close();

            if (requestCount < 1 && chatRoomCount < 1)
            {
                response.ResponseCode = (int)ResponseCodes.RequestValid;
                response.ResponseMessage = ResponseCodes.RequestValid.ToString();
            }

            else if (requestCount > 0)
            {
                response.ResponseCode = (int)ResponseCodes.RequestExists;
                response.ResponseMessage = ResponseCodes.RequestExists.ToString();
            }

            else if (chatRoomCount > 0 && userIsIgnoredCount < 1)
            {
                response.ResponseCode = (int)ResponseCodes.ChatRoomMemberExists;
                response.ResponseMessage = ResponseCodes.ChatRoomMemberExists.ToString();
            }

            else if (chatRoomCount > 0 && userIsIgnoredCount > 0)
            {
                response.ResponseCode = (int)ResponseCodes.UserIsIgnored;
                response.ResponseMessage = ResponseCodes.UserIsIgnored.ToString();
            }

            /* SqlCommand cmd = new SqlCommand("ValidateRequest", con);
             cmd.CommandType = CommandType.StoredProcedure;
             cmd.Parameters.AddWithValue("@From_ID", UserID); 
             cmd.Parameters.AddWithValue("@To_ID", Recipient);

             con.Open();

             int i = cmd.ExecuteNonQuery();

             con.Close();

             if (i > 0)
             {
                 response.ResponseCode = (int)ResponseCodes.RequestValid;
                 response.ResponseMessage = ResponseCodes.RequestValid.ToString();
             }

             else if (i == 0)
             {
                 response.ResponseCode = (int)ResponseCodes.RequestExists;
                 response.ResponseMessage = ResponseCodes.RequestExists.ToString();
             }

             else if (i < 0)
             {
                 response.ResponseCode = (int)ResponseCodes.ChatRoomMemberExists;
                 response.ResponseMessage = ResponseCodes.ChatRoomMemberExists.ToString();
             } */

            return response;
        }
               
        //OPTIMIZED

        /// <summary>
        /// Gets all requests sent by a specific user
        /// </summary>
        /// <returns>Response object</returns>
        [HttpGet]
        [Route("sent")]
        public ResponseModel _GetSentRequests()
        {
            ResponseModel response = new ResponseModel();
            List<object> requests = new List<object>();

            string get = "GetRequestsSentByUser";
            SqlCommand cmd = new SqlCommand(get, con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserID", User.Identity.Name);
            cmd.Parameters.AddWithValue("@IPAddress", "");

            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);

            int i = dt.Rows.Count;

            if (i > 0)
            {
                for (int j = 0; j < i; j++)
                {
                    string From_Type = dt.Rows[j]["From_Type"].ToString();
                    string To_Type = dt.Rows[j]["To_Type"].ToString();

                    int ID = (int)dt.Rows[j]["id"];

                    if (From_Type == "User" && To_Type == "User")
                    {
                        SqlCommand cmdUser = new SqlCommand("GetUserRequests_User_User", con);
                        cmdUser.CommandType = CommandType.StoredProcedure;
                        cmdUser.Parameters.AddWithValue("@UserID", User.Identity.Name);
                        cmdUser.Parameters.AddWithValue("@IPAddress", "");
                        cmdUser.Parameters.AddWithValue("@ID", ID);

                        SqlDataAdapter sdaUser = new SqlDataAdapter(cmdUser);
                        DataTable dtUser = new DataTable();
                        sdaUser.Fill(dtUser);

                        int iUser = dtUser.Rows.Count;

                        if (iUser > 0)
                        {
                            for (int jUser = 0; jUser < iUser; jUser++)
                            {
                                RequestModel request = new RequestModel();
                                UserModel from = new UserModel();
                                UserModel to = new UserModel();
                                ValidateUser from_validate = new ValidateUser();
                                ValidateUser to_validate = new ValidateUser();
                                string fromSrc = String.Empty, toSrc = String.Empty;

                                //From Object
                                if (dtUser.Rows[jUser]["From_ProfilePicture"].ToString() != null && dtUser.Rows[jUser]["From_ProfilePicture"].ToString() != "")
                                {
                                    var base64 = Convert.ToBase64String((byte[])dtUser.Rows[jUser]["From_ProfilePicture"]);
                                    fromSrc = String.Format("data:image/png;base64, {0}", base64);
                                }

                                from.UserID = dtUser.Rows[jUser]["From_UserID"].ToString();
                                from.UserName = dtUser.Rows[jUser]["From_UserName"].ToString();
                                from.Email = dtUser.Rows[jUser]["From_Email"].ToString();
                                from.ProfilePicture = fromSrc;
                                from.IsOnline = Convert.ToBoolean(dtUser.Rows[jUser]["From_IsOnline"]);
                                from.Description = dtUser.Rows[jUser]["From_Description"].ToString();
                                from.LastSeen = Convert.ToDateTime(dtUser.Rows[jUser]["From_Lastseen"]);
                                from_validate.UserExists = true;

                                if (Convert.ToInt32(dtUser.Rows[jUser]["From_Authenticated"]) > 0)
                                {
                                    from_validate.IsAuthenticated = true;
                                }
                                else
                                {
                                    from_validate.IsAuthenticated = false;
                                }

                                from.Response = from_validate;

                                //To Object
                                if (dtUser.Rows[jUser]["To_ProfilePicture"].ToString() != null && dtUser.Rows[jUser]["To_ProfilePicture"].ToString() != "")
                                {
                                    var base64 = Convert.ToBase64String((byte[])dtUser.Rows[jUser]["To_ProfilePicture"]);
                                    toSrc = String.Format("data:image/png;base64, {0}", base64);
                                }

                                to.UserID = dtUser.Rows[jUser]["To_UserID"].ToString();
                                to.UserName = dtUser.Rows[jUser]["To_UserName"].ToString();
                                to.Email = dtUser.Rows[jUser]["To_Email"].ToString();
                                to.ProfilePicture = toSrc;
                                to.IsOnline = Convert.ToBoolean(dtUser.Rows[jUser]["To_IsOnline"]);
                                to.Description = dtUser.Rows[jUser]["To_Description"].ToString();
                                to.LastSeen = Convert.ToDateTime(dtUser.Rows[jUser]["To_Lastseen"]);
                                to_validate.UserExists = true;

                                if (Convert.ToInt32(dtUser.Rows[jUser]["To_Authenticated"]) > 0)
                                {
                                    to_validate.IsAuthenticated = true;
                                }
                                else
                                {
                                    to_validate.IsAuthenticated = false;
                                }

                                to.Response = to_validate;

                                request.From = from;
                                request.To = to;
                                request.Message = dtUser.Rows[jUser]["_Message"].ToString();
                                request.From_Type = dtUser.Rows[jUser]["From_Type"].ToString();
                                request.To_Type = dtUser.Rows[jUser]["To_Type"].ToString();
                                request.ID = (int)dtUser.Rows[jUser]["id"];

                                requests.Add(request);
                            }
                        }
                    }

                    else if (From_Type == "User" && To_Type == "Group")
                    {
                        SqlCommand cmdUser = new SqlCommand("GetUserRequests_User_Group", con);
                        cmdUser.CommandType = CommandType.StoredProcedure;
                        cmdUser.Parameters.AddWithValue("@UserID", User.Identity.Name);
                        cmdUser.Parameters.AddWithValue("@IPAddress", "");
                        cmdUser.Parameters.AddWithValue("@ID", ID);

                        SqlDataAdapter sdaUser = new SqlDataAdapter(cmdUser);
                        DataTable dtUser = new DataTable();
                        sdaUser.Fill(dtUser);

                        int iUser = dtUser.Rows.Count;

                        if (iUser > 0)
                        {
                            for (int jUser = 0; jUser < iUser; jUser++)
                            {
                                RequestModel request = new RequestModel();
                                UserModel from = new UserModel();
                                UserModel to = new UserModel();
                                ValidateUser from_validate = new ValidateUser();
                                ValidateUser to_validate = new ValidateUser();
                                string fromSrc = String.Empty, toSrc = String.Empty;

                                //From Object
                                if (dtUser.Rows[jUser]["From_ProfilePicture"].ToString() != null && dtUser.Rows[jUser]["From_ProfilePicture"].ToString() != "")
                                {
                                    var base64 = Convert.ToBase64String((byte[])dtUser.Rows[jUser]["From_ProfilePicture"]);
                                    fromSrc = String.Format("data:image/png;base64, {0}", base64);
                                }

                                from.UserID = dtUser.Rows[jUser]["From_UserID"].ToString();
                                from.UserName = dtUser.Rows[jUser]["From_UserName"].ToString();
                                from.Email = dtUser.Rows[jUser]["From_Email"].ToString();
                                from.ProfilePicture = fromSrc;
                                from.IsOnline = Convert.ToBoolean(dtUser.Rows[jUser]["From_IsOnline"]);
                                from.Description = dtUser.Rows[jUser]["From_Description"].ToString();
                                from.LastSeen = Convert.ToDateTime(dtUser.Rows[jUser]["From_Lastseen"]);
                                from_validate.UserExists = true;

                                if (Convert.ToInt32(dtUser.Rows[jUser]["From_Authenticated"]) > 0)
                                {
                                    from_validate.IsAuthenticated = true;
                                }
                                else
                                {
                                    from_validate.IsAuthenticated = false;
                                }

                                from.Response = from_validate;

                                //To Object
                                if (dtUser.Rows[jUser]["To_ProfilePicture"].ToString() != null && dtUser.Rows[jUser]["To_ProfilePicture"].ToString() != "")
                                {
                                    var base64 = Convert.ToBase64String((byte[])dtUser.Rows[jUser]["To_ProfilePicture"]);
                                    toSrc = String.Format("data:image/png;base64, {0}", base64);
                                }

                                to.UserID = dtUser.Rows[jUser]["To_UserID"].ToString();
                                to.UserName = dtUser.Rows[jUser]["To_UserName"].ToString();
                                to.ProfilePicture = toSrc;
                                to.IsOnline = true;

                                request.From = from;
                                request.To = to;
                                request.Message = dtUser.Rows[jUser]["_Message"].ToString();
                                request.From_Type = dtUser.Rows[jUser]["From_Type"].ToString();
                                request.To_Type = dtUser.Rows[jUser]["To_Type"].ToString();
                                request.ID = (int)dtUser.Rows[jUser]["id"];

                                requests.Add(request);
                            }
                        }
                    }

                    else if (From_Type == "Group" && To_Type == "User")
                    {
                        SqlCommand cmdUser = new SqlCommand("GetUserRequests_Group_User", con);
                        cmdUser.CommandType = CommandType.StoredProcedure;
                        cmdUser.Parameters.AddWithValue("@ID", ID);

                        SqlDataAdapter sdaUser = new SqlDataAdapter(cmdUser);
                        DataTable dtUser = new DataTable();
                        sdaUser.Fill(dtUser);

                        int iUser = dtUser.Rows.Count;

                        if (iUser > 0)
                        {
                            for (int jUser = 0; jUser < iUser; jUser++)
                            {
                                RequestModel request = new RequestModel();
                                UserModel from = new UserModel();
                                UserModel to = new UserModel();
                                ValidateUser from_validate = new ValidateUser();
                                ValidateUser to_validate = new ValidateUser();
                                string fromSrc = String.Empty, toSrc = String.Empty;

                                //From Object
                                if (dtUser.Rows[jUser]["From_ProfilePicture"].ToString() != null && dtUser.Rows[jUser]["From_ProfilePicture"].ToString() != "")
                                {
                                    var base64 = Convert.ToBase64String((byte[])dtUser.Rows[jUser]["From_ProfilePicture"]);
                                    fromSrc = String.Format("data:image/png;base64, {0}", base64);
                                }

                                from.UserID = dtUser.Rows[jUser]["From_UserID"].ToString();
                                from.UserName = dtUser.Rows[jUser]["From_UserName"].ToString();
                                from.ProfilePicture = fromSrc;
                                from.IsOnline = true;
                                from_validate.UserExists = true;

                                //To Object
                                if (dtUser.Rows[jUser]["To_ProfilePicture"].ToString() != null && dtUser.Rows[jUser]["To_ProfilePicture"].ToString() != "")
                                {
                                    var base64 = Convert.ToBase64String((byte[])dtUser.Rows[jUser]["To_ProfilePicture"]);
                                    toSrc = String.Format("data:image/png;base64, {0}", base64);
                                }

                                to.UserID = dtUser.Rows[jUser]["To_UserID"].ToString();
                                to.UserName = dtUser.Rows[jUser]["To_UserName"].ToString();
                                to.Email = dtUser.Rows[jUser]["To_Email"].ToString();
                                to.ProfilePicture = toSrc;
                                to.IsOnline = Convert.ToBoolean(dtUser.Rows[jUser]["To_IsOnline"]);
                                to.Description = dtUser.Rows[jUser]["To_Description"].ToString();
                                to.LastSeen = Convert.ToDateTime(dtUser.Rows[jUser]["To_Lastseen"]);
                                to_validate.UserExists = true;

                                if (Convert.ToInt32(dtUser.Rows[jUser]["To_Authenticated"]) > 0)
                                {
                                    to_validate.IsAuthenticated = true;
                                }
                                else
                                {
                                    to_validate.IsAuthenticated = false;
                                }

                                to.Response = to_validate;

                                request.From = from;
                                request.To = to;
                                request.Message = dtUser.Rows[jUser]["_Message"].ToString();
                                request.From_Type = dtUser.Rows[jUser]["From_Type"].ToString();
                                request.To_Type = dtUser.Rows[jUser]["To_Type"].ToString();
                                request.ID = (int)dtUser.Rows[jUser]["id"];

                                requests.Add(request);
                            }
                        }
                    }
                }

                int success = (int)ResponseCodes.Successfull;
                response.ResponseCode = success;
                response.ResponseMessage = ResponseCodes.Successfull.ToString();
                response.Data = requests;
            }

            else
            {
                int noRequests = (int)ResponseCodes.NoRequests;
                response.ResponseCode = noRequests;
                response.ResponseMessage = ResponseCodes.NoRequests.ToString();
            }

            return response;
        }

        /// <summary>
        /// Gets all requests sent to a specific user
        /// </summary>
        /// <returns>Response object</returns>
        [HttpGet]
        [Route("recieved")]
        public ResponseModel _GetRecievedRequests()
        {
            ResponseModel response = new ResponseModel();
            List<object> requests = new List<object>();

            string get = "GetRequestsRecievedByUser";
            SqlCommand cmd = new SqlCommand(get, con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserID", User.Identity.Name);
            cmd.Parameters.AddWithValue("@IPAddress", "");

            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);

            int i = dt.Rows.Count;

            if (i > 0)
            {
                for (int j = 0; j < i; j++)
                {
                    string From_Type = dt.Rows[j]["From_Type"].ToString();
                    string To_Type = dt.Rows[j]["To_Type"].ToString();
                    int ID = (int)dt.Rows[j]["id"];

                    if (From_Type == "User" && To_Type == "User")
                    {
                        SqlCommand cmdUser = new SqlCommand("GetUserRequests_User_User", con);
                        cmdUser.CommandType = CommandType.StoredProcedure;
                        cmdUser.Parameters.AddWithValue("@UserID", User.Identity.Name);
                        cmdUser.Parameters.AddWithValue("@IPAddress", "");
                        cmdUser.Parameters.AddWithValue("@ID", ID);

                        SqlDataAdapter sdaUser = new SqlDataAdapter(cmdUser);
                        DataTable dtUser = new DataTable();
                        sdaUser.Fill(dtUser);

                        int iUser = dtUser.Rows.Count;

                        if (iUser > 0)
                        {
                            for (int jUser = 0; jUser < iUser; jUser++)
                            {
                                RequestModel request = new RequestModel();
                                UserModel from = new UserModel();
                                UserModel to = new UserModel();
                                ValidateUser from_validate = new ValidateUser();
                                ValidateUser to_validate = new ValidateUser();
                                string fromSrc = String.Empty, toSrc = String.Empty;

                                //From Object
                                if (dtUser.Rows[jUser]["From_ProfilePicture"].ToString() != null && dtUser.Rows[jUser]["From_ProfilePicture"].ToString() != "")
                                {
                                    var base64 = Convert.ToBase64String((byte[])dtUser.Rows[jUser]["From_ProfilePicture"]);
                                    fromSrc = String.Format("data:image/png;base64, {0}", base64);
                                }

                                from.UserID = dtUser.Rows[jUser]["From_UserID"].ToString();
                                from.UserName = dtUser.Rows[jUser]["From_UserName"].ToString();
                                from.Email = dtUser.Rows[jUser]["From_Email"].ToString();
                                from.ProfilePicture = fromSrc;
                                from.IsOnline = Convert.ToBoolean(dtUser.Rows[jUser]["From_IsOnline"]);
                                from.Description = dtUser.Rows[jUser]["From_Description"].ToString();
                                from.LastSeen = Convert.ToDateTime(dtUser.Rows[jUser]["From_Lastseen"]);
                                from_validate.UserExists = true;

                                if (Convert.ToInt32(dtUser.Rows[jUser]["From_Authenticated"]) > 0)
                                {
                                    from_validate.IsAuthenticated = true;
                                }
                                else
                                {
                                    from_validate.IsAuthenticated = false;
                                }

                                from.Response = from_validate;

                                //To Object
                                if (dtUser.Rows[jUser]["To_ProfilePicture"].ToString() != null && dtUser.Rows[jUser]["To_ProfilePicture"].ToString() != "")
                                {
                                    var base64 = Convert.ToBase64String((byte[])dtUser.Rows[jUser]["To_ProfilePicture"]);
                                    toSrc = String.Format("data:image/png;base64, {0}", base64);
                                }

                                to.UserID = dtUser.Rows[jUser]["To_UserID"].ToString();
                                to.UserName = dtUser.Rows[jUser]["To_UserName"].ToString();
                                to.Email = dtUser.Rows[jUser]["To_Email"].ToString();
                                to.ProfilePicture = toSrc;
                                to.IsOnline = Convert.ToBoolean(dtUser.Rows[jUser]["To_IsOnline"]);
                                to.Description = dtUser.Rows[jUser]["To_Description"].ToString();
                                to.LastSeen = Convert.ToDateTime(dtUser.Rows[jUser]["To_Lastseen"]);
                                to_validate.UserExists = true;

                                if (Convert.ToInt32(dtUser.Rows[jUser]["To_Authenticated"]) > 0)
                                {
                                    to_validate.IsAuthenticated = true;
                                }
                                else
                                {
                                    to_validate.IsAuthenticated = false;
                                }

                                to.Response = to_validate;

                                request.From = from;
                                request.To = to;
                                request.Message = dtUser.Rows[jUser]["_Message"].ToString();
                                request.From_Type = dtUser.Rows[jUser]["From_Type"].ToString();
                                request.To_Type = dtUser.Rows[jUser]["To_Type"].ToString();
                                request.ID = (int)dtUser.Rows[jUser]["id"];

                                requests.Add(request);
                            }
                        }
                    }

                    else if (From_Type == "User" && To_Type == "Group")
                    {
                        SqlCommand cmdUser = new SqlCommand("GetUserRequests_User_Group", con);
                        cmdUser.CommandType = CommandType.StoredProcedure;
                        cmdUser.Parameters.AddWithValue("@UserID", User.Identity.Name);
                        cmdUser.Parameters.AddWithValue("@IPAddress", "");
                        cmdUser.Parameters.AddWithValue("@ID", ID);

                        SqlDataAdapter sdaUser = new SqlDataAdapter(cmdUser);
                        DataTable dtUser = new DataTable();
                        sdaUser.Fill(dtUser);

                        int iUser = dtUser.Rows.Count;

                        if (iUser > 0)
                        {
                            for (int jUser = 0; jUser < iUser; jUser++)
                            {
                                RequestModel request = new RequestModel();
                                UserModel from = new UserModel();
                                UserModel to = new UserModel();
                                ValidateUser from_validate = new ValidateUser();
                                ValidateUser to_validate = new ValidateUser();
                                string fromSrc = String.Empty, toSrc = String.Empty;

                                //From Object
                                if (dtUser.Rows[jUser]["From_ProfilePicture"].ToString() != null && dtUser.Rows[jUser]["From_ProfilePicture"].ToString() != "")
                                {
                                    var base64 = Convert.ToBase64String((byte[])dtUser.Rows[jUser]["From_ProfilePicture"]);
                                    fromSrc = String.Format("data:image/png;base64, {0}", base64);
                                }

                                from.UserID = dtUser.Rows[jUser]["From_UserID"].ToString();
                                from.UserName = dtUser.Rows[jUser]["From_UserName"].ToString();
                                from.Email = dtUser.Rows[jUser]["From_Email"].ToString();
                                from.ProfilePicture = fromSrc;
                                from.IsOnline = Convert.ToBoolean(dtUser.Rows[jUser]["From_IsOnline"]);
                                from.Description = dtUser.Rows[jUser]["From_Description"].ToString();
                                from.LastSeen = Convert.ToDateTime(dtUser.Rows[jUser]["From_Lastseen"]);
                                from_validate.UserExists = true;

                                if (Convert.ToInt32(dtUser.Rows[jUser]["From_Authenticated"]) > 0)
                                {
                                    from_validate.IsAuthenticated = true;
                                }
                                else
                                {
                                    from_validate.IsAuthenticated = false;
                                }

                                from.Response = from_validate;

                                //To Object
                                if (dtUser.Rows[jUser]["To_ProfilePicture"].ToString() != null && dtUser.Rows[jUser]["To_ProfilePicture"].ToString() != "")
                                {
                                    var base64 = Convert.ToBase64String((byte[])dtUser.Rows[jUser]["To_ProfilePicture"]);
                                    toSrc = String.Format("data:image/png;base64, {0}", base64);
                                }

                                to.UserID = dtUser.Rows[jUser]["To_UserID"].ToString();
                                to.UserName = dtUser.Rows[jUser]["To_UserName"].ToString();
                                to.ProfilePicture = toSrc;
                                to.IsOnline = true;

                                request.From = from;
                                request.To = to;
                                request.Message = dtUser.Rows[jUser]["_Message"].ToString();
                                request.From_Type = dtUser.Rows[jUser]["From_Type"].ToString();
                                request.To_Type = dtUser.Rows[jUser]["To_Type"].ToString();
                                request.ID = (int)dtUser.Rows[jUser]["id"];

                                requests.Add(request);
                            }
                        }
                    }

                    else if (From_Type == "Group" && To_Type == "User")
                    {
                        SqlCommand cmdUser = new SqlCommand("GetUserRequests_Group_User", con);
                        cmdUser.CommandType = CommandType.StoredProcedure;
                        cmdUser.Parameters.AddWithValue("@ID", ID);

                        SqlDataAdapter sdaUser = new SqlDataAdapter(cmdUser);
                        DataTable dtUser = new DataTable();
                        sdaUser.Fill(dtUser);

                        int iUser = dtUser.Rows.Count;

                        if (iUser > 0)
                        {
                            for (int jUser = 0; jUser < iUser; jUser++)
                            {
                                RequestModel request = new RequestModel();
                                UserModel from = new UserModel();
                                UserModel to = new UserModel();
                                ValidateUser from_validate = new ValidateUser();
                                ValidateUser to_validate = new ValidateUser();
                                string fromSrc = String.Empty, toSrc = String.Empty;

                                //From Object
                                if (dtUser.Rows[jUser]["From_ProfilePicture"].ToString() != null && dtUser.Rows[jUser]["From_ProfilePicture"].ToString() != "")
                                {
                                    var base64 = Convert.ToBase64String((byte[])dtUser.Rows[jUser]["From_ProfilePicture"]);
                                    fromSrc = String.Format("data:image/png;base64, {0}", base64);
                                }

                                from.UserID = dtUser.Rows[jUser]["From_UserID"].ToString();
                                from.UserName = dtUser.Rows[jUser]["From_UserName"].ToString();
                                from.ProfilePicture = fromSrc;
                                from.IsOnline = true;
                                from_validate.UserExists = true;

                                //To Object
                                if (dtUser.Rows[jUser]["To_ProfilePicture"].ToString() != null && dtUser.Rows[jUser]["To_ProfilePicture"].ToString() != "")
                                {
                                    var base64 = Convert.ToBase64String((byte[])dtUser.Rows[jUser]["To_ProfilePicture"]);
                                    toSrc = String.Format("data:image/png;base64, {0}", base64);
                                }

                                to.UserID = dtUser.Rows[jUser]["To_UserID"].ToString();
                                to.UserName = dtUser.Rows[jUser]["To_UserName"].ToString();
                                to.Email = dtUser.Rows[jUser]["To_Email"].ToString();
                                to.ProfilePicture = toSrc;
                                to.IsOnline = Convert.ToBoolean(dtUser.Rows[jUser]["To_IsOnline"]);
                                to.Description = dtUser.Rows[jUser]["To_Description"].ToString();
                                to.LastSeen = Convert.ToDateTime(dtUser.Rows[jUser]["To_Lastseen"]);
                                to_validate.UserExists = true;

                                if (Convert.ToInt32(dtUser.Rows[jUser]["To_Authenticated"]) > 0)
                                {
                                    to_validate.IsAuthenticated = true;
                                }
                                else
                                {
                                    to_validate.IsAuthenticated = false;
                                }

                                to.Response = to_validate;

                                request.From = from;
                                request.To = to;
                                request.Message = dtUser.Rows[jUser]["_Message"].ToString();
                                request.From_Type = dtUser.Rows[jUser]["From_Type"].ToString();
                                request.To_Type = dtUser.Rows[jUser]["To_Type"].ToString();
                                request.ID = (int)dtUser.Rows[jUser]["id"];

                                requests.Add(request);
                            }
                        }
                    }
                }

                int success = (int)ResponseCodes.Successfull;
                response.ResponseCode = success;
                response.ResponseMessage = ResponseCodes.Successfull.ToString();
                response.Data = requests;
            }

            else
            {
                int noRequests = (int)ResponseCodes.NoRequests;
                response.ResponseCode = noRequests;
                response.ResponseMessage = ResponseCodes.NoRequests.ToString();
            }

            return response;
        }

        /// <summary>
        /// Gets all requests sent by a specific group
        /// </summary>
        /// <returns>Response object</returns>
        [HttpGet]
        [Route("sent/{groupid}")]
        public ResponseModel _GetGroupSentRequests(string groupid)
        {
            ResponseModel response = new ResponseModel();
            List<object> requests = new List<object>();

            string get = "GetRequestsSentByUser";
            SqlCommand cmd = new SqlCommand(get, con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserID", groupid);
            cmd.Parameters.AddWithValue("@IPAddress", "");

            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);

            int i = dt.Rows.Count;

            if (i > 0)
            {
                for (int j = 0; j < i; j++)
                {
                    string From_Type = dt.Rows[j]["From_Type"].ToString();
                    string To_Type = dt.Rows[j]["To_Type"].ToString();

                    int ID = (int)dt.Rows[j]["id"];

                    if (From_Type == "User" && To_Type == "User")
                    {
                        SqlCommand cmdUser = new SqlCommand("GetUserRequests_User_User", con);
                        cmdUser.CommandType = CommandType.StoredProcedure;
                        cmdUser.Parameters.AddWithValue("@UserID", groupid);
                        cmdUser.Parameters.AddWithValue("@IPAddress", "");
                        cmdUser.Parameters.AddWithValue("@ID", ID);

                        SqlDataAdapter sdaUser = new SqlDataAdapter(cmdUser);
                        DataTable dtUser = new DataTable();
                        sdaUser.Fill(dtUser);

                        int iUser = dtUser.Rows.Count;

                        if (iUser > 0)
                        {
                            for (int jUser = 0; jUser < iUser; jUser++)
                            {
                                RequestModel request = new RequestModel();
                                UserModel from = new UserModel();
                                UserModel to = new UserModel();
                                ValidateUser from_validate = new ValidateUser();
                                ValidateUser to_validate = new ValidateUser();
                                string fromSrc = String.Empty, toSrc = String.Empty;

                                //From Object
                                if (dtUser.Rows[jUser]["From_ProfilePicture"].ToString() != null && dtUser.Rows[jUser]["From_ProfilePicture"].ToString() != "")
                                {
                                    var base64 = Convert.ToBase64String((byte[])dtUser.Rows[jUser]["From_ProfilePicture"]);
                                    fromSrc = String.Format("data:image/png;base64, {0}", base64);
                                }

                                from.UserID = dtUser.Rows[jUser]["From_UserID"].ToString();
                                from.UserName = dtUser.Rows[jUser]["From_UserName"].ToString();
                                from.Email = dtUser.Rows[jUser]["From_Email"].ToString();
                                from.ProfilePicture = fromSrc;
                                from.IsOnline = Convert.ToBoolean(dtUser.Rows[jUser]["From_IsOnline"]);
                                from.Description = dtUser.Rows[jUser]["From_Description"].ToString();
                                from.LastSeen = Convert.ToDateTime(dtUser.Rows[jUser]["From_Lastseen"]);
                                from_validate.UserExists = true;

                                if (Convert.ToInt32(dtUser.Rows[jUser]["From_Authenticated"]) > 0)
                                {
                                    from_validate.IsAuthenticated = true;
                                }
                                else
                                {
                                    from_validate.IsAuthenticated = false;
                                }

                                from.Response = from_validate;

                                //To Object
                                if (dtUser.Rows[jUser]["To_ProfilePicture"].ToString() != null && dtUser.Rows[jUser]["To_ProfilePicture"].ToString() != "")
                                {
                                    var base64 = Convert.ToBase64String((byte[])dtUser.Rows[jUser]["To_ProfilePicture"]);
                                    toSrc = String.Format("data:image/png;base64, {0}", base64);
                                }

                                to.UserID = dtUser.Rows[jUser]["To_UserID"].ToString();
                                to.UserName = dtUser.Rows[jUser]["To_UserName"].ToString();
                                to.Email = dtUser.Rows[jUser]["To_Email"].ToString();
                                to.ProfilePicture = toSrc;
                                to.IsOnline = Convert.ToBoolean(dtUser.Rows[jUser]["To_IsOnline"]);
                                to.Description = dtUser.Rows[jUser]["To_Description"].ToString();
                                to.LastSeen = Convert.ToDateTime(dtUser.Rows[jUser]["To_Lastseen"]);
                                to_validate.UserExists = true;

                                if (Convert.ToInt32(dtUser.Rows[jUser]["To_Authenticated"]) > 0)
                                {
                                    to_validate.IsAuthenticated = true;
                                }
                                else
                                {
                                    to_validate.IsAuthenticated = false;
                                }

                                to.Response = to_validate;

                                request.From = from;
                                request.To = to;
                                request.Message = dtUser.Rows[jUser]["_Message"].ToString();
                                request.From_Type = dtUser.Rows[jUser]["From_Type"].ToString();
                                request.To_Type = dtUser.Rows[jUser]["To_Type"].ToString();
                                request.ID = (int)dtUser.Rows[jUser]["id"];

                                requests.Add(request);
                            }
                        }
                    }

                    else if (From_Type == "User" && To_Type == "Group")
                    {
                        SqlCommand cmdUser = new SqlCommand("GetUserRequests_User_Group", con);
                        cmdUser.CommandType = CommandType.StoredProcedure;
                        cmdUser.Parameters.AddWithValue("@UserID", groupid);
                        cmdUser.Parameters.AddWithValue("@IPAddress", "");
                        cmdUser.Parameters.AddWithValue("@ID", ID);

                        SqlDataAdapter sdaUser = new SqlDataAdapter(cmdUser);
                        DataTable dtUser = new DataTable();
                        sdaUser.Fill(dtUser);

                        int iUser = dtUser.Rows.Count;

                        if (iUser > 0)
                        {
                            for (int jUser = 0; jUser < iUser; jUser++)
                            {
                                RequestModel request = new RequestModel();
                                UserModel from = new UserModel();
                                UserModel to = new UserModel();
                                ValidateUser from_validate = new ValidateUser();
                                ValidateUser to_validate = new ValidateUser();
                                string fromSrc = String.Empty, toSrc = String.Empty;

                                //From Object
                                if (dtUser.Rows[jUser]["From_ProfilePicture"].ToString() != null && dtUser.Rows[jUser]["From_ProfilePicture"].ToString() != "")
                                {
                                    var base64 = Convert.ToBase64String((byte[])dtUser.Rows[jUser]["From_ProfilePicture"]);
                                    fromSrc = String.Format("data:image/png;base64, {0}", base64);
                                }

                                from.UserID = dtUser.Rows[jUser]["From_UserID"].ToString();
                                from.UserName = dtUser.Rows[jUser]["From_UserName"].ToString();
                                from.Email = dtUser.Rows[jUser]["From_Email"].ToString();
                                from.ProfilePicture = fromSrc;
                                from.IsOnline = Convert.ToBoolean(dtUser.Rows[jUser]["From_IsOnline"]);
                                from.Description = dtUser.Rows[jUser]["From_Description"].ToString();
                                from.LastSeen = Convert.ToDateTime(dtUser.Rows[jUser]["From_Lastseen"]);
                                from_validate.UserExists = true;

                                if (Convert.ToInt32(dtUser.Rows[jUser]["From_Authenticated"]) > 0)
                                {
                                    from_validate.IsAuthenticated = true;
                                }
                                else
                                {
                                    from_validate.IsAuthenticated = false;
                                }

                                from.Response = from_validate;

                                //To Object
                                if (dtUser.Rows[jUser]["To_ProfilePicture"].ToString() != null && dtUser.Rows[jUser]["To_ProfilePicture"].ToString() != "")
                                {
                                    var base64 = Convert.ToBase64String((byte[])dtUser.Rows[jUser]["To_ProfilePicture"]);
                                    toSrc = String.Format("data:image/png;base64, {0}", base64);
                                }

                                to.UserID = dtUser.Rows[jUser]["To_UserID"].ToString();
                                to.UserName = dtUser.Rows[jUser]["To_UserName"].ToString();
                                to.ProfilePicture = toSrc;
                                to.IsOnline = true;

                                request.From = from;
                                request.To = to;
                                request.Message = dtUser.Rows[jUser]["_Message"].ToString();
                                request.From_Type = dtUser.Rows[jUser]["From_Type"].ToString();
                                request.To_Type = dtUser.Rows[jUser]["To_Type"].ToString();
                                request.ID = (int)dtUser.Rows[jUser]["id"];

                                requests.Add(request);
                            }
                        }
                    }

                    else if (From_Type == "Group" && To_Type == "User")
                    {
                        SqlCommand cmdUser = new SqlCommand("GetUserRequests_Group_User", con);
                        cmdUser.CommandType = CommandType.StoredProcedure;
                        cmdUser.Parameters.AddWithValue("@ID", ID);

                        SqlDataAdapter sdaUser = new SqlDataAdapter(cmdUser);
                        DataTable dtUser = new DataTable();
                        sdaUser.Fill(dtUser);

                        int iUser = dtUser.Rows.Count;

                        if (iUser > 0)
                        {
                            for (int jUser = 0; jUser < iUser; jUser++)
                            {
                                RequestModel request = new RequestModel();
                                UserModel from = new UserModel();
                                UserModel to = new UserModel();
                                ValidateUser from_validate = new ValidateUser();
                                ValidateUser to_validate = new ValidateUser();
                                string fromSrc = String.Empty, toSrc = String.Empty;

                                //From Object
                                if (dtUser.Rows[jUser]["From_ProfilePicture"].ToString() != null && dtUser.Rows[jUser]["From_ProfilePicture"].ToString() != "")
                                {
                                    var base64 = Convert.ToBase64String((byte[])dtUser.Rows[jUser]["From_ProfilePicture"]);
                                    fromSrc = String.Format("data:image/png;base64, {0}", base64);
                                }

                                from.UserID = dtUser.Rows[jUser]["From_UserID"].ToString();
                                from.UserName = dtUser.Rows[jUser]["From_UserName"].ToString();
                                from.ProfilePicture = fromSrc;
                                from.IsOnline = true;
                                from_validate.UserExists = true;

                                //To Object
                                if (dtUser.Rows[jUser]["To_ProfilePicture"].ToString() != null && dtUser.Rows[jUser]["To_ProfilePicture"].ToString() != "")
                                {
                                    var base64 = Convert.ToBase64String((byte[])dtUser.Rows[jUser]["To_ProfilePicture"]);
                                    toSrc = String.Format("data:image/png;base64, {0}", base64);
                                }

                                to.UserID = dtUser.Rows[jUser]["To_UserID"].ToString();
                                to.UserName = dtUser.Rows[jUser]["To_UserName"].ToString();
                                to.Email = dtUser.Rows[jUser]["To_Email"].ToString();
                                to.ProfilePicture = toSrc;
                                to.IsOnline = Convert.ToBoolean(dtUser.Rows[jUser]["To_IsOnline"]);
                                to.Description = dtUser.Rows[jUser]["To_Description"].ToString();
                                to.LastSeen = Convert.ToDateTime(dtUser.Rows[jUser]["To_Lastseen"]);
                                to_validate.UserExists = true;

                                if (Convert.ToInt32(dtUser.Rows[jUser]["To_Authenticated"]) > 0)
                                {
                                    to_validate.IsAuthenticated = true;
                                }
                                else
                                {
                                    to_validate.IsAuthenticated = false;
                                }

                                to.Response = to_validate;

                                request.From = from;
                                request.To = to;
                                request.Message = dtUser.Rows[jUser]["_Message"].ToString();
                                request.From_Type = dtUser.Rows[jUser]["From_Type"].ToString();
                                request.To_Type = dtUser.Rows[jUser]["To_Type"].ToString();
                                request.ID = (int)dtUser.Rows[jUser]["id"];

                                requests.Add(request);
                            }
                        }
                    }
                }

                int success = (int)ResponseCodes.Successfull;
                response.ResponseCode = success;
                response.ResponseMessage = ResponseCodes.Successfull.ToString();
                response.Data = requests;
            }

            else
            {
                int noRequests = (int)ResponseCodes.NoRequests;
                response.ResponseCode = noRequests;
                response.ResponseMessage = ResponseCodes.NoRequests.ToString();
            }

            return response;
        }

        /// <summary>
        /// Gets all requests sent to a specific group
        /// </summary>
        /// <returns>Response object</returns>
        [HttpGet]
        [Route("recieved/{groupid}")]
        public ResponseModel _GetGroupRecievedRequests(string groupid)
        {
            ResponseModel response = new ResponseModel();
            List<object> requests = new List<object>();

            string get = "GetRequestsRecievedByUser";
            SqlCommand cmd = new SqlCommand(get, con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserID", groupid);
            cmd.Parameters.AddWithValue("@IPAddress", "");

            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);

            int i = dt.Rows.Count;

            if (i > 0)
            {
                for (int j = 0; j < i; j++)
                {
                    string From_Type = dt.Rows[j]["From_Type"].ToString();
                    string To_Type = dt.Rows[j]["To_Type"].ToString();
                    int ID = (int)dt.Rows[j]["id"];

                    if (From_Type == "User" && To_Type == "User")
                    {
                        SqlCommand cmdUser = new SqlCommand("GetUserRequests_User_User", con);
                        cmdUser.CommandType = CommandType.StoredProcedure;
                        cmdUser.Parameters.AddWithValue("@UserID", User.Identity.Name);
                        cmdUser.Parameters.AddWithValue("@IPAddress", "");
                        cmdUser.Parameters.AddWithValue("@ID", ID);

                        SqlDataAdapter sdaUser = new SqlDataAdapter(cmdUser);
                        DataTable dtUser = new DataTable();
                        sdaUser.Fill(dtUser);

                        int iUser = dtUser.Rows.Count;

                        if (iUser > 0)
                        {
                            for (int jUser = 0; jUser < iUser; jUser++)
                            {
                                RequestModel request = new RequestModel();
                                UserModel from = new UserModel();
                                UserModel to = new UserModel();
                                ValidateUser from_validate = new ValidateUser();
                                ValidateUser to_validate = new ValidateUser();
                                string fromSrc = String.Empty, toSrc = String.Empty;

                                //From Object
                                if (dtUser.Rows[jUser]["From_ProfilePicture"].ToString() != null && dtUser.Rows[jUser]["From_ProfilePicture"].ToString() != "")
                                {
                                    var base64 = Convert.ToBase64String((byte[])dtUser.Rows[jUser]["From_ProfilePicture"]);
                                    fromSrc = String.Format("data:image/png;base64, {0}", base64);
                                }

                                from.UserID = dtUser.Rows[jUser]["From_UserID"].ToString();
                                from.UserName = dtUser.Rows[jUser]["From_UserName"].ToString();
                                from.Email = dtUser.Rows[jUser]["From_Email"].ToString();
                                from.ProfilePicture = fromSrc;
                                from.IsOnline = Convert.ToBoolean(dtUser.Rows[jUser]["From_IsOnline"]);
                                from.Description = dtUser.Rows[jUser]["From_Description"].ToString();
                                from.LastSeen = Convert.ToDateTime(dtUser.Rows[jUser]["From_Lastseen"]);
                                from_validate.UserExists = true;

                                if (Convert.ToInt32(dtUser.Rows[jUser]["From_Authenticated"]) > 0)
                                {
                                    from_validate.IsAuthenticated = true;
                                }
                                else
                                {
                                    from_validate.IsAuthenticated = false;
                                }

                                from.Response = from_validate;

                                //To Object
                                if (dtUser.Rows[jUser]["To_ProfilePicture"].ToString() != null && dtUser.Rows[jUser]["To_ProfilePicture"].ToString() != "")
                                {
                                    var base64 = Convert.ToBase64String((byte[])dtUser.Rows[jUser]["To_ProfilePicture"]);
                                    toSrc = String.Format("data:image/png;base64, {0}", base64);
                                }

                                to.UserID = dtUser.Rows[jUser]["To_UserID"].ToString();
                                to.UserName = dtUser.Rows[jUser]["To_UserName"].ToString();
                                to.Email = dtUser.Rows[jUser]["To_Email"].ToString();
                                to.ProfilePicture = toSrc;
                                to.IsOnline = Convert.ToBoolean(dtUser.Rows[jUser]["To_IsOnline"]);
                                to.Description = dtUser.Rows[jUser]["To_Description"].ToString();
                                to.LastSeen = Convert.ToDateTime(dtUser.Rows[jUser]["To_Lastseen"]);
                                to_validate.UserExists = true;

                                if (Convert.ToInt32(dtUser.Rows[jUser]["To_Authenticated"]) > 0)
                                {
                                    to_validate.IsAuthenticated = true;
                                }
                                else
                                {
                                    to_validate.IsAuthenticated = false;
                                }

                                to.Response = to_validate;

                                request.From = from;
                                request.To = to;
                                request.Message = dtUser.Rows[jUser]["_Message"].ToString();
                                request.From_Type = dtUser.Rows[jUser]["From_Type"].ToString();
                                request.To_Type = dtUser.Rows[jUser]["To_Type"].ToString();
                                request.ID = (int)dtUser.Rows[jUser]["id"];

                                requests.Add(request);
                            }
                        }
                    }

                    else if (From_Type == "User" && To_Type == "Group")
                    {
                        SqlCommand cmdUser = new SqlCommand("GetUserRequests_User_Group", con);
                        cmdUser.CommandType = CommandType.StoredProcedure;
                        cmdUser.Parameters.AddWithValue("@UserID", User.Identity.Name);
                        cmdUser.Parameters.AddWithValue("@IPAddress", "");
                        cmdUser.Parameters.AddWithValue("@ID", ID);

                        SqlDataAdapter sdaUser = new SqlDataAdapter(cmdUser);
                        DataTable dtUser = new DataTable();
                        sdaUser.Fill(dtUser);

                        int iUser = dtUser.Rows.Count;

                        if (iUser > 0)
                        {
                            for (int jUser = 0; jUser < iUser; jUser++)
                            {
                                RequestModel request = new RequestModel();
                                UserModel from = new UserModel();
                                UserModel to = new UserModel();
                                ValidateUser from_validate = new ValidateUser();
                                ValidateUser to_validate = new ValidateUser();
                                string fromSrc = String.Empty, toSrc = String.Empty;

                                //From Object
                                if (dtUser.Rows[jUser]["From_ProfilePicture"].ToString() != null && dtUser.Rows[jUser]["From_ProfilePicture"].ToString() != "")
                                {
                                    var base64 = Convert.ToBase64String((byte[])dtUser.Rows[jUser]["From_ProfilePicture"]);
                                    fromSrc = String.Format("data:image/png;base64, {0}", base64);
                                }

                                from.UserID = dtUser.Rows[jUser]["From_UserID"].ToString();
                                from.UserName = dtUser.Rows[jUser]["From_UserName"].ToString();
                                from.Email = dtUser.Rows[jUser]["From_Email"].ToString();
                                from.ProfilePicture = fromSrc;
                                from.IsOnline = Convert.ToBoolean(dtUser.Rows[jUser]["From_IsOnline"]);
                                from.Description = dtUser.Rows[jUser]["From_Description"].ToString();
                                from.LastSeen = Convert.ToDateTime(dtUser.Rows[jUser]["From_Lastseen"]);
                                from_validate.UserExists = true;

                                if (Convert.ToInt32(dtUser.Rows[jUser]["From_Authenticated"]) > 0)
                                {
                                    from_validate.IsAuthenticated = true;
                                }
                                else
                                {
                                    from_validate.IsAuthenticated = false;
                                }

                                from.Response = from_validate;

                                //To Object
                                if (dtUser.Rows[jUser]["To_ProfilePicture"].ToString() != null && dtUser.Rows[jUser]["To_ProfilePicture"].ToString() != "")
                                {
                                    var base64 = Convert.ToBase64String((byte[])dtUser.Rows[jUser]["To_ProfilePicture"]);
                                    toSrc = String.Format("data:image/png;base64, {0}", base64);
                                }

                                to.UserID = dtUser.Rows[jUser]["To_UserID"].ToString();
                                to.UserName = dtUser.Rows[jUser]["To_UserName"].ToString();
                                to.ProfilePicture = toSrc;
                                to.IsOnline = true;

                                request.From = from;
                                request.To = to;
                                request.Message = dtUser.Rows[jUser]["_Message"].ToString();
                                request.From_Type = dtUser.Rows[jUser]["From_Type"].ToString();
                                request.To_Type = dtUser.Rows[jUser]["To_Type"].ToString();
                                request.ID = (int)dtUser.Rows[jUser]["id"];

                                requests.Add(request);
                            }
                        }
                    }

                    else if (From_Type == "Group" && To_Type == "User")
                    {
                        SqlCommand cmdUser = new SqlCommand("GetUserRequests_Group_User", con);
                        cmdUser.CommandType = CommandType.StoredProcedure;
                        cmdUser.Parameters.AddWithValue("@ID", ID);

                        SqlDataAdapter sdaUser = new SqlDataAdapter(cmdUser);
                        DataTable dtUser = new DataTable();
                        sdaUser.Fill(dtUser);

                        int iUser = dtUser.Rows.Count;

                        if (iUser > 0)
                        {
                            for (int jUser = 0; jUser < iUser; jUser++)
                            {
                                RequestModel request = new RequestModel();
                                UserModel from = new UserModel();
                                UserModel to = new UserModel();
                                ValidateUser from_validate = new ValidateUser();
                                ValidateUser to_validate = new ValidateUser();
                                string fromSrc = String.Empty, toSrc = String.Empty;

                                //From Object
                                if (dtUser.Rows[jUser]["From_ProfilePicture"].ToString() != null && dtUser.Rows[jUser]["From_ProfilePicture"].ToString() != "")
                                {
                                    var base64 = Convert.ToBase64String((byte[])dtUser.Rows[jUser]["From_ProfilePicture"]);
                                    fromSrc = String.Format("data:image/png;base64, {0}", base64);
                                }

                                from.UserID = dtUser.Rows[jUser]["From_UserID"].ToString();
                                from.UserName = dtUser.Rows[jUser]["From_UserName"].ToString();
                                from.ProfilePicture = fromSrc;
                                from.IsOnline = true;
                                from_validate.UserExists = true;

                                //To Object
                                if (dtUser.Rows[jUser]["To_ProfilePicture"].ToString() != null && dtUser.Rows[jUser]["To_ProfilePicture"].ToString() != "")
                                {
                                    var base64 = Convert.ToBase64String((byte[])dtUser.Rows[jUser]["To_ProfilePicture"]);
                                    toSrc = String.Format("data:image/png;base64, {0}", base64);
                                }

                                to.UserID = dtUser.Rows[jUser]["To_UserID"].ToString();
                                to.UserName = dtUser.Rows[jUser]["To_UserName"].ToString();
                                to.Email = dtUser.Rows[jUser]["To_Email"].ToString();
                                to.ProfilePicture = toSrc;
                                to.IsOnline = Convert.ToBoolean(dtUser.Rows[jUser]["To_IsOnline"]);
                                to.Description = dtUser.Rows[jUser]["To_Description"].ToString();
                                to.LastSeen = Convert.ToDateTime(dtUser.Rows[jUser]["To_Lastseen"]);
                                to_validate.UserExists = true;

                                if (Convert.ToInt32(dtUser.Rows[jUser]["To_Authenticated"]) > 0)
                                {
                                    to_validate.IsAuthenticated = true;
                                }
                                else
                                {
                                    to_validate.IsAuthenticated = false;
                                }

                                to.Response = to_validate;

                                request.From = from;
                                request.To = to;
                                request.Message = dtUser.Rows[jUser]["_Message"].ToString();
                                request.From_Type = dtUser.Rows[jUser]["From_Type"].ToString();
                                request.To_Type = dtUser.Rows[jUser]["To_Type"].ToString();
                                request.ID = (int)dtUser.Rows[jUser]["id"];

                                requests.Add(request);
                            }
                        }
                    }
                }

                int success = (int)ResponseCodes.Successfull;
                response.ResponseCode = success;
                response.ResponseMessage = ResponseCodes.Successfull.ToString();
                response.Data = requests;
            }

            else
            {
                int noRequests = (int)ResponseCodes.NoRequests;
                response.ResponseCode = noRequests;
                response.ResponseMessage = ResponseCodes.NoRequests.ToString();
            }

            return response;
        }

        /// <summary>
        /// Deletes a request/invitation sent to or by a user from the database
        /// </summary>
        /// <param name="ID">ID of request in the database table</param>
        /// <returns>Response object</returns>
        [HttpDelete]
        [Route("{ID}")]
        public ResponseModel _DeleteRequest(int ID)
        {
            ResponseModel response = new ResponseModel();

            SqlCommand cmd = new SqlCommand("DeleteRequest", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ID", ID);

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
        /// Inserts a new chatroom and chatroom member in the database based of the data returned
        /// </summary>
        /// <param name="userid">The id of the user/group</param>
        /// <param name="ID">Get's the ID of the particular request to evaluate</param>
        /// <returns>Response object</returns>
        [HttpPost]
        [Route("accept/{userid}/{ID}")]
        public ResponseModel _AcceptRequest(string userid, int ID)
        {
            ResponseModel response = new ResponseModel();

            var guid = Guid.NewGuid();
            string ChatRoomID = String.Concat("CHATROOM", "_", guid.ToString());

            SqlCommand cmd = new SqlCommand("AcceptRequest", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ID", ID);
            cmd.Parameters.AddWithValue("@UserID", userid);
            cmd.Parameters.AddWithValue("@IPAddress", "");
            cmd.Parameters.AddWithValue("@ChatRoomID", ChatRoomID);

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
        /// Verifies if a user is allowed to send a request to another user or chatroom(group)
        /// </summary>
        /// <param name="groupid">The ID of the group</param>
        /// <returns>Response object</returns>
        [HttpGet]
        [Route("verify/{groupid}/group")]
        public ResponseModel _VerifyRequestStatus_Group(string groupid)
        {
            ResponseModel response = new ResponseModel();

            string checkIfRequestExists = "select count(*) from Requests where From_ID = @From_ID and To_ID = @To_ID";
            string checkIfChatRoomMemberexists = @"select
			count(*)
			from ChatRoomMembers
			where MemberId=@From_ID and IdentityToRender=@To_ID";

            SqlCommand requestExistsCommand = new SqlCommand(checkIfRequestExists, con);
            requestExistsCommand.Parameters.AddWithValue("@From_ID", User.Identity.Name);
            requestExistsCommand.Parameters.AddWithValue("@To_ID", groupid);

            SqlCommand chatRoomMemberExistsCommand = new SqlCommand(checkIfChatRoomMemberexists, con);
            chatRoomMemberExistsCommand.Parameters.AddWithValue("@From_ID", User.Identity.Name);
            chatRoomMemberExistsCommand.Parameters.AddWithValue("@To_ID", groupid);

            con.Open();

            int requestCount = (int)requestExistsCommand.ExecuteScalar();
            int chatRoomCount = (int)chatRoomMemberExistsCommand.ExecuteScalar();

            con.Close();

            if (requestCount < 1 && chatRoomCount < 1)
            {
                response.ResponseCode = (int)ResponseCodes.RequestValid;
                response.ResponseMessage = ResponseCodes.RequestValid.ToString();
            }

            else if (requestCount > 0)
            {
                response.ResponseCode = (int)ResponseCodes.RequestExists;
                response.ResponseMessage = ResponseCodes.RequestExists.ToString();
            }

            else if (chatRoomCount > 0)
            {
                response.ResponseCode = (int)ResponseCodes.ChatRoomMemberExists;
                response.ResponseMessage = ResponseCodes.ChatRoomMemberExists.ToString();
            }

            return response;
        }

        /// <summary>
        /// Deletes a request/invitation sent to or by a user from the database
        /// </summary>
        /// <param name="userid">The id of the user/group making the request</param>
        /// <param name="To_ID">ID of request recipient</param>
        /// <returns>Response object</returns>
        [HttpDelete]
        [Route("delete/{userid}/{To_ID}")]
        public ResponseModel _DeleteRequest(string userid, string To_ID)
        {
            ResponseModel response = new ResponseModel();

            SqlCommand cmd = new SqlCommand("DeleteRequest_By_From_And_To_ID", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserID", userid);
            cmd.Parameters.AddWithValue("@IPAddress", "");
            cmd.Parameters.AddWithValue("@From_ID", userid);
            cmd.Parameters.AddWithValue("@To_ID", To_ID);

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
    }
}
