using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project.Models
{
    public class thongke
    {
        public int chuyenmuc { get; set; }
        public int baiviet { get; set; }
        public int binhluan { get; set; }
        public int user { get; set; }
        public int bqt { get; set; }
        public int like { get; set; }
        public int view { get; set; }
        public int blog { get; set; }
    }
    public class TopicStatistic
    {
        public int aid { get; set; }
        public string title { get; set; }
        public int uid { get; set; }
        public string username { get; set; }
        public int view { get; set; }
        public int comment { get; set; }
        public string created_at { get; set; }
    }
    public class multiTopicStatic
    {
        public IEnumerable<TopicStatistic> topNgay { get; set; }
        public IEnumerable<TopicStatistic> topThang { get; set; }
    }
}