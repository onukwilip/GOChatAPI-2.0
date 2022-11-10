using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GOChatAPI.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string ClientId { get; set; }
        public string Token { get; set; }
        public DateTime IssuedTime { get; set; }
        public DateTime ExpiredTime { get; set; }
        public string ProtectedTicket { get; set; }
    }
}