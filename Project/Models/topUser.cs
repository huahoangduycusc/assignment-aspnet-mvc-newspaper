using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project.Models
{
    public class topUser
    {
        public int uid { get; set; }
        public string username { get; set; }
        public string image { get; set; }
        public int post { get; set; }
    }
    public class allTopUser
    {
        public IEnumerable<topUser> TopTuan { get; set; }
        public IEnumerable<topUser> topThang { get; set; }
        public IEnumerable<topUser> TopEver { get; set; }
    }
    public class nguoidung
    {
        public int uid { get; set; }
        public string username { get; set; }
        public string fullname { get;set; }
        public string gender { get; set; }
        public string role { get; set; }
        public string avatar { get; set; }
    }
    public class Follwer
    {
        public string nickname { get; set; }
        public int uid { get; set; }
        public string avatar { get; set; }
    }
    public class Follow_Thongbao
    {
        public IEnumerable<Follwer> follwers { get; set; }
        public IEnumerable<thongbao> thongbaofollows { get; set; }
    }
    public class TopFollow
    {
        public string nickname { get; set; }
        public int uid { get; set; }
        public string avatar { get; set; }
        public int number { get; set; }
    }
    public class ListTopFollow
    {
        public IEnumerable<TopFollow> top2Follow { get; set; }
        public IEnumerable<TopFollow> top8Follow { get; set; }
    }
}