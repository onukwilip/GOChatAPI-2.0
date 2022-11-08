using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GOChatAPI.Models
{
    public class UserModel
    {
        public string UserID { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public bool IsOnline { get; set; }
        public string ProfilePicture { get; set; }
        public string Description { get; set; }
        public DateTime LastSeen { get; set; }
        public string OTP { get; set; }
        public object Response { get; set; }
    }

    public class ValidateUser
    {
        public bool UserExists { get; set; }
        public bool IsAuthenticated { get; set; }
    }

    public class PasswordModel
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string RetypePassword { get; set; }
    }
}