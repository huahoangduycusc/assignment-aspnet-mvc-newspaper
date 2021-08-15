using Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList.Mvc;
using PagedList;

namespace Project.Controllers
{
    public class FeedbackController : Controller
    {
        NewspaperEntities db = new NewspaperEntities();
        // GET: Feedback
        public ActionResult Index()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Index(feedback obj)
        {
            string msg = "";
            if (ModelState.IsValid)
            {
                feedback fb = new feedback();
                fb.email = obj.email;
                fb.title = obj.title;
                fb.message = obj.message;
                fb.created_at = DateTime.Now;
                fb.seen = 0;
                db.feedbacks.Add(fb);
                db.SaveChanges();
                msg = "Gửi feedback thành công, cảm ơn phản hồi của bạn !";
            }
            else
            {
                msg = "Có lỗi xảy ra trong quá trình gửi Feedback.";
            }
            return Json(new { msg = msg }, JsonRequestBehavior.AllowGet);
        }
        // list feedback
        [Authorize(Roles ="admin,btv")]
        public ActionResult List(int ? page, string type = "chuaxem")
        {
            if(type == "chuaxem")
            {
                var result = db.feedbacks.Where(m => m.seen == 0).OrderByDescending(m => m.fb_id).ToList().ToPagedList(page ?? 1, 10);
                return View(result);
            }
            else
            {
                var result = db.feedbacks.Where(m => m.seen == 1).OrderByDescending(m => m.fb_id).ToList().ToPagedList(page ?? 1, 10);
                return View(result);
            }
        }
        // delete
        [Authorize(Roles = "admin,btv")]
        public ActionResult Delete(int id)
        {
            var result = db.feedbacks.Where(m => m.fb_id == id).FirstOrDefault();
            db.feedbacks.Remove(result);
            db.SaveChanges();
            return RedirectToAction("List");
        }
        // view feedback
        [Authorize(Roles = "admin,btv")]
        public ActionResult View(int id)
        {
            var result = db.feedbacks.Where(m => m.fb_id == id).FirstOrDefault();
            string fb_title = result.title;
            string fb_email = result.email;
            string fb_msg = result.message;
            string fb_time = result.created_at.ToString();
            result.seen = 1;
            db.SaveChanges();
            return Json(new {title=fb_title,email=fb_email,msg=fb_msg,dates=fb_time },JsonRequestBehavior.AllowGet);
        }
    }
}