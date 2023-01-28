using System;
using System.Collections.Generic;
using System.Text;

namespace Invoice.Generator.Models.DB
{
    public class UserDetails
    {
        public string username { get; set; }
        public string passwordHash { get; set; }
        public string salt { get; set; }
        public string _type { get; set; }
        public string userID { get; set; }
        public int iterations { get; set; }
    }

}
