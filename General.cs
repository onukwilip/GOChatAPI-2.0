using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using MailKit;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using GOChatAPI.Models;
using System.Data.SqlClient;
using System.Configuration;

namespace GOChatAPI
{
    public enum ResponseCodes
    {
        //Success or Failure
        Successfull = 200,
        Deleted = 204,
        Unsuccessfull = 400,
        NoUser = 404,
        NoRequests = 404,
        NoChatRoom = 404,
        NotFound = 404,
        //Mail
        MailNotSent = 100,
        //Requests Validation
        RequestValid = 0,
        RequestExists = 1,
        ChatRoomMemberExists = 2,
        UserIsIgnored = 3
    }

    public class General
    {
        //public readonly string domain = "https://localhost:44358";
        //public readonly string domain = "https://gochatapi-2-0.azurewebsites.net";
        public readonly string domain = "https://gochatapi.goit.net.ng";
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["GOChat"].ConnectionString);
        public static string IPAddress { get; set; }

        /// <summary>
        /// Generates a random set of 6 digits,
        /// commonly used to generate random One time passwords (OTP)
        /// </summary>
        /// <returns>6 Random digits</returns>
        public int Random()
        {
            int random = new Random().Next(100000, 999999);
            return random;
        }

        /// <summary>
        /// Sends mail using google smtp
        /// </summary>
        /// <param name="fromEmail">The email address of the sender</param>
        /// <param name="toEmail">The email address of the reciepient</param>
        /// <param name="fromName">The Name sender</param>
        /// <param name="toName">The name of the reciepient</param>
        /// <param name="body">The body of the mail</param>
        /// <param name="subject">The subject of the mail</param>
        /// <returns>String message if successfull else error message</returns>
        public string Mail(string toEmail, string fromEmail, string fromName, string toName, string body, string subject)
        {
            string msg = String.Empty;
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName, fromEmail));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = subject;

                message.Body = new TextPart("html")
                {
                    Text = body
                };

                using (var client = new SmtpClient())
                {
                    // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                    client.ServerCertificateValidationCallback = (s, c, h, t) => true;

                    //client.Connect("smtp.gmail.com", 587, false);
                    client.Connect("smtp.gmail.com", 465, true);

                    // Note: only needed if the SMTP server requires authentication
                    client.Authenticate("onukwilip@gmail.com", "zkauigqmghjdtges");

                    client.Send(message);
                    client.Disconnect(true);

                    msg = "Mail sent successfully...";
                }
            }

            catch (Exception ex)
            {
                msg = null;//$"Something went wrong...😥";
            }

            return msg;
        }

        /// <summary>
        /// Sends mail using google smtp
        /// </summary>
        /// <param name="email">The email address to be sent to</param>
        /// <param name="Body">The body of the mail</param>
        /// <param name="subject">The subject of the mail</param>
        /// <returns>String message if successfull else error message</returns>
        public string Mail2(string email, string Body, string subject)
        {
            string msg = String.Empty;
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Go-IT", "onukwilip@gmail.com"));
                message.To.Add(new MailboxAddress(email, email));
                message.Subject = subject;

                message.Body = new TextPart("html")
                {
                    Text = Body
                };

                using (var client = new SmtpClient())
                {
                    // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                    client.ServerCertificateValidationCallback = (s, c, h, t) => true;

                    client.Connect("smtp.gmail.com", 25, false);

                    // Note: only needed if the SMTP server requires authentication
                    client.Authenticate("onukwilip@gmail.com", "vradvsdyewmjpjal");//vradvsdyewmjpjal

                    client.Send(message);
                    client.Disconnect(true);

                    msg = "Mail sent successfully...";
                }
            }
            catch (Exception ex)
            {
                msg = $"Error {ex}";
            }

            return msg;
        }

        /// <summary>
        /// Encrypts an encrypted string
        /// </summary>
        /// <param name="encryptString">The string to be encrypted</param>
        /// <returns>Encrypted string</returns>
        public string Encrypt(string encryptString)
        {
            string encryptionKey = "!@#$%^&*()_+=-|}{\\][\":';?></.,'"; //"0123456789abcdefghijklmnopqrstuvwxyzABDEFGHIJKLMNOPQRSTUVWXYZ";
            byte[] clearBytes = Encoding.Unicode.GetBytes(encryptString);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] {
                    0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
                });

                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    encryptString = Convert.ToBase64String(ms.ToArray());
                }
            }

            return encryptString;
        }

        /// <summary>
        /// Decrypts an encrypted string
        /// </summary>
        /// <param name="cipherText">The string to be decrypted</param>
        /// <returns>Decrypted string</returns>
        public string Decrypt(string cipherText)
        {
            string encryptionKey = "!@#$%^&*()_+=-|}{\\][\":';?></.,'"; //"0123456789abcdefghijklmnopqrstuvwxyzABDEFGHIJKLMNOPQRSTUVWXYZ";
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[]{
                    0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
                });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }

            return cipherText;
        }

        /// <summary>
        /// Outputs the hash of the inserted string
        /// </summary>
        /// <param name="input">The string to be hashed</param>
        /// <returns>Hashed string</returns>
        public static string GetHash(string input)
        {
            HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider();
            byte[] byteValue = System.Text.Encoding.UTF8.GetBytes(input);
            byte[] byteHash = hashAlgorithm.ComputeHash(byteValue);
            return Convert.ToBase64String(byteHash);
        }

        /// <summary>
        /// Method called to validate user in database using user id and password
        /// </summary>
        /// <param name="userId"> The Id the user to be validated</param>
        /// <param name="password"> The password of the user</param>
        /// <returns> User model object if user is found, else null.</returns>
        public static UserModel ValidateUser(string userId, string password)
        {
            UserModel user = new UserModel();
            General general = new General();

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["GOChat"].ConnectionString))
            {
                string query = @"SELECT * FROM Users WHERE UserID=@userid";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@userid", userId);

                con.Open();
                SqlDataReader read = cmd.ExecuteReader();

                bool count = read.HasRows;

                if (count && read.Read())
                {
                    string pword = general.Decrypt(read["PWord"].ToString());

                    if (password == pword)
                    {
                        user.UserID = read["UserID"].ToString();
                        user.UserName = read["UserName"].ToString();
                        user.Email = read["Email"].ToString();
                        user.Password = read["PWord"].ToString();

                        read.Close();

                        return user;
                    }
                    else
                    {
                        read.Close();

                        return null;
                    }

                }
                else
                {
                    read.Close();

                    return null;
                }
            }
        }

        /// <summary>
        /// Validates a client in the database
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="clientSecret">secret password of the client</param>
        /// <returns>Client model if found, else null</returns>
        public static ClientModel ValidateClient(string clientId, string clientSecret)
        {
            ClientModel client = new ClientModel();
            General general = new General();

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["GOChat"].ConnectionString))
            {
                string query = @"SELECT * FROM Clients WHERE ClientID=@clientid";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@clientid", clientId);

                con.Open();
                SqlDataReader read = cmd.ExecuteReader();

                bool count = read.HasRows;

                if (count && read.Read())
                {
                    string secret = general.Decrypt(read["ClientSecret"].ToString());

                    if (clientSecret == secret)
                    {
                        client.ClientId = read["ClientID"].ToString();
                        client.ClientSecret = read["ClientSecret"].ToString();
                        client.ClientName = read["ClientName"].ToString();
                        client.Active = (int)read["Active"];
                        client.AllowOrigin = read["AllowedOrigin"].ToString();
                        client.RefreshTokenLifeTime = (int)read["RefreshTokenLifeTime"];

                        read.Close();

                        return client;
                    }
                    else
                    {
                        read.Close();

                        return null;
                    }

                }
                else
                {
                    read.Close();

                    return null;
                }
            }
        }

        /// <summary>
        ///  Method called to add a refresh token if it doesn't exist else, delete existing and add a new one
        /// </summary>
        /// <param name="token">token: the token object</param>
        /// <returns> Returns true</returns>
        public static bool ValidateRefreshToken(RefreshToken token)
        {
            var existingToken = GetRefreshToken(token);

            if (existingToken != null)
            {
                //Remove refresh token
                RemoveRefreshToken(token);
            }

            //Add refresh token
            var addToken = AddRefreshToken(token);

            return addToken;
        }

        /// <summary>
        ///  Method called to get a refresh token from the database using it's userid and clientid
        /// </summary>
        /// <param name="token">The token object</param>
        /// <returns> Returns tpken object if found, else null</returns>
        public static RefreshToken GetRefreshToken(RefreshToken token)
        {
            General general = new General();
            RefreshToken refreshToken = new RefreshToken();

            SqlCommand cmd = new SqlCommand("Select * from RefreshToken where UserId=@userid and ClientId=@clientid", general.con);
            cmd.Parameters.AddWithValue("@userid", token.UserId);
            cmd.Parameters.AddWithValue("@clientid", token.ClientId);

            general.con.Open();
            SqlDataReader read = cmd.ExecuteReader();

            if (read.HasRows && read.Read())
            {
                refreshToken.Id = (int)read["Id"];
                refreshToken.Token = read["Token"].ToString();
                refreshToken.UserId = read["UserId"].ToString();
                refreshToken.ClientId = read["ClientId"].ToString();
                refreshToken.IssuedTime = Convert.ToDateTime(read["IssuedTime"]);
                refreshToken.ExpiredTime = Convert.ToDateTime(read["ExpiredTime"]);
                refreshToken.ProtectedTicket = read["ProtectedTicket"].ToString();

                read.Close();
                general.con.Close();

                return refreshToken;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        ///  Method called to get a refresh token from the database using it's token id
        /// </summary>
        /// <param name="token">The token id</param>
        /// <returns> Returns token object if found, else null</returns>
        public static RefreshToken GetRefreshTokenByID(string token)
        {
            General general = new General();
            RefreshToken refreshToken = new RefreshToken();

            SqlCommand cmd = new SqlCommand("Select * from RefreshToken where Token=@token", general.con);
            cmd.Parameters.AddWithValue("@token", token);

            general.con.Open();
            SqlDataReader read = cmd.ExecuteReader();

            if (read.HasRows && read.Read())
            {
                refreshToken.Id = (int)read["Id"];
                refreshToken.Token = read["Token"].ToString();
                refreshToken.UserId = read["UserId"].ToString();
                refreshToken.ClientId = read["ClientId"].ToString();
                refreshToken.IssuedTime = Convert.ToDateTime(read["IssuedTime"]);
                refreshToken.ExpiredTime = Convert.ToDateTime(read["ExpiredTime"]);
                refreshToken.ProtectedTicket = read["ProtectedTicket"].ToString();

                read.Close();
                general.con.Close();

                return refreshToken;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Method called to add a refresh token to the database
        /// </summary>
        /// <param name="token"> The token object</param>
        /// <returns>Returns true if found, else false</returns>
        public static bool AddRefreshToken(RefreshToken token)
        {
            General general = new General();
            RefreshToken refreshToken = new RefreshToken();

            SqlCommand cmd = new SqlCommand("Insert into RefreshToken(Token, UserId, ClientId, IssuedTime, ExpiredTime, ProtectedTicket) values(@token, @userid, @clientid, @issuedtime, @expiredtime, @protectedticket)", general.con);
            cmd.Parameters.AddWithValue("@userid", token.UserId);
            cmd.Parameters.AddWithValue("@clientid", token.ClientId);
            cmd.Parameters.AddWithValue("@token", token.Token);
            cmd.Parameters.AddWithValue("@issuedtime", token.IssuedTime);
            cmd.Parameters.AddWithValue("@expiredtime", token.ExpiredTime);
            cmd.Parameters.AddWithValue("@protectedticket", token.ProtectedTicket);

            general.con.Open();
            int i = cmd.ExecuteNonQuery();
            general.con.Close();

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
        ///  Method called to delete a refresh token from the database using it's userid and clientid
        /// </summary>
        /// <param name="token">The token object</param>
        /// <returns>Returns true if deleted, else false</returns>
        public static bool RemoveRefreshToken(RefreshToken token)
        {
            General general = new General();

            SqlCommand cmd = new SqlCommand("Delete from RefreshToken where UserId=@userid and ClientId=@clientid", general.con);
            cmd.Parameters.AddWithValue("@userid", token.UserId);
            cmd.Parameters.AddWithValue("@clientid", token.ClientId);

            general.con.Open();
            int i = cmd.ExecuteNonQuery();
            general.con.Close();

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
        ///  Method called to delete a refresh token from the database using it's id
        /// </summary>
        /// <param name="token">The token id</param>
        /// <returns>Returns true if deleted, else false</returns>
        public static bool RemoveRefreshTokenByID(string token)
        {
            General general = new General();

            SqlCommand cmd = new SqlCommand("Delete from RefreshToken where Token=@token", general.con);
            cmd.Parameters.AddWithValue("@token", token);

            general.con.Open();
            int i = cmd.ExecuteNonQuery();
            general.con.Close();

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
        /// Converts a base64 string to UTF8
        /// </summary>
        /// <param name="base64">Base64 string to be converted</param>
        /// <returns>UTF8 string</returns>
        public static string ConvertFromBase64(string base64)
        {
            try
            {
                var ByteCode = Convert.FromBase64String(base64);
                return Encoding.UTF8.GetString(ByteCode);
            }
           catch(Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Converts a UTF8 string to base64
        /// </summary>
        /// <param name="value">String to be converted</param>
        /// <returns>Base64 string</returns>
        public static string ConvertToBase64(string value)
        {
            try
            {
                byte[] toEncodeAsBytes = ASCIIEncoding.ASCII.GetBytes(value);
                return Convert.ToBase64String(toEncodeAsBytes);
            }
            catch (Exception)
            {
                return value;
            }
        }
    }
}