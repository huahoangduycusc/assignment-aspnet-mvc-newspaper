using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project.Models
{
    public class chatmail
    {
        public int mid { get; set; } // mail id
        public int fuser { get; set; }
        public int tuser { get; set; }
        public string from_u { get; set; } // from user
        public string t_u { get; set; } // to user
        public string msg { get; set; } // text
        public string avatar_f { get; set; } // avatar from user
        public string avatar_t { get; set; } // avatar to user
        public string created_at { get; set; }
    }
}