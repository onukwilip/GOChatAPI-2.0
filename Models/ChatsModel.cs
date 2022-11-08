using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GOChatAPI.Models
{
    public class ChatsModel
    {
        public string ChatID { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public Parent Parent { get; set; }
        public Author Author { get; set; }
        public List<ChatFile> ChatFile { get; set; }
        public string ChatroomID { get; set; }
        public List<ReactionGroup> Reactions { get; set; }
    }

    public class Parent
    {
        public string ParentID { get; set; }
        public string ParentAuthor { get; set; }
        public string ParentMessage { get; set; }
    }

    public class ChatFile
    {
        public string FileName { get; set; }
        public string Path { get; set; }
        public float Size { get; set; }
    }

    public class Author
    {
        public string AuthorID { get; set; }
        public string AuthorName { get; set; }
        public string AuthorImage { get; set; }
    }
    public class ReactionGroup
    {
        public string ChatID { get; set; }
        public string ChatroomID { get; set; }
        public string ReactionID { get; set; }
        public List<Reaction> Reactions { get; set; }
    }

    public class Reaction
    {
        public string ChatID { get; set; }
        public DateTime Date { get; set; }
        public string ChatroomID { get; set; }
        public string ReactionID { get; set; }
        public DateTime DateCreated { get; set; }
        public Author Author { get; set; }
    }
}
