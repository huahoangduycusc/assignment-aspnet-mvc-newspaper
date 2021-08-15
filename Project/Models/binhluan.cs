using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project.Models
{
    public class binhluan
    {
        public string title { get; set; } // article name
        public int aid { get; set; } // article id
        public int id { get; set; } // id comment
        public int uid { get; set; } // user id
        public string name { get; set; } // name of user
        public string img { get; set; } // img of user
        public string day { get; set; } // tine of post created at
        public string msg { get; set; } // message text of user comment
        public string role { get; set; } // role of user
    }
    public class baocaoCmt
    {
        public int cid { get; set; } // comment id
        public int user_id { get; set; }
        public int aid { get; set; }
        public string title { get; set; }
        public string nickname { get; set; }
        public string msg { get; set; }
        public int report { get; set; }
        public string created_at { get; set; }
    }
}