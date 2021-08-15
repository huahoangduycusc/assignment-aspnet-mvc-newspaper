using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project.Models
{
    public class hopthu
    {
        public int mid { get; set; }
        public int from_user { get; set; }
        public int to_user { get; set; }
        public string name_from { get; set; }
        public string name_to { get; set; }
        public string avatar_from { get; set; }
        public string avatar_to { get; set; }
        public string message { get; set; }
        public DateTime created_at { get; set; }
        public int block { get; set; }
        public int seen { get; set; }
    }
}