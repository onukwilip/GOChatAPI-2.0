using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GOChatAPI.Models
{
    /// <summary>
    /// Object in which Request JSON body is being mapped into
    /// </summary>
    public class RequestModel
    {
        public UserModel From { get; set; }
        public UserModel To { get; set; }
        public string Message { get; set; }
        public string From_Type { get; set; }
        public string To_Type { get; set; }
        public int ID { get; set; }
    }
}