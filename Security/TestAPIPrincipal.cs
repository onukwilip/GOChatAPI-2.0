using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace GOChatAPI.Security
{
    public class TestAPIPrincipal : IPrincipal
    {
        //Constructor
        public TestAPIPrincipal(string userName, string ipAddress)
        {
            UserName = userName;
            IPAddress = ipAddress;
            Identity = new GenericIdentity(userName);
        }

        public string UserName { get; set; }
        public string IPAddress { get; set; }
        public IIdentity Identity { get; set; }
        public bool IsInRole(string role)
        {
            if (role.Equals("user"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}