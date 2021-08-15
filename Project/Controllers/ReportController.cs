using PagedList;
using Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList.Mvc;
using System.Web.Mvc;

namespace Project.Controllers
{
    public class ReportController : Controller
    {
        NewspaperEntities db = new NewspaperEntities();
        // GET: Report
        [Authorize]
        public ActionResult Index(int id = -1)
        {
            System.Threading.Thread.Sleep(1500);
            var result = db.Comments.Where(m => m.cmt_id == id).FirstOrDefault();
            if(result == null)
            {
                return Json(new {msg="Chúng tôi phát hiện bạn đang cố gắng hack website !"},JsonRequestBehavior.AllowGet);
            }
            else
            {
                result.report = result.report + 1;
                db.SaveChanges();
                return Json(new {msg="Report thành công, bình luận này đã được báo lên BQT !"},JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize(Roles = "admin,btv,nb,ctv")]
        // list report
        public ActionResult Warning(int? page)
        {
            var result = (from c in db.Comments
                          join a in db.articles
                          on c.article_id equals a.article_id
                          join u in db.Users
                          on c.user_id equals u.user_id
                          where c.report > 0
                          orderby c.report descending
                          select new Project.Models.baocaoCmt
                          {
                              cid = c.cmt_id,
                              user_id = u.user_id,
                              aid = a.article_id,
                              title = a.article_name,
                              nickname = u.nickname,
                              msg = c.message,
                              report = (int)c.report,
                              created_at = c.created_at.ToString()
                          }).ToList().ToPagedList(page ?? 1, 10);
            return View(result);
        }
        [Authorize(Roles = "admin,btv,nb,ctv")]
        // list all comments
        public ActionResult List(int? page)
        {
            var result = (from c in db.Comments
                          join a in db.articles
                          on c.article_id equals a.article_id
                          join u in db.Users
                          on c.user_id equals u.user_id
                          orderby c.cmt_id descending
                          select new Project.Models.baocaoCmt
                          {
                              cid = c.cmt_id,
                              user_id = u.user_id,
                              aid = a.article_id,
                              title = a.article_name,
                              nickname = u.nickname,
                              msg = c.message,
                              created_at = c.created_at.ToString()
                          }).ToList().ToPagedList(page ?? 1, 10);
            return View(result);
        }
        [Authorize(Roles = "admin,btv,nb,ctv")]
        // delete comment
        public ActionResult Delete(int id, string back)
        {
            var result = db.Comments.Where(m => m.cmt_id == id).FirstOrDefault();
            if(result == null)
            {
                RedirectToAction("Index", "Home", null);
            }
            else
            {
                db.Comments.Remove(result);
                db.SaveChanges();
            }
            return Redirect(back);
        }
    }
}