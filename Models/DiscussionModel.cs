using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GOChatAPI.Models
{
    public class DiscussionModel
    {
        public string ChatRoomID { get; set; }
        public string ChatID { get; set; }
        public string MemberID { get; set; }
        public string ChatRoomName { get; set; }
        public string ProfilePicture { get; set; }
        public string Type { get; set; }
        public int Count { get; set; }
        public string LastMessage { get; set; }
        public DateTime LastSeen { get; set; }
        public bool IsOnline { get; set; }
    }
}