using PagedList;
using Project.Models;
using Rotativa;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Project.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        NewspaperEntities db = new NewspaperEntities();
        // GET: Admin
        [Authorize(Roles ="admin,btv,nb,ctv")]
        public ActionResult Index()
        {
            var chuyenmuc = db.Categories.Count();
            var baiviet = db.articles.Count();
            var comment = db.Comments.Count();
            var user = db.Users.Count();
            var bqt = db.Users.Where(m => m.role != "mem").Count();
            var likes = db.likes.Count();
            var views = (int)db.articles.Sum(m => m.views);
            var blog = db.blogs.Count();
            var result = new thongke
            {
                chuyenmuc = chuyenmuc,
                baiviet = baiviet,
                binhluan = comment,
                user = user,
                bqt = bqt,
                like = likes,
                view = views,
                blog = blog
            };
            return View(result);
        }
        // create category
        [Authorize(Roles = "admin")]
        public ActionResult Category()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Category(Category c)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Categories.Add(c);
                    db.SaveChanges();
                    ViewBag.Msg = "Create a new category success !";
                }
                else
                {
                    ViewBag.Msg = "There is something wrong";
                }
            }
            catch(Exception ex)
            {
                ViewBag.Msg = ex.Message;
            }
            return View();
        }
        [Authorize(Roles = "admin")]
        // list category
        public ActionResult List(int ? page)
        {
            var result = db.Categories.ToList().ToPagedList(page ?? 1, 10); ;
            return View(result);
        }
        [Authorize(Roles = "admin")]
        // del category
        public ActionResult DelCategory(int id)
        {
            var result = db.Categories.SingleOrDefault(m => m.category_id == id);
            var topic = db.articles.Where(m => m.category_id == id).ToList();
            db.articles.RemoveRange(topic);
            db.Categories.Remove(result);
            db.SaveChanges();
            return RedirectToAction("List");
        }
        [Authorize(Roles = "admin")]
        // edit category
        public ActionResult EditCategory(int id)
        {
            var result = db.Categories.SingleOrDefault(m => m.category_id == id);
            return View(result);
        }
        [HttpPost]
        public ActionResult EditCategory(int id, Category c)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = db.Categories.SingleOrDefault(m => m.category_id == id);
                    result.category_name = c.category_name;
                    result.category_name_en = c.category_name_en;
                    db.SaveChanges();
                    ViewBag.Msg = "Change the new name of category success";
                }
                catch(Exception ex)
                {
                    ViewBag.Msg = ex.Message;
                }
            }
            return View();
        }
        [Authorize(Roles = "admin,btv,nb,ctv")]
        // list ban quan tri
        public ActionResult ListBQT(int ? page)
        {
            var result = db.Users.Where(m => m.role != "mem").ToList().ToPagedList(page ?? 1, 10);
            return View(result);
        }
        // report view
        public ActionResult Report()
        {
            ArrayList xValue = new ArrayList();
            ArrayList yValue = new ArrayList();
            var result = (from a in db.statistics
                          where a.created_at.Value.Month == DateTime.Now.Month
                          group a by a.created_at into grp
                          select new { 
                              key = grp.Key,
                              value = grp.Sum(m => m.view)
                          });
            result.ToList().ForEach(rs => xValue.Add(rs.key));
            result.ToList().ForEach(rs => yValue.Add(rs.value));
            new Chart(width: 600, height: 400, theme: ChartTheme.Green)
                .AddTitle("Thống kê lượt xem trong tháng")
                .AddSeries("Default", chartType: "Column", xValue: xValue, yValues: yValue)
                .Write("bmp");
            return null;
        }
        // thong ke luot xem bai viet theo ngay
        public ActionResult ReportView(int? page)
        {
            DateTime today = DateTime.Now.Date;
            var result = (from a in db.articles
                          join s in db.statistics
                          on a.article_id equals s.article_id
                          join u in db.Users
                          on a.user_id equals u.user_id
                          where s.created_at == today
                          select new Project.Models.TopicStatistic
                          {
                              aid = a.article_id,
                              title = a.article_name,
                              uid = (int)a.user_id,
                              username = u.nickname,
                              view = (int)s.view,
                              created_at = a.created_at.ToString()
                          }).OrderByDescending(m => m.view).ToList().ToPagedList(page ?? 1,10);
            return View(result);
        }
        // thong ke chu de duoc quan tam => comment nhieu
        public ActionResult ReportHot(int ? page)
        {
            DateTime today = DateTime.Now.Date;
            var result = (from a in db.articles
                          join s in db.Comments
                          on a.article_id equals s.article_id
                          join u in db.Users
                          on a.user_id equals u.user_id
                          where s.created_at == today
                          group s by new {a.article_id,a.article_name,u.nickname,a.user_id,a.created_at} into grp
                          select new Project.Models.TopicStatistic
                          {
                              aid = grp.Key.article_id,
                              title = grp.Key.article_name,
                              uid = (int)grp.Key.user_id,
                              username = grp.Key.nickname,
                              comment = grp.Count(),
                              created_at = grp.Key.created_at.ToString()
                          }).OrderByDescending(m => m.comment).ToList().ToPagedList(page ?? 1, 10);
            return View(result);
        }
        // bai viet hot cua thang
        public ActionResult ReportHotMonth(DateTime tu, DateTime den)
        {
            System.Threading.Thread.Sleep(1000);
            var result = (from a in db.articles
                          join s in db.Comments
                          on a.article_id equals s.article_id
                          join u in db.Users
                          on a.user_id equals u.user_id
                          where s.created_at >= tu
                          && s.created_at <= den
                          group s by new { a.article_id, a.article_name, u.nickname, a.user_id, a.created_at } into grp
                          select new Project.Models.TopicStatistic
                          {
                              aid = grp.Key.article_id,
                              title = grp.Key.article_name,
                              uid = (int)grp.Key.user_id,
                              username = grp.Key.nickname,
                              comment = grp.Count(),
                              created_at = grp.Key.created_at.ToString()
                          }).OrderByDescending(m => m.comment).ToList();
            return PartialView("_ReportHotMonth",result);
        }
        // bai viet nhieu luot xem filter
        public ActionResult ReportViewMonth(DateTime tu, DateTime den)
        {
            System.Threading.Thread.Sleep(1000);
            var result = (from a in db.articles
                          join s in db.statistics
                          on a.article_id equals s.article_id
                          join u in db.Users
                          on a.user_id equals u.user_id
                          where s.created_at >= tu
                          && s.created_at <= den
                          group s by new { a.article_id, a.article_name, u.nickname, a.user_id, a.created_at } into grp
                          select new Project.Models.TopicStatistic
                          {
                              aid = grp.Key.article_id,
                              title = grp.Key.article_name,
                              uid = (int)grp.Key.user_id,
                              username = grp.Key.nickname,
                              view = (int)grp.Sum(m =>m.view),
                              created_at = grp.Key.created_at.ToString()
                          }).OrderByDescending(m => m.view).ToList();
            return PartialView("_ReportViewFilter",result);
        }
            // print report view
            public ActionResult PrintReportView(DateTime tu, DateTime den)
        {
            var result = (from a in db.articles
                          join s in db.statistics
                          on a.article_id equals s.article_id
                          join u in db.Users
                          on a.user_id equals u.user_id
                          where s.created_at >= tu
                          && s.created_at <= den
                          group s by new { a.article_id, a.article_name, u.nickname, a.user_id, a.created_at } into grp
                          select new Project.Models.TopicStatistic
                          {
                              aid = grp.Key.article_id,
                              title = grp.Key.article_name,
                              uid = (int)grp.Key.user_id,
                              username = grp.Key.nickname,
                              view = (int)grp.Sum(m => m.view),
                              created_at = grp.Key.created_at.ToString()
                          }).OrderByDescending(m => m.view).ToList();
            var report = new ViewAsPdf(result);
            return report;
        }
        public ActionResult PrintReportHot(DateTime tu, DateTime den)
        {
            var result = (from a in db.articles
                          join s in db.Comments
                          on a.article_id equals s.article_id
                          join u in db.Users
                          on a.user_id equals u.user_id
                          where s.created_at >= tu
                          && s.created_at <= den
                          group s by new { a.article_id, a.article_name, u.nickname, a.user_id, a.created_at } into grp
                          select new Project.Models.TopicStatistic
                          {
                              aid = grp.Key.article_id,
                              title = grp.Key.article_name,
                              uid = (int)grp.Key.user_id,
                              username = grp.Key.nickname,
                              comment = grp.Count(),
                              created_at = grp.Key.created_at.ToString()
                          }).OrderByDescending(m => m.comment).ToList();
            var report = new ViewAsPdf(result);
            return report;
        }
        [Authorize(Roles = "admin")]
        // tim kiem
        public ActionResult TimKiem()
        {
            return View();
        }
        // tra ve ket qua cho partial view
        public ActionResult GetUser(string name)
        {
            System.Threading.Thread.Sleep(1000);
            var result = (from u in db.Users
                          where u.username.Contains(name)
                          select new Project.Models.nguoidung
                          {
                              uid = u.user_id,
                              username = u.username,
                              fullname = u.fullname,
                              gender = u.gender,
                              role = u.role,
                              avatar = u.avatar
                          }).ToList();
            return PartialView("_TimKiem",result);
        }

    }
}