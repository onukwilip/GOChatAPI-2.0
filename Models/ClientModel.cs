using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GOChatAPI.Models
{
    public class ClientModel
    {
        public int Id { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ClientName { get; set; }
        public int Active { get; set; }
        public int RefreshTokenLifeTime { get; set; }
        public string AllowOrigin { get; set; }
    }
}