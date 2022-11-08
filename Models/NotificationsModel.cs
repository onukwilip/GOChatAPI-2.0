using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GOChatAPI.Models
{
    public class NotificationModel
    {
        public int ID { get; set; }
        public string UserID { get; set; }
        public IdentityToRenderModel IdentityToRender { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public string Target { get; set; }
        public bool Viewed { get; set; }
    }

    public class IdentityToRenderModel
    {
        public string IdentityToRenderID { get; set; }
        public string IdentityToRenderName { get; set; }
        public string IdentityToRenderProfilePicture { get; set; }
        public bool IsOnline { get; set; }
    }
}