using GOChatAPI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using GOChatAPI.Security;
using System.Web.Http.Cors;

namespace GOChatAPI.Controllers
{
    /// <summary>
    /// Handles all API endoints concerning the user and it's information
    /// </summary>
    [Authorize]
    //[EnableCors("*","*","*")]
    [RoutePrefix("api/user")]
    public class UserController : ApiController
    {

        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["GOChat"].ConnectionString);
        UserModel user = new UserModel();
        General general = new General();

        /// <summary>
        /// Get all users of the chat application
        /// </summary>
        /// <returns>Response object</returns>
        [HttpGet]
        [Route("all")]
        public ResponseModel GetAll()
        {
            List<Object> users = new List<Object>();
            ResponseModel response = new ResponseModel();

            SqlCommand cmd = new SqlCommand("GetAllUsers", con);

            cmd.CommandType = CommandType.StoredProcedure;

            con.Open();

            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);

            int i = dt.Rows.Count;

            if (i > 0)
            {
                for (int j = 0; j < i; j++)
                {
                    UserModel user = new UserModel();
                    ValidateUser validate = new ValidateUser();

                    string imgSrc = String.Empty;

                    if (dt.Rows[j]["ProfilePicture"].ToString() != null && dt.Rows[j]["ProfilePicture"].ToString() != "")
                    {
                        var base64 = Convert.ToBase64String((byte[])dt.Rows[j]["ProfilePicture"]);
                        imgSrc = String.Format("data:image/png;base64, {0}", base64);
                    }

                    user.UserID = dt.Rows[j]["UserID"].ToString();
                    user.UserName = dt.Rows[j]["UserName"].ToString();
                    user.Email = dt.Rows[j]["Email"].ToString();
                    user.Description = dt.Rows[j]["Description"].ToString();
                    user.IsOnline = Convert.ToBoolean(dt.Rows[j]["IsOnline"]);
                    user.LastSeen = Convert.ToDateTime(dt.Rows[j]["LastSeen"]);
                    validate.UserExists = true;

                    if (imgSrc != "")
                    {
                        user.ProfilePicture = imgSrc;
                    }
                    else
                    {
                        user.ProfilePicture = null;
                    }

                    if (Convert.ToInt32(dt.Rows[j]["Authenticated"]) > 0)
                    {
                        validate.IsAuthenticated = true;
                    }
                    else
                    {
                        validate.IsAuthenticated = false;
                    }

                    user.Response = validate;

                    users.Add(user);
                }

                int success = (int)ResponseCodes.Successfull;
                response.ResponseCode = success;
                response.ResponseMessage = ResponseCodes.Successfull.ToString();
                response.Data = users;
            }
            else
            {
                int noUser = (int)ResponseCodes.NoUser;
                response.ResponseCode = noUser;
                response.ResponseMessage = ResponseCodes.NoUser.ToString();
            }

            con.Close();

