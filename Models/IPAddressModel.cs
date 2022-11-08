using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GOChatAPI.Models
{
    public class IPAddressModel
    {
        public string UserId { get; set; }
        public string IPv4 { get; set; }
        public string City { get; set; }
        public string Country_code { get; set; }
        public string Country_name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string State { get; set; }
    }
}