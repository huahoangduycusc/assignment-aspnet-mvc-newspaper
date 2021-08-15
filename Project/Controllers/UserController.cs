using Project.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;
using Rotativa;
using System.Data.Entity.SqlServer;

namespace Project.Controllers
{
    public class UserController : Controller
    {
        NewspaperEntities db = new NewspaperEntities();
        // GET: User
        public ActionResult Index(int id = -1)
        {
            var result = db.Users.SingleOrDefault(m => m.user_id == id);
            if(result == null)
            {
                return RedirectToAction("Index", "Home", "");
            }
            else
            {
                TempData["countPost"] = db.Comments.Where(m => m.user_id == id).Count();
                TempData["CountFollow"] = db.follows.Where(m => m.to_user == id).Count();
                return View(result);
            }
        }
        // view profile and commnet
        public ActionResult viewProfile(int id_user,int skip = 0)
        {
            System.Threading.Thread.Sleep(1500);
            var result = (from u in db.Users
                          join c in db.Comments on u.user_id equals c.user_id
                          join a in db.articles on c.article_id equals a.article_id
                          where u.user_id == id_user orderby c.cmt_id descending
                          select new binhluan
                          { 
                              aid = a.article_id,
                              title = a.article_name,
                              img = u.avatar,
                              name = u.nickname,
                              uid = u.user_id,
                              day = c.created_at.ToString(),
                              msg = c.message,
                              role = u.role

                          }).Skip(skip).Take(5).ToList();
            return PartialView("postProfile", result);
        }
        // edit profile
        [Authorize]
        public ActionResult Setting(int id)
        {
            var result = db.Users.Where(m => m.user_id == id).FirstOrDefault();
            if(result != null)
            {
                if (User.IsInRole("admin") || Session["uid"] != null && Session["uid"].ToString() == result.user_id.ToString())
                {
                    return View(result);
                }
                else
                {
                    return RedirectToAction("Index", "Home", "");
                }
            }
            else
            {
                return RedirectToAction("Index", "Home", "");
            }
        }
        [HttpPost]
        public ActionResult Setting(int id, User u, HttpPostedFileBase avatar)
        {
            var result = db.Users.Where(m => m.user_id == id).FirstOrDefault();
            var allowExtension = new[] {".png",".jpg",".jpeg"};
            try
            {
                result.fullname = u.fullname;
                result.gender = u.gender;
                result.birthday = u.birthday;
                result.address = u.address;
                result.phone = u.phone;
                result.favourite = u.favourite;
                result.email = u.email;
                if(avatar!= null && avatar.ContentLength > 0)
                {
                    string extension;
                    string name;
                    extension = Path.GetExtension(avatar.FileName).ToLower();
                    if (!allowExtension.Contains(extension))
                    {
                        TempData["msg"] = "Dinh dang tep khong hop le, duoi anh phai la png, jpg, jpeg!";
                    }
                    else
                    {
                        name = DateTime.Now.ToFileTime().ToString();
                        name += extension;
                        string foldername = Path.Combine(Server.MapPath("~/images/profile"), name);
                        avatar.SaveAs(foldername);
                        result.avatar = name;
                    }
                }
                if (User.IsInRole("admin"))
                {
                    result.role = u.role;
                    result.nickname = u.nickname;
                }
                db.SaveChanges();
                ViewBag.Msg = "Update information success";
            }
            catch(Exception ex)
            {
                ViewBag.Msg = ex.Message;
            }
            return View(result);
        }
        // change password
        [Authorize]
        public ActionResult ChangePassword()
        {
            int id = (int)Session["uid"];
            var result = db.Users.Where(m => m.user_id == id).FirstOrDefault();
            return View(result);
        }
        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(string pw, string npw,string cfpw)
        {
            int id = (int)Session["uid"];
            bool flag = true;
            if (npw != cfpw)
            {
                ViewBag.Msg = "Mật khẩu mới và mật khẩu xác nhận phải giống nhau !";
                flag = false;
            }
            var result = db.Users.Where(m => m.user_id == id).FirstOrDefault();
            string ePw = encrypt(pw);
            if(result.password != ePw)
            {
                ViewBag.Msg = "Mật khẫu cũ không chính xác, vui lòng thử lại !";
                flag = false;
            }
            if (flag)
            {
                string newPass = encrypt(npw);
                result.password = newPass;
                ViewBag.Success = "Thay đổi mật khẩu mới thành công !";
                db.SaveChanges();
            }
            return View(result);
        }
        // list user
        [Authorize(Roles ="admin")]
        public ActionResult List(int? page)
        {
            var result = db.Users.OrderByDescending(m => m.user_id).ToList().ToPagedList(page ?? 1,10);
            return View(result);
        }
        // print
        public ActionResult printAllReport()
        {
            var result = db.Users.OrderByDescending(m => m.user_id).ToList();
            var report = new ViewAsPdf(result);
            return report;
        }
        [Authorize(Roles ="admin")]
        // del user
        public ActionResult DelUser(int id)
        {
            var result = db.Users.Where(m => m.user_id == id).FirstOrDefault();
            var cmts = db.Comments.Where(m => m.user_id == id).ToList();
            var likes = db.likes.Where(m => m.user_id == id).ToList();
            var topic = db.articles.Where(m => m.user_id == id).ToList();
            // del all records
            db.Comments.RemoveRange(cmts);
            db.likes.RemoveRange(likes);
            db.articles.RemoveRange(topic);
            db.Users.Remove(result);
            db.SaveChanges();
            return RedirectToAction("List","User");
        }
        // article of user
        public ActionResult Search(int id)
        {
            TempData["countOwn"] = db.articles.Where(m => m.user_id == id && m.status == 1).Count();
            var result = db.Users.Where(m => m.user_id == id).FirstOrDefault();
            if(result == null)
            {
                return RedirectToAction("Index", "Home", null);
            }
            return View(result);
        }
        public ActionResult OwnArticle(int id, int skip=0)
        {
            System.Threading.Thread.Sleep(1500);
            var result = (from a in db.articles
                          where a.status == 1 && a.user_id == id
                          orderby a.views descending
                          select new Project.Models.baiviet
                          {
                              aid = a.article_id,
                              name = a.article_name,
                              day = a.created_at.ToString(),
                              img = a.thumbnail
                          }).Skip(skip).Take(6).ToList();
            return PartialView("_OwnArticle", result);
        }
        // mã hóa password
        public string encrypt(string clearText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }
        // giải mã password
        public string Decrypt(string cipherText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
        // top comment
        public ActionResult Top()
        {
            DateTime today = DateTime.Now.Date.AddDays(-7);
            var topTuan = (from u in db.Users
                           join c in db.Comments
                           on u.user_id equals c.user_id
                           where c.created_at >= today
                           group c by new { u.user_id,u.username,u.avatar }
                           into grp
                           select new Project.Models.topUser
                           {
                               uid = grp.Key.user_id,
                               username = grp.Key.username,
                               image = grp.Key.avatar,
                               post = grp.Count()
                               
                           }).OrderByDescending(m => m.post).Take(10).ToList();
            var topThang = (from u in db.Users
                           join c in db.Comments
                           on u.user_id equals c.user_id
                           where c.created_at.Value.Month == DateTime.Now.Month
                           group c by new { u.user_id, u.username, u.avatar }
                           into grp
                           select new Project.Models.topUser
                           {
                               uid = grp.Key.user_id,
                               username = grp.Key.username,
                               image = grp.Key.avatar,
                               post = grp.Count()

                           }).OrderByDescending(m => m.post).Take(10).ToList();
            var TopEver = (from u in db.Users
                           select new Project.Models.topUser
                           {
                               uid = u.user_id,
                               username = u.username,
                               image = u.avatar,
                               post = (int)u.post
                           }).OrderByDescending(m => m.post).Take(10).ToList();
            var result = new Project.Models.allTopUser
            {
                TopTuan = topTuan,
                topThang = topThang,
                TopEver = TopEver
            };
            return View(result);
        }
        // follow doc gia
        [Authorize]
        public ActionResult Follow(int id)
        {
            int user_id = (int)Session["uid"];
            var result = db.follows.Where(m => m.to_user == id && m.from_user == user_id).FirstOrDefault();
            if(result != null) // da theo doi truoc do
            {
                db.follows.Remove(result);
            }
            else // chua theo doi
            {
                follow obj = new follow();
                obj.from_user = user_id;
                obj.to_user = id;
                db.follows.Add(obj);
            }
            db.SaveChanges();
            return RedirectToAction("Index","User",new {id=id });
        }
        // check follow or unfollow
        public ActionResult checkFollow(int id)
        {
            int user_id = (int)Session["uid"];
            var result = db.follows.Where(m => m.to_user == id && m.from_user == user_id).FirstOrDefault();
            TempData["to_user"] = id;
            return PartialView("_follow", result);
        }
        // thong bao moi
        public ActionResult demThongbao()
        {
            int user_id = (int)Session["uid"];
            var result = db.thongbaos.Where(m => m.user_id == user_id && m.seen == 0).ToList();
            return PartialView("_thongbao", result);
        }
        // danh sach thong bao moi
        public ActionResult Notifications()
        {
            int user_id = (int)Session["uid"];
            var tb = db.thongbaos.Where(m => m.user_id == user_id).OrderByDescending(m => m.tb_id).ToList();
            var fl = (from f in db.follows
                      join u in db.Users
                      on f.to_user equals u.user_id
                      where f.from_user == user_id
                      select new Project.Models.Follwer
                      {
                          nickname = u.nickname,
                          uid = (int)f.to_user,
                          avatar = u.avatar
                      }).ToList();
            var result = new Project.Models.Follow_Thongbao
            {
                follwers = fl,
                thongbaofollows = tb
            };
            db.thongbaos.Where(m => m.user_id == user_id).ToList().ForEach(m => m.seen = 1);
            db.SaveChanges();
            return View(result);
        }
        public ActionResult TopFollow()
        {
            var top2 = (from f in db.follows
                        join u in db.Users
                        on f.to_user equals u.user_id
                        group f by new {u.nickname,f.to_user,u.avatar }
                        into grp
                        select new Project.Models.TopFollow
                        {
                            nickname = grp.Key.nickname,
                            uid = (int)grp.Key.to_user,
                            avatar = grp.Key.avatar,
                            number = grp.Count()
                        }).OrderByDescending(m => m.number).Take(2);
            var top8 = (from f in db.follows
                        join u in db.Users
                        on f.to_user equals u.user_id
                        group f by new { u.nickname, f.to_user, u.avatar }
                        into grp
                        select new Project.Models.TopFollow
                        {
                            nickname = grp.Key.nickname,
                            uid = (int)grp.Key.to_user,
                            avatar = grp.Key.avatar,
                            number = grp.Count()
                        }).OrderByDescending(m => m.number).Skip(2).Take(8);
            var result = new Project.Models.ListTopFollow
            {
                top2Follow = top2,
                top8Follow = top8
            };
            return PartialView("_topFollow",result);
        }
    }
}