            return response;
        }

        /// <summary>
        /// Returns a single user based on his/her userid
        /// </summary>
        /// <returns>User Object</returns>
        [HttpGet]
        [Route("")]
        public UserModel Get()
        {
            UserModel user = new UserModel();
            ValidateUser validate = new ValidateUser();

            con.Open();

            SqlCommand cmd = new SqlCommand("GetUserByID", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserID", User.Identity.Name);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);

            int rows = dt.Rows.Count;

            if (rows > 0)
            {
                string imgSrc = String.Empty;

                if (dt.Rows[0]["ProfilePicture"].ToString() != null && dt.Rows[0]["ProfilePicture"].ToString() != "")
                {
                    var base64 = Convert.ToBase64String((byte[])dt.Rows[0]["ProfilePicture"]);
                    imgSrc = String.Format("data:image/png;base64, {0}", base64);
                }

                user.UserID = dt.Rows[0]["UserID"].ToString();
                user.UserName = dt.Rows[0]["UserName"].ToString();
                user.Email = dt.Rows[0]["Email"].ToString();
                user.IsOnline = Convert.ToBoolean(dt.Rows[0]["IsOnline"]);
                user.LastSeen = Convert.ToDateTime(dt.Rows[0]["LastSeen"]);
                validate.UserExists = true;

                if (imgSrc != "")
                {
                    user.ProfilePicture = imgSrc;
                }
                else
                {
                    user.ProfilePicture = null;
                }

                if (Convert.ToInt32(dt.Rows[0]["Authenticated"]) > 0)
                {
                    validate.IsAuthenticated = true;
                }
                else
                {
                    validate.IsAuthenticated = false;
                }

                user.Response = validate;

            }
            else
            {
                validate.UserExists = false;
                user.Response = validate;
            }

            con.Close();

            return user;
        }

        /// <summary>
        /// Returns a single user based on his/her userid passed in the URI
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns>User Object</returns>
        [Route("{UserID}")]
        [HttpGet]
        public ResponseModel Get(string UserID)
        {
            ResponseModel response = new ResponseModel();
            UserModel user = new UserModel();
            ValidateUser validate = new ValidateUser();
            List<object> userList = new List<object>();

            con.Open();

            SqlCommand cmd = new SqlCommand("GetUserByID", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserID", UserID);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);

            int rows = dt.Rows.Count;

            if (rows > 0)
            {
                string imgSrc = String.Empty;

                if (dt.Rows[0]["ProfilePicture"].ToString() != null && dt.Rows[0]["ProfilePicture"].ToString() != "")
                {
                    var base64 = Convert.ToBase64String((byte[])dt.Rows[0]["ProfilePicture"]);
                    imgSrc = String.Format("data:image/png;base64, {0}", base64);
                }

                user.UserID = dt.Rows[0]["UserID"].ToString();
                user.UserName = dt.Rows[0]["UserName"].ToString();
                user.Email = dt.Rows[0]["Email"].ToString();
                user.Description = dt.Rows[0]["Description"].ToString();
                user.IsOnline = Convert.ToBoolean(dt.Rows[0]["IsOnline"]);
                user.LastSeen = Convert.ToDateTime(dt.Rows[0]["LastSeen"]);
                validate.UserExists = true;

                if (imgSrc != "")
                {
                    user.ProfilePicture = imgSrc;
                }
                else
                {
                    user.ProfilePicture = null;
                }

                if (Convert.ToInt32(dt.Rows[0]["Authenticated"]) > 0)
                {
                    validate.IsAuthenticated = true;
                }
                else
                {
                    validate.IsAuthenticated = false;
                }

                user.Response = validate;

                int success = (int)ResponseCodes.Successfull;
                response.ResponseCode = success;
                response.ResponseMessage = ResponseCodes.Successfull.ToString();
                userList.Add(user);
                response.Data = userList;
            }
            else
            {
                validate.UserExists = false;
                user.Response = validate;

                int noUser = (int)ResponseCodes.NoUser;
                response.ResponseCode = noUser;
                response.ResponseMessage = ResponseCodes.NoUser.ToString();
            }

            con.Close();

            return response;
        }

        /// <summary>
        /// Returns if a user is logged in based on his/her token
        /// </summary>
        /// <returns>User Object</returns>
        [Route("validate")]
        [HttpGet]
        public ResponseModel ValidateUser()
        {
            ResponseModel response = new ResponseModel();

            con.Open();

            SqlCommand cmd = new SqlCommand("GetUserByID", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserID", User.Identity.Name);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);

            int rows = dt.Rows.Count;

            if (rows > 0)
            {
                if (Convert.ToInt32(dt.Rows[0]["Authenticated"]) > 0)
                {
                    int success = (int)ResponseCodes.Successfull;
                    response.ResponseCode = success;
                    response.ResponseMessage = ResponseCodes.Successfull.ToString();
                }
                else
                {
                    int noUser = (int)ResponseCodes.NotFound;
                    response.ResponseCode = noUser;
                    response.ResponseMessage = "UnAuthenticated";
                }
            }
            else
            {
                int noUser = (int)ResponseCodes.NotFound;
                response.ResponseCode = noUser;
                response.ResponseMessage = ResponseCodes.NotFound.ToString();
            }

            con.Close();

            return response;
        }

        /// <summary>
        /// Check user login credidentials
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns>User object</returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public UserModel Login(UserModel userModel)
        {
            UserModel user = new UserModel();
            ValidateUser validate = new ValidateUser();

            string Password = String.Empty;

            SqlCommand cmd = new SqlCommand("CheckUserLogin", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Email", userModel.Email);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);

            int rows = dt.Rows.Count;

            if (rows > 0)
            {
                for (int i = 0; i < rows; i++)
                {
                    string encrypted = dt.Rows[i]["PWord"].ToString();
                    Password = general.Decrypt(encrypted);

                    if (userModel.Password == Password)
                    {
                        user.UserID = dt.Rows[i]["UserID"].ToString();
                        user.UserName = dt.Rows[i]["UserName"].ToString();
                        user.Email = dt.Rows[i]["Email"].ToString();
                        user.IsOnline = Convert.ToBoolean(dt.Rows[i]["IsOnline"]);
                        user.LastSeen = Convert.ToDateTime(dt.Rows[i]["LastSeen"]);
                        user.Description = dt.Rows[i]["Description"].ToString();
                        user.Password = Password;//general.Encrypt(Password);
                        validate.UserExists = true;

                        if (Convert.ToInt32(dt.Rows[i]["Authenticated"]) > 0)
                        {
                            validate.IsAuthenticated = true;
                        }
                        else
                        {
                            validate.IsAuthenticated = false;
                        }

                        user.Response = validate;
                    }

                    else
                    {
                        validate.UserExists = false;
                        user.Response = validate;
                    }
                }
            }

            else
            {
                validate.UserExists = false;
                user.Response = validate;
            }

            return user;
        }

        /// <summary>
        /// To change the online state of a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("isOnline")]
        [HttpPut]
        public string IsOnline(UserModel user)
        {
            string msg = String.Empty;
            try
            {
                SqlCommand cmd = new SqlCommand("IsOnline", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserID", user.UserID);
                cmd.Parameters.AddWithValue("@IsOnline", user.IsOnline.ToString());

                con.Open();
                int i = cmd.ExecuteNonQuery();
                con.Close();

                if (i > 0)
                {
                    msg = "User is onine...";
                }
                else
                {
                    msg = "Error...";
                }
            }
            catch (Exception ex)
            {
                msg = $"Error {ex}";
            }

            return msg;
        }

        /// <summary>
        /// Change user lastseen to NOW...
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("lastSeen")]
        [HttpPut]
        public string LastSeen()
        {
            string msg = String.Empty;

            try
            {
                SqlCommand cmd = new SqlCommand("LastSeen", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserId", User.Identity.Name);
                cmd.Parameters.AddWithValue("@LastSeen", DateTime.Now);

                con.Open();
                int i = cmd.ExecuteNonQuery();
                con.Close();

                if (i > 0)
                {
                    msg = $"User was last seen at {DateTime.Now}";
                }
                else
                {
                    msg = "Error...";
                }
            }
            catch (Exception ex)
            {
                msg = $"Error {ex}";
            }

            return msg;
        }

        /// <summary>
        /// Creates a new user in the database
        /// </summary>
        /// <param name="user">User object in which the JSON object is being mapped into</param>
        /// <returns>Boolean</returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("register")]
        public UserModel Register(UserModel user)
        {
            UserModel returnModel = new UserModel();
            ValidateUser validate = new ValidateUser();

            string Id = Guid.NewGuid().ToString();
            DateTime date = DateTime.Now;
            int code = general.Random();
            string Password = general.Encrypt(user.Password);

            try
            {
                SqlCommand cmd = new SqlCommand("Register", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserId", Id);
                cmd.Parameters.AddWithValue("@UserName", user.UserName);
                cmd.Parameters.AddWithValue("@Password", Password);
                cmd.Parameters.AddWithValue("@Email", user.Email);
                //cmd.Parameters.AddWithValue("@ConfirmCode", OTP);
                cmd.Parameters.AddWithValue("@DateCreated", date);
                var returnParam = cmd.Parameters.Add("@ReturnValue", SqlDbType.Int);
                returnParam.Direction = ParameterDirection.ReturnValue;

                con.Open();

                cmd.ExecuteNonQuery();
                int i = (int)returnParam.Value;

                if (i > 0)
                {
                    returnModel.UserID = Id;
                    returnModel.UserName = user.UserName;
                    returnModel.Email = user.Email;
                    returnModel.Password = user.Password;
                    validate.UserExists = true;
                    returnModel.Response = validate;
                }
                else
                {
                    validate.UserExists = false;
                    returnModel.Response = validate;
                }

                con.Close();
            }

            catch (Exception)
            {
                validate.UserExists = false;
                returnModel.Response = validate;
            }

            return returnModel;
        }

        /// <summary>
        /// Inserts One time password in the database with record of user specified in the user object and sends mail to the user
        /// </summary>
        /// <param name="user">Object the JSON body is being mapped into</param>
        /// <returns>Boolean</returns>
        [AllowAnonymous]
        [HttpPut]
        [Route("updateOTP")]
        public bool UpdateOTP(UserModel user)
        {
            bool msg = false;

            int code = general.Random();
            string OTP = general.Encrypt(code.ToString());

            try
            {
                SqlCommand cmd = new SqlCommand("InsertOTP", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@ConfirmCode", OTP);

                con.Open();

                int i = cmd.ExecuteNonQuery();

                if (i > 0)
                {
                    var mailmsg = general.Mail(user.Email, "onukwilip@gmail.com", "GO Chat", user.UserName, code.ToString(), "no-reply");
                    //string mailmsg = general.Mail2(user.Email, code.ToString(), "No-reply");
                    if (mailmsg == null)
                    {
                        msg = false;
                    }
                    msg = true;
                }
                else
                {
                    msg = false;
                }

                con.Close();
            }

            catch (Exception)
            {
                msg = false;
            }

            return msg;
        }

        /// <summary>
        /// Erases One time password from database with record of specified user. Usually done when the OTP is verified or when he exits the client side application
        /// </summary>
        /// <param name="user">Object the JSON body is mapped into</param>
        /// <returns>Boolean</returns>
        [AllowAnonymous]
        [HttpPut]
        [Route("eraseOTP")]
        public bool EraseOTP(UserModel user)
        {
            bool msg = false;

            try
            {
                SqlCommand cmd = new SqlCommand("eraseOTP", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Email", user.Email);

                con.Open();

                int i = cmd.ExecuteNonQuery();

                if (i > 0)
                {
                    msg = true;
                }
                else
                {
                    msg = false;
                }

                con.Close();
            }

            catch (Exception)
            {
                msg = false;
            }

            return msg;
        }

        /// <summary>
        /// Confirms OTP of user, if true returns true else returns false
        /// </summary>
        /// <param name="user">Object the JSON body is being mapped into</param>
        /// <returns>Boolean</returns>
        [AllowAnonymous]
        [HttpPut]
        [Route("confirmOTP")]
        public bool ConfirmOTP(UserModel user)
        {
            bool msg = false;

            try
            {
                SqlCommand cmd = new SqlCommand("selectOTP", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Email", user.Email);

                con.Open();

                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                sda.Fill(dt);

                int count = dt.Rows.Count;

                if (count > 0)
                {
                    string OTP = general.Decrypt(dt.Rows[0]["ConfirmCode"].ToString());

                    if (user.OTP == OTP)
                    {
                        SqlCommand cmdTrue = new SqlCommand("AuthenticateUser", con);

                        cmdTrue.CommandType = CommandType.StoredProcedure;
                        cmdTrue.Parameters.AddWithValue("@Email", user.Email);

                        int j = cmdTrue.ExecuteNonQuery();

                        if (j > 0)
                        {
                            msg = true;
                        }
                        else
                        {
                            msg = false;
                        }
                    }
                    else
                    {
                        msg = false;
                    }
                }
                else
                {
                    msg = false;
                }

                con.Close();
            }
            catch (Exception)
            {
                msg = false;
            }

            return msg;
        }

        /// <summary>
        /// As the name implies it gets a user by his/her email address
        /// </summary>
        /// <param name="body">The object the body is being mapped into</param>
        /// <returns>Boolean</returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("getUserByEmail")]
        public List<UserModel> GetUser(UserModel body)
        {
            UserModel user = new UserModel();
            ValidateUser validate = new ValidateUser();

            List<UserModel> users = new List<UserModel>();

            SqlCommand cmd = new SqlCommand("CheckUserLogin", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Email", body.Email);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);

            int rows = dt.Rows.Count;

            if (rows > 0)
            {
                for (int i = 0; i < rows; i++)
                {
                    user.UserID = dt.Rows[i]["UserID"].ToString();
                    user.UserName = dt.Rows[i]["UserName"].ToString();
                    user.Email = dt.Rows[i]["Email"].ToString();
                    user.IsOnline = Convert.ToBoolean(dt.Rows[i]["IsOnline"]);
                    user.LastSeen = Convert.ToDateTime(dt.Rows[i]["LastSeen"]);

                    validate.UserExists = true;

                    if (Convert.ToInt32(dt.Rows[i]["Authenticated"]) > 0)
                    {
                        validate.IsAuthenticated = true;
                    }
                    else
                    {
                        validate.IsAuthenticated = false;
                    }

                    user.Response = validate;
                }
            }
            else
            {
                user.Response = validate;
            }

            users.Add(user);

            return users;
        }

        /// <summary>
        /// This is to insert or update user IP address after registeration or login
        /// </summary>
        [AllowAnonymous]
        [HttpPost]
        [Route("IPAddress")]
        public ResponseModel LogIPAddress(IPAddressModel iPAddress)
        {
            ResponseModel response = new ResponseModel();

            SqlCommand cmd = new SqlCommand("LogIPAddress", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IPAddress", iPAddress.IPv4);
            cmd.Parameters.AddWithValue("@City", iPAddress.City);
            cmd.Parameters.AddWithValue("@Country_code", iPAddress.Country_code);
            cmd.Parameters.AddWithValue("@Country_name", iPAddress.Country_name);
            cmd.Parameters.AddWithValue("@Latitude", iPAddress.Latitude);
            cmd.Parameters.AddWithValue("@Longitude", iPAddress.Longitude);
            cmd.Parameters.AddWithValue("@State", iPAddress.State);
            cmd.Parameters.AddWithValue("@UserId", iPAddress.UserId);

            con.Open();
            int i = cmd.ExecuteNonQuery();
            con.Close();

            if (i > 0)
            {
                ResponseCodes success = ResponseCodes.Successfull;
                response.ResponseCode = (int)success;
                response.ResponseMessage = "Successfull";
            }
            else
            {
                ResponseCodes fail = ResponseCodes.Unsuccessfull;

                response.ResponseCode = (int)fail;
                response.ResponseMessage = "Unsuccessfull";
            }

            return response;
        }

        /// <summary>
        /// This deletes user IP address from log table. Usually done when user logs out of his/her account
        /// </summary>
        /// <param name="base64clientid">This accepts the id of the client accessing this application</param>
        /// <returns>Response object</returns>
        [HttpDelete]
        [Route("logout/{base64clientid}")]
        public ResponseModel LogOut(string base64clientid)
        {
            ResponseModel response = new ResponseModel();

            string clientid = General.ConvertFromBase64(base64clientid);

            SqlCommand cmd = new SqlCommand("LogOut", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@userid", User.Identity.Name);
            cmd.Parameters.AddWithValue("@clientid", clientid);

            con.Open();
            int i = cmd.ExecuteNonQuery();
            con.Close();

            if (i > 0)
            {
                var success = ResponseCodes.Successfull;
                response.ResponseCode = (int)success;
                response.ResponseMessage = "Successfull";
            }
            else
            {
                var fail = ResponseCodes.Unsuccessfull;
                response.ResponseCode = (int)fail;
                response.ResponseMessage = "Unsuccessfull";
            }

            return response;
        }

        /// <summary>
        /// Updates user information with userid passed in the URI
        /// </summary>
        /// <param name="UserID">ID of user</param>
        /// <param name="Base64IPAddress">Base 64 ip address</param>
        /// <param name="user">Object in which the body is being mapped into</param>
        /// <returns>Response object</returns>
        [HttpPut]
        [Route("")]
        public ResponseModel UpdateUser(UserModel user)
        {
            ResponseModel response = new ResponseModel();

            SqlCommand cmd = new SqlCommand("UpdateUser", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserID", User.Identity.Name);
            cmd.Parameters.AddWithValue("@UserName", user.UserName);
            cmd.Parameters.AddWithValue("@Email", user.Email);
            cmd.Parameters.AddWithValue("@Description", user.Description);

            con.Open();
            int i = cmd.ExecuteNonQuery();
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
        /// Updates the profile picture of user with userid in the URI
        /// </summary>
        /// <param name="UserID">ID of user</param>
        /// <param name="Base64IPAddress">IP address in a base64 fromat </param>
        /// <returns>Response object</returns>
        [HttpPut]
        [Route("file")]
        public async Task<ResponseModel> UpdateUserProfilePicture()
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
                    name = name.Trim('"');
                    var localFileName = file.LocalFileName;
                    res = SaveFile(localFileName, name, User.Identity.Name);
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

            }

            return response;
        }

        public bool SaveFile(string localFile, string fileName, string UserID)
        {
            byte[] fileBytes;

            using (var fs = new FileStream(localFile, FileMode.Open, FileAccess.Read))
            {
                fileBytes = new byte[fs.Length];
                fs.Read(fileBytes, 0, Convert.ToInt32(fs.Length));
            }

            SqlCommand cmd = new SqlCommand("UpdateUserImage", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserID", UserID);
            cmd.Parameters.AddWithValue("@ProfilePicture", fileBytes);

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

        /// <summary>
        /// Changes the password of user with userid in the URI
        /// </summary>
        /// <param name="UserID">ID of user</param>
        /// <param name="Base64IPAddress">IP address in a base64 fromat </param>
        /// <param name="password">Object the JSON body is being mapped into</param>
        /// <returns>Response object</returns>
        [HttpPut]
        [Route("password")]
        public ResponseModel ChangePassword(PasswordModel password)
        {
            ResponseModel response = new ResponseModel();
            string decodedPassword = String.Empty;

            con.Open();

            SqlCommand cmd = new SqlCommand("GetPassword", con);
            cmd.Parameters.AddWithValue("@UserID", User.Identity.Name);
            cmd.CommandType = CommandType.StoredProcedure;

            SqlCommand cmdUpdate = new SqlCommand("ChangePassword", con);
            cmdUpdate.Parameters.AddWithValue("@Password", general.Encrypt(password.NewPassword));
            cmdUpdate.Parameters.AddWithValue("@UserID", User.Identity.Name);
            cmdUpdate.CommandType = CommandType.StoredProcedure;

            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);
            int i = dt.Rows.Count;

            if (i > 0)
            {
                decodedPassword = dt.Rows[0]["PWord"].ToString() != null || dt.Rows[0]["PWord"].ToString() != "" ? general.Decrypt(dt.Rows[0]["PWord"].ToString()) : "";

                if (decodedPassword == password.OldPassword && password.NewPassword == password.RetypePassword)
                {
                    int j = cmdUpdate.ExecuteNonQuery();

                    if (j > 0)
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

                else
                {
                    var fail = ResponseCodes.Unsuccessfull;
                    response.ResponseCode = (int)fail;
                    response.ResponseMessage = "The field 'Old password' doesn't match the password OR the field 'New Password' doesn't match the field 'Retype Password'";
                }
            }

            con.Close();

            return response;
        }

        [HttpGet]
        [Route("identity")]
        [AllowAnonymous]
        public string _Identity()
        {
            string name = User.Identity.Name;
            string ip = General.IPAddress;

            return $"{name} {ip}";
        }
    }
}
