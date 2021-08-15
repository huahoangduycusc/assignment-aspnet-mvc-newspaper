using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project.Models
{
    public class bangxephang
    {
        public IEnumerable<baiviet> TopDay { get; set; }
        public IEnumerable<baiviet> TopWeek { get; set; }
        public IEnumerable<baiviet> TopMonth { get; set; }
    }
}