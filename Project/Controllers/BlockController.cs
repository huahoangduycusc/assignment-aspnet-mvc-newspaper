using PagedList;
using Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Project.Controllers
{
    public class BlockController : Controller
    {

        NewspaperEntities db = new NewspaperEntities(); // data
        // block user
        [Authorize(Roles ="admin")]
        [HttpPost]
        public ActionResult Index(block obj)
        {
            var result = db.Users.Where(m => m.user_id == obj.block_user).FirstOrDefault();
            int uid = (int)Session["uid"];
            if(result == null)
            {
                return RedirectToAction("Index", "Home", null);
            }
            block b = new block();
            b.block_user = obj.block_user;
            b.block_from = uid;
            b.block_lydo = obj.block_lydo;
            b.block_time = obj.block_time;
            b.created_at = DateTime.Now;
            db.blocks.Add(b);
            db.SaveChanges();
            string msg = "Khóa thành công tài khoản, người dùng sẽ bị cấm sử dụng chức năng trên hệ thống trong thời gian chỉ định !";
            return Json(new {msg=msg },JsonRequestBehavior.AllowGet);
        }
        // display popup user bi block
        public ActionResult displayBlock()
        {
            int user_id = (int)Session["uid"]; // session
            DateTime today = DateTime.Now.Date;
            var khoa = db.blocks.Where(m => m.block_user == user_id && m.block_time >= today).OrderByDescending(m => m.block_id).FirstOrDefault();
            if (khoa != null)
            {
                return PartialView("_blockInfo",khoa);
            }
            else
            {
                return PartialView("_blockInfo",khoa);
            }
        }
        // danh sach user bi khoa
        [Authorize(Roles ="admin")]
        public ActionResult List(int ? page)
        {
            DateTime today = DateTime.Now.Date;
            var result = (from b in db.blocks
                          where b.block_time >= today
                          select new Project.Models.bannick
                          {
                              bid = b.block_id,
                              uid = (int)b.block_user,
                              uname = db.Users.Where(u => u.user_id == b.block_user).FirstOrDefault().nickname,
                              fid = (int)b.block_from,
                              fname = db.Users.Where(u => u.user_id == b.block_from).FirstOrDefault().nickname,
                              lydo = b.block_lydo,
                              today = b.block_time.ToString()
                          }).OrderByDescending(m => m.fid).ToList().ToPagedList(page ?? 1, 10);
            return View(result);
        }
        // xoa lenh cam
        [Authorize(Roles ="admin")]
        public ActionResult Delete(int id)
        {
            var result = db.blocks.Where(m => m.block_id == id).FirstOrDefault();
            db.blocks.Remove(result);
            db.SaveChanges();
            return RedirectToAction("List");
        }
    }
}