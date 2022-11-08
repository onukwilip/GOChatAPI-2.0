using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GOChatAPI.Models
{
    public class ChatRoomModel
    {
        public string ChatRoomID { get; set; }
        public string ChatRoomName { get; set; }
        public string ProfilePicture { get; set; }
        public string Type { get; set; }
        public bool IsOnline { get; set; }
        public string Description { get; set; }
        public string ChatRoom_Owner { get; set; }
        public DateTime LastSeen { get; set; }
        public List<UserModel> Members { get; set; }
        public List<ChatsModel> Chats { get; set; }
    }
    public class ChatRoomProfileModel
    {
        public string ChatRoomID { get; set; }
        public string ChatRoomName { get; set; }
        public string ProfilePicture { get; set; }
        public string Type { get; set; }
        public string ChatRoom_Owner { get; set; }
        public int MembersCount { get; set; }
        public int ChatsCount { get; set; }
    }
}