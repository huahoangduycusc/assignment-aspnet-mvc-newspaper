using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project.Models
{
    public class baiviet
    {
        public int aid { get; set; }
        public string name { get; set; }
        public string name_en { get; set; }
        public string nickname { get; set; }
        public string content { get; set; }
        public string content_en { get; set; }
        public string img { get; set; }
        public string cate { get; set; }
        public string cate_en { get; set; }
        public string user { get; set; }
        public string day { get; set; }
        public int uid { get; set; }
        public int cid { get; set; }
        public string tags { get; set; }
        public int status { get; set; }
        public int ghim { get; set; }
        public int trend { get; set; }
        public int view { get; set; }
    }
    public class viewBlog
    {
        public int bid { get; set; } // blog id
        public string title { get; set; } // blog title
        public string title_en { get; set; }
        public string thumbnail { get; set; }
        public string msg { get; set; }
        public string msg_en { get; set; }
        public string created_at { get; set; }
        public int uid { get; set; } // user id
        public string nickname { get; set; } // nick name of user
        public string avatar { get; set; } // avatar profile of user
        public int post { get; set; }
        public string role { get; set; }
    }
}