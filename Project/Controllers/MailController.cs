using PagedList;
using Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Project.Controllers
{
    public class MailController : Controller
    {
        NewspaperEntities db = new NewspaperEntities();
        // GET: Mail
        public ActionResult Index()
        {
            return View();
        }
        [Authorize]
        public ActionResult Chat(int id = -1, int skip = 0)
        {
            int user_id = (int)Session["uid"]; // session
            DateTime today = DateTime.Now.Date;
            var khoa = db.blocks.Where(m => m.block_user == user_id && m.block_time > today).OrderByDescending(m => m.block_id).FirstOrDefault();
            if(khoa != null)
            {
                return Redirect("/");
            }
            TempData["to_user"] = id; // chat who with ?
            var result = db.Users.Where(m => m.user_id == id).FirstOrDefault();
            if(result == null) // khong ton tai thi chuyen huong
            {
                return RedirectToAction("Index", "Home", null);
            }
            else if(result.user_id == user_id)
            {
                return RedirectToAction("Index", "Home", null); // khong the tu nhan voi myself
            }
            TempData["chat_to"] = result.nickname;
            var mails = (from m in db.cms_contact
                         join u in db.Users
                         on m.from_user equals u.user_id
                         where m.to_user == id && m.from_user == user_id
                         || m.from_user == id && m.to_user == user_id
                         select new Project.Models.chatmail
                         {
                            mid = m.contact_id,
                            msg = m.message,
                            fuser = (int)m.from_user,
                            tuser = (int)m.to_user,
                            from_u = u.nickname,
                            t_u = u.nickname,
                            avatar_f = u.avatar,
                            avatar_t = u.avatar,
                            created_at = m.created_at.ToString()
                         }).OrderByDescending(m => m.mid).Skip(skip).Take(10).ToList();
            if(mails.Count() > 0)
            {
                db.cms_contact.Where(m => m.to_user == user_id && m.from_user == id).ToList().ForEach(m => m.seen = 1);
                db.SaveChanges();
            }
            TempData["countText"] = db.cms_contact.Where(m => m.to_user == user_id && m.from_user == id || m.from_user == user_id && m.to_user == id).Count();
            return View(mails);
        }
        [Authorize]
        [HttpPost]
        public ActionResult Chat(cms_contact m, int id = -1)
        {
            TempData.Keep("to_user");
            TempData.Keep("chat_to");
            TempData.Keep("countText");
            int user_id = (int)Session["uid"];
            int mail_id = 0; // get data parent id
            var check = (from mail in db.cms_mail
                         where mail.from_user == user_id && mail.to_user == id
                         || mail.from_user == id && mail.to_user == user_id
                         select mail).FirstOrDefault();
            if(check == null)
            {
                cms_mail mobj = new cms_mail();
                mobj.from_user = user_id;
                mobj.to_user = id;
                mobj.created_at = DateTime.Now;
                mobj.spam = 0;
                db.cms_mail.Add(mobj);
                db.SaveChanges(); // add new contact mail
                mail_id = mobj.mail_id;
            }
            else
            {
                mail_id = check.mail_id;
                check.created_at = DateTime.Now;
            }
            cms_contact obj = new cms_contact();
            obj.from_user = user_id;
            obj.to_user = m.to_user;
            obj.message = m.message;
            obj.created_at = DateTime.Now;
            obj.seen = 0;
            obj.mail_id = mail_id; // gan du lieu cua id parent
            db.cms_contact.Add(obj); // add
            //// gui thong bao den nguoi nhan tin nhan
            //string mesTb = getNickname(user_id) + " đã gửi cho bạn <a href=\"" + Url.Action("Chat", "Mail", new { id = user_id }) + "\"><b>1 tin nhắn mới</b></a>";
            //thongbao objTb = new thongbao();
            //objTb.user_id = id;
            //objTb.msg = mesTb;
            //objTb.seen = 0;
            //objTb.created_at = DateTime.Now;
            //db.thongbaos.Add(objTb); // add
            db.SaveChanges(); // save
            return RedirectToAction("Chat", "Mail", new { id = id });
        }
        // list mail to you
        public ActionResult List(int? page)
        {
            int user_id = (int)Session["uid"];
            var result = (from m in db.cms_mail
                          where m.from_user == user_id || m.to_user == user_id
                          select new Project.Models.hopthu
                          {
                              mid = m.mail_id,
                              from_user = (int)m.from_user,
                              to_user = (int)m.to_user,
                              message = db.cms_contact.OrderByDescending(a => a.contact_id).FirstOrDefault(mbox => mbox.mail_id == m.mail_id).message,
                              created_at = (DateTime)m.created_at,
                              name_from = db.Users.Where(u => u.user_id == m.from_user).FirstOrDefault().nickname,
                              name_to = db.Users.Where(u => u.user_id == m.to_user).FirstOrDefault().nickname,
                              avatar_from = db.Users.Where(u => u.user_id == m.from_user).FirstOrDefault().avatar,
                              avatar_to = db.Users.Where(u => u.user_id == m.to_user).FirstOrDefault().avatar,
                              block = (int)m.spam,
                              seen = db.cms_contact.Where(c => c.mail_id == m.mail_id && c.to_user == user_id && c.seen == 0).Count()
                          }).OrderByDescending(m => m.created_at).ToList().ToPagedList(page ?? 1, 10); ;
            return View(result);
        }
        // dem tin nhan den
        public ActionResult DemTinNhan()
        {
            int user_id = (int)Session["uid"];
            var result = db.cms_contact.Where(m => m.to_user == user_id && m.seen == 0).ToList();
            return PartialView("_newMessage",result);
        }
        // xoa tin nhan
        public ActionResult Delete(int id, string back)
        {
            int user_id = (int)Session["uid"];
            var result = db.cms_mail.Where(m => m.from_user == user_id || m.to_user == user_id && m.mail_id == id).FirstOrDefault();
            if(result != null)
            {
                db.cms_mail.Remove(result);
                db.SaveChanges();
            }
            return Redirect(back);
        }
        // get more messgae, 10 line skip
        public ActionResult getMessage(int id, int skip = 10)
        {
            System.Threading.Thread.Sleep(1000);
            int user_id = (int)Session["uid"];
            var result = (from m in db.cms_contact
                         join u in db.Users
                         on m.from_user equals u.user_id
                         where m.to_user == id && m.from_user == user_id
                         || m.from_user == id && m.to_user == user_id
                         select new Project.Models.chatmail
                         {
                             mid = m.contact_id,
                             msg = m.message,
                             fuser = (int)m.from_user,
                             tuser = (int)m.to_user,
                             from_u = u.nickname,
                             t_u = u.nickname,
                             avatar_f = u.avatar,
                             avatar_t = u.avatar,
                             created_at = m.created_at.ToString()
                         }).OrderByDescending(m => m.mid).Skip(skip).Take(10).ToList();
            return PartialView("_getMoreText",result);
        }
        // get nick name of user
        public string getNickname(int id)
        {
            string nName = "";
            var result = db.Users.Where(m => m.user_id == id).FirstOrDefault();
            if (result == null)
            {
                nName = "Unkown";
            }
            else
            {
                nName = result.nickname;
            }
            return nName;
        }
    }
